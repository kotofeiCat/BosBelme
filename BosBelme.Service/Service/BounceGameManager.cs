namespace BosBelme.Service.Service;
// Класс менеджер игры
public class BounceGameManager(IHubContext<BounceHub> hubContext, IServiceProvider serviceProvider) : IBounceGameManager
{
    private readonly ConcurrentDictionary<string, GameSessionInstance> _sessions = new();
    private readonly ConcurrentDictionary<string, string> _playerToRoomMap = new();
    private readonly ConcurrentDictionary<string, Vector2> _playerMoveDirections = new();
    private readonly Lock _managerLock = new();

    private record GameSessionInstance(
        EngineGameSession Session,
        CancellationTokenSource Cts,
        SemaphoreSlim Semaphore,
        Task LoopTask
    );

    public async Task<EngineGameSession?> JoinOrCreateSessionAsync(string roomId, string playerId, string playerName)
    {
        lock (_managerLock)
        {
            if (_sessions.TryGetValue(roomId, out var instance))
            {
                var session = instance.Session;

                if (session.State.Player1?.Id == playerId || (session.State.Player1?.Name == playerName && playerName != "Гость"))
                {
                    _playerToRoomMap.TryRemove(session.State.Player1.Id, out _);
                    session.State.Player1.Id = playerId;
                    _playerToRoomMap[playerId] = roomId;
                    return session;
                }

                if (session.State.Player2?.Id == playerId || (session.State.Player2?.Name == playerName && playerName != "Гость"))
                {
                    _playerToRoomMap.TryRemove(session.State.Player2.Id, out _);
                    session.State.Player2.Id = playerId;
                    _playerToRoomMap[playerId] = roomId;
                    return session;
                }

                if (session.State.Player2 is null && session.State.Player1?.Id != playerId)
                {
                    session.State.Player2 = new Player
                    {
                        Id = playerId,
                        Name = playerName,
                        IsAlive = true,
                        Position = session.State.CurrentMap.Player2SpawnPoint
                    };

                    session.StartNewRound();
                    StartGameLoop(roomId, instance);
                }

                _playerToRoomMap[playerId] = roomId;
                return session;
            }
        }

        using (var scope = serviceProvider.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var roomExists = await dbContext.GameHubs.AnyAsync(gh => gh.ConnectionKey == roomId);
            if (!roomExists) return null;
        }

        lock (_managerLock)
        {
            if (_sessions.TryGetValue(roomId, out var existingInstance))
            {
                _playerToRoomMap[playerId] = roomId;
                return existingInstance.Session;
            }

            var newSession = new EngineGameSession(roomId);
            newSession.State.Player1 = new Player
            {
                Id = playerId,
                Name = playerName,
                IsAlive = true
            };

            InitializeDefaultMap(newSession.State.CurrentMap);

            newSession.State.Player1.Position = newSession.State.CurrentMap.Player1SpawnPoint;

            var cts = new CancellationTokenSource();
            var semaphore = new SemaphoreSlim(1, 1);

            var newInstance = new GameSessionInstance(
                newSession,
                cts,
                semaphore,
                Task.CompletedTask
            );

            _sessions[roomId] = newInstance;
            _playerToRoomMap[playerId] = roomId;
            return newSession;
        }
    }

    public async Task UpdatePlayerInputAsync(string roomId, string playerId, Vector2 direction)
    {
        if (_sessions.TryGetValue(roomId, out var instance))
        {
            await instance.Semaphore.WaitAsync();
            try
            {
                var player = instance.Session.State.Player1?.Id == playerId
                    ? instance.Session.State.Player1
                    : instance.Session.State.Player2;

                if (player is { IsAlive: true })
                {
                    player.RotationAngle = direction != Vector2.Zero ? MathF.Atan2(direction.Y, direction.X) : player.RotationAngle;
                    _playerMoveDirections[playerId] = direction;
                }
            }
            finally
            {
                instance.Semaphore.Release();
            }
        }
    }

    public async Task HandleShootAsync(string roomId, string playerId, float angle)
    {
        if (_sessions.TryGetValue(roomId, out var instance))
        {
            await instance.Semaphore.WaitAsync();
            try
            {
                instance.Session.HandleShoot(playerId, angle);
            }
            finally
            {
                instance.Semaphore.Release();
            }
        }
    }

    public async Task ActivateShieldAsync(string roomId, string playerId)
    {
        if (_sessions.TryGetValue(roomId, out var instance))
        {
            await instance.Semaphore.WaitAsync();
            try
            {
                instance.Session.ActivateShield(playerId);
            }
            finally
            {
                instance.Semaphore.Release();
            }
        }
    }

