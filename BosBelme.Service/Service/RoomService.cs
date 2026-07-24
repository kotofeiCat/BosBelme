namespace BosBelme.Service.Service;

// Сервис для работы с комнатами и игровыми сессиями
public class RoomService(AppDbContext context, IHubContext<GameRoomHub> hubContext) : IRoomService
{
    // Создает новую игровую комнату и добавляет пользователя в нее
    public async Task<GameHubDto> CreateRoomAsync(int userId)
    {
        var defaultGame = await context.Games.AsNoTracking().FirstOrDefaultAsync()
            ?? throw new Exception("Игра не найдена.");

        var gameHub = new GameHub
        {
            Name = $"Комната-{String.GetRandomName()}",
            GameId = defaultGame.Id,
            ConnectionKey = String.GetRandomString(),
            Status = GameStatus.Waiting
        };

        context.GameHubs.Add(gameHub);

        var gameSession = new GameSession
        {
            GameHub = gameHub,
            PlayerId = userId,
            IsHost = true
        };

        context.GameSessions.Add(gameSession);

        await context.SaveChangesAsync();
        return gameHub.FromGameHub();
    }

    // Добавляет пользователя в существующую игровую комнату
    public async Task InviteUserToRoomAsync(string roomCode, int userId)
    {
        if (await context.GameSessions.AnyAsync(gs => gs.PlayerId == userId))
            throw new Exception($"Пользователь уже состоит в комнате.");

        var gameHub = await context.GameHubs.FirstOrDefaultAsync(gh => gh.ConnectionKey == roomCode)
            ?? throw new Exception("Комната не найдена");

        var gameSession = new GameSession
        {
            GameHubId = gameHub.Id,
            PlayerId = userId
        };

        context.GameSessions.Add(gameSession);
        await context.SaveChangesAsync();
    }

    // Метод для получения данных о комнате
    public async Task<RoomDto> GetRoomDetailsAsync(string code)
    {
        if (string.IsNullOrEmpty(code)) throw new Exception("Укажите код комнаты");

        var gameHub = await context.GameHubs
            .AsNoTracking()
            .Include(gh => gh.Game)
                .ThenInclude(gh => gh.PlayerCounts)
            .Include(gh => gh.GameSessions)
                .ThenInclude(gs => gs.Player)
            .FirstOrDefaultAsync(gh => gh.ConnectionKey == code)
            ?? throw new Exception("Такой комнаты не существует");

        var hostSession = gameHub.GameSessions.FirstOrDefault(gs => gs.IsHost)
            ?? throw new Exception("Хост в данной комнате не найден");

        var allGames = await context.Games
            .AsNoTracking()
            .Select(g => new GameSelectDto { Id = g.Id, Name = g.NameGame, Description = g.Discription, IsStrictRange = g.IsStrictRange, MaxPlayers = g.MaxPlayers, MinPlayers = g.MinPlayers })
            .ToListAsync();

        List<int> playersCounts = gameHub.Game.PlayerCounts
            .Select(pc => pc.Count)
            .ToList();

        return new RoomDto
        {
            RoomCode = gameHub.ConnectionKey,
            RoomName = gameHub.Name,
            HostName = hostSession.Player?.Name ?? "Неизвестно",
            Status = gameHub.Status.ToString(),
            GameId = gameHub.GameId,
            GameName = gameHub.Game.NameGame,
            PlayersCounts = playersCounts,
            AvailableGames = allGames,
            Players = gameHub.GameSessions
                .Select(gs => new RoomPlayerDto
                {
                    Id = gs.PlayerId,
                    Name = gs.Player.Name,
                    IsHost = gs.IsHost,
                    IsGuest = gs.Player.IsGuest,
                    IsReady = gs.IsReady
                }).ToList()
        };
    }

    // Удаляет пользователя из комнаты. Если пользователь является хостом, удаляет всю комнату и все сессии.
    public async Task<bool> LeaveRoomAsync(int userId, string roomCode)
    {
        var gameHub = await context.GameHubs
            .Include(gh => gh.GameSessions)
                .ThenInclude(gs => gs.Player)
            .FirstOrDefaultAsync(gh => gh.ConnectionKey == roomCode)
            ?? throw new Exception("Игровая комната не найдена");

        var userSession = gameHub.GameSessions.FirstOrDefault(gs => gs.PlayerId == userId)
            ?? throw new Exception("Игровая сессия не найдена");

        bool isHost = userSession.IsHost;
        bool isCallerGuest = userSession.Player?.IsGuest ?? false;

        if (isHost)
        {
            await hubContext.Clients.Group(roomCode).SendAsync("RoomDelete");

            var guestSessions = gameHub.GameSessions.Where(gs => gs.Player != null && gs.Player.IsGuest).ToList();
            foreach (var gs in guestSessions)
            {
                context.Remove(gs.Player);
            }

            context.GameSessions.RemoveRange(gameHub.GameSessions);
            context.GameHubs.Remove(gameHub);
        }
        else
        {
            if (userSession.Player != null && userSession.Player.IsGuest)
            {
                context.Remove(userSession.Player);
            }

            context.GameSessions.Remove(userSession);
        }

        await context.SaveChangesAsync();

        if (!isHost)
        {
            try
            {
                var updatedRoom = await GetRoomDetailsAsync(roomCode);
                await hubContext.Clients.Group(roomCode).SendAsync("UpdateRoom", updatedRoom);
            }
            catch { }
        }

        return isCallerGuest;
    }

    // Удаляет комнату и все связанные сессии по коду комнаты (Код нигде не используется)
    public async Task DeleteRoomAsync(string roomCode)
    {
        var gameHub = await context.GameHubs
            .Include(gh => gh.GameSessions)
                .ThenInclude(gs => gs.Player)
            .FirstOrDefaultAsync(gh => gh.ConnectionKey == roomCode)
            ?? throw new Exception("Игровой хаб не найден");

        if (gameHub != null)
        {
            var guestSessions = gameHub.GameSessions.Where(gs => gs.Player != null && gs.Player.IsGuest).ToList();
            foreach (var gs in guestSessions)
            {
                context.Remove(gs.Player);
            }

            context.GameSessions.RemoveRange(gameHub.GameSessions);
            context.GameHubs.Remove(gameHub);

            await context.SaveChangesAsync();
        }
    }

    // Метод смены игры
    public async Task ChangeGameAsync(string roomCode, int gameId, int userId)
    {
        var hub = await context.GameHubs
            .Include(gh => gh.GameSessions)
            .FirstOrDefaultAsync(gh => gh.ConnectionKey == roomCode)
            ?? throw new Exception("Комната не найдена");

        var userSession = hub.GameSessions.FirstOrDefault(gs => gs.PlayerId == userId);
        if (userSession == null || !userSession.IsHost)
            throw new Exception("Только хост может менять игру");

        var game = await context.Games.FindAsync(gameId)
            ?? throw new Exception("Игра не найдена");

        hub.GameId = game.Id;
        await context.SaveChangesAsync();
    }

    // Метод готовности игрока
    public async Task ToggleReadyAsync(string roomCode, int userId)
    {
        var session = await context.GameSessions
            .FirstOrDefaultAsync(gs => gs.GameHub.ConnectionKey == roomCode && gs.PlayerId == userId)
            ?? throw new Exception("Сессия не найдена");


        session.IsReady = !session.IsReady;
        await context.SaveChangesAsync();
    }

    // Метод старта игры
    public async Task StartGameAsync(string roomCode, int userId)
    {
        var hub = await context.GameHubs
            .Include(gh => gh.Game)
                .ThenInclude(pc => pc.PlayerCounts)
            .Include(gh => gh.GameSessions)
            .FirstOrDefaultAsync(gh => gh.ConnectionKey == roomCode)
            ?? throw new Exception("Комната не найдена");

        var userSession = hub.GameSessions.FirstOrDefault(gs => gs.PlayerId == userId);
        if (userSession == null || !userSession.IsHost)
            throw new Exception("Только хост может начать игру");

        int playersCount = hub.GameSessions.Count;

        if (!hub.Game.IsStrictRange)
        {
            bool isCountAllowed = hub.Game.PlayerCounts.Any(pc => pc.Count == playersCount);

            if (!isCountAllowed)
                throw new Exception("Невозможно запустить, нет нужного числа игроков");
        }
        else
        {
            int min = hub.Game.MinPlayers ?? 0;
            int max = hub.Game.MaxPlayers ?? int.MaxValue;

            if (playersCount < hub.Game.MinPlayers)
                throw new Exception("Невозможно запустить, игроков слишком мало");

            if (playersCount > hub.Game.MaxPlayers)
                throw new Exception("Невозможно запустить, игроков слишком много");
        }



        var ordinaryPlayers = hub.GameSessions.Where(gs => !gs.IsHost);
        if (ordinaryPlayers.Any(gs => !gs.IsReady))
            throw new Exception("Не все игроки готовы!");

        hub.Status = GameStatus.Playing;
        hub.StartedAt = DateTime.UtcNow;

        await context.SaveChangesAsync();
    }

    // Проверяет находится ли игрок в комнате
    public async Task<bool> IsInRoom(int userId)
    {
        return await context.GameSessions.AnyAsync(gs => gs.PlayerId == userId);
    }

    // Возвращает код комнаты по Id игрока
    public async Task<string?> RoomCode(int userId)
    {
        var gameSession = await context.GameSessions
            .AsNoTracking()
            .Include(gs => gs.GameHub)
            .FirstOrDefaultAsync(gs => gs.PlayerId == userId);

        return gameSession?.GameHub?.ConnectionKey;
    }
}