    public async Task<string?> HandlePlayerDisconnectAsync(string playerId)
    {
        if (_playerToRoomMap.TryRemove(playerId, out var affectedRoomId))
        {
            _ = Task.Run(async () =>
            {
                await Task.Delay(TimeSpan.FromSeconds(5));

                if (!_playerToRoomMap.TryGetKey(affectedRoomId, out _))
                {
                    RemoveSession(affectedRoomId);
                }
            });

            return affectedRoomId;
        }
        return null;
    }

    public void RemoveSession(string roomId)
    {
        if (_sessions.TryRemove(roomId, out var instance))
        {
            instance.Cts.Cancel();

            if (instance.Session.State.Player1 is not null) _playerToRoomMap.TryRemove(instance.Session.State.Player1.Id, out _);
            if (instance.Session.State.Player2 is not null) _playerToRoomMap.TryRemove(instance.Session.State.Player2.Id, out _);

            if (instance.Session.State.Player1 is not null) _playerMoveDirections.TryRemove(instance.Session.State.Player1.Id, out _);
            if (instance.Session.State.Player2 is not null) _playerMoveDirections.TryRemove(instance.Session.State.Player2.Id, out _);

            instance.Semaphore.Dispose();
            instance.Cts.Dispose();

            _ = Task.Run(async () =>
            {
                try
                {
                    using var scope = serviceProvider.CreateScope();
                    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                    var hubEntity = await dbContext.GameHubs.FirstOrDefaultAsync(gh => gh.ConnectionKey == roomId);
                    if (hubEntity != null)
                    {
                        hubEntity.Status = GameStatus.Waiting;
                        await dbContext.SaveChangesAsync();
                    }
                }
                catch (Exception) { }
            });
        }
    }

    private void StartGameLoop(string roomId, GameSessionInstance instance)
    {
        var cts = instance.Cts;
        var runningTask = Task.Run(() => RunGameLoopAsync(roomId, instance, cts.Token), cts.Token);
        _sessions[roomId] = instance with { LoopTask = runningTask };
    }

    private async Task RunGameLoopAsync(string roomId, GameSessionInstance instance, CancellationToken ct)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromMilliseconds(16));
        var lastTime = DateTime.UtcNow;
        var session = instance.Session;

        try
        {
            while (await timer.WaitForNextTickAsync(ct))
            {
                var now = DateTime.UtcNow;
                var deltaTime = (float)(now - lastTime).TotalSeconds;
                lastTime = now;

                if (deltaTime > 0.1f) deltaTime = 0.016f;

                await instance.Semaphore.WaitAsync(ct);
                try
                {
                    if (session.State.Player1 != null && _playerMoveDirections.TryGetValue(session.State.Player1.Id, out var dir1))
                        session.MovePlayer(session.State.Player1.Id, dir1, deltaTime);

                    if (session.State.Player2 != null && _playerMoveDirections.TryGetValue(session.State.Player2.Id, out var dir2))
                        session.MovePlayer(session.State.Player2.Id, dir2, deltaTime);

                    session.Update(deltaTime);

                    await hubContext.Clients.Group(roomId).SendAsync("UpdateState", session.State, cancellationToken: ct);

                    if (session.State.Status == MatchStatus.MatchOver)
                    {
                        await hubContext.Clients.Group(roomId).SendAsync("GameOver", session.State.Scores, cancellationToken: ct);
                        break;
                    }
                }
                finally
                {
                    instance.Semaphore.Release();
                }
            }
        }
        catch (OperationCanceledException) { }
        finally
        {
            RemoveSession(roomId);
        }
    }

    private static void InitializeDefaultMap(GameMap map)
    {
        int cols = 20;
        int rows = 15;

        map.Grid = new BlockType[cols][];

        for (int c = 0; c < cols; c++)
        {
            map.Grid[c] = new BlockType[rows];
            for (int r = 0; r < rows; r++)
            {
                if (c == 0 || c == cols - 1 || r == 0 || r == rows - 1)
                {
                    map.Grid[c][r] = BlockType.Wall;
                }
                else if ((c == 6 || c == 13) && (r > 3 && r < 11))
                {
                    map.Grid[c][r] = BlockType.Destructible;
                }
                else
                {
                    map.Grid[c][r] = BlockType.Empty;
                }
            }
        }

        map.Player1SpawnPoint = new Vector2(120f, 300f);
        map.Player2SpawnPoint = new Vector2(680f, 300f);
    }
}