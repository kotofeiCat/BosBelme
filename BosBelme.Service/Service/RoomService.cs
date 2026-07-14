using System;
namespace BosBelme.Service.Service
{
    // Сервис для работы с комнатами и игровыми сессиями
    public class RoomService : IRoomService
    {
        private readonly AppDbContext _context;
        private readonly IHubContext<GameRoomHub> _hubContext;

        public RoomService(AppDbContext context, IHubContext<GameRoomHub> hubContext)
        {
            _context = context;
            _hubContext = hubContext;
        }

        // Создает новую игровую комнату и добавляет пользователя в нее
        public async Task<GameHubDto> CreateRoomAsync(int userId)
        {
            var defaultGame = await _context.Games.AsNoTracking().FirstOrDefaultAsync()
                ?? throw new Exception("Игра не найдена.");

            var gameHub = new GameHub
            {
                Name = $"Комната-{String.GetRandomName()}",
                GameId = defaultGame.Id,
                ConnectionKey = String.GetRandomString(),
                Status = GameStatus.Waiting
            };

            _context.GameHubs.Add(gameHub);

            var gameSession = new GameSession
            {
                GameHub = gameHub,
                PlayerId = userId,
                IsHost = true
            };

            _context.GameSessions.Add(gameSession);

            await _context.SaveChangesAsync();
            return gameHub.FromGameHub();
        }

        // Добавляет пользователя в существующую игровую комнату
        public async Task InviteUserToRoomAsync(string roomCode, int userId)
        {
            if (await _context.GameSessions.AnyAsync(gs => gs.PlayerId == userId))
                throw new Exception($"Пользователь уже состоит в комнате.");

            var gameHub = await _context.GameHubs.FirstOrDefaultAsync(gh => gh.ConnectionKey == roomCode)
                ?? throw new Exception("Комната не найдена");

            var gameSession = new GameSession
            {
                GameHubId = gameHub.Id,
                PlayerId = userId
            };

            _context.GameSessions.Add(gameSession);
            await _context.SaveChangesAsync();
        }

        // Метод для получения данных о комнате
        public async Task<RoomDto> GetRoomDetailsAsync(string code)
        {
            if (string.IsNullOrEmpty(code)) throw new Exception("Укажите код комнаты");

            var gameHub = await _context.GameHubs
                .AsNoTracking()
                .Include(gh => gh.Game) 
                .Include(gh => gh.GameSessions)
                    .ThenInclude(gs => gs.Player)
                .FirstOrDefaultAsync(gh => gh.ConnectionKey == code)
                ?? throw new Exception("Такой комнаты не существует");

            var hostSession = gameHub.GameSessions.FirstOrDefault(gs => gs.IsHost)
                ?? throw new Exception("Хост в данной комнате не найден");

            var allGames = await _context.Games
                .AsNoTracking()
                .Select(g => new GameSelectDto { Id = g.Id, Name = g.NameGame })
                .ToListAsync();

            return new RoomDto
            {
                RoomCode = gameHub.ConnectionKey,
                RoomName = gameHub.Name,
                HostName = hostSession.Player?.Name ?? "Неизвестно",
                Status = gameHub.Status.ToString(),
                GameId = gameHub.GameId,
                GameName = gameHub.Game.NameGame,
                MinPlayers = gameHub.Game.MinPlayers,
                MaxPlayers = gameHub.Game.MaxPlayers,
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
        public async Task LeaveRoomAsync(int userId, string roomCode)
        {
            var gameHub = await _context.GameHubs
                .Include(gs => gs.GameSessions)
                    .ThenInclude(gs => gs.Player)
                .FirstOrDefaultAsync(gh => gh.ConnectionKey == roomCode)
                ?? throw new Exception("Игровая комната не найдена");

            var userSession = gameHub.GameSessions.FirstOrDefault(gs => gs.PlayerId == userId)
                ?? throw new Exception("Игровая сессия не найдена");

            if (userSession.IsHost)
            {
                var guestSessions = gameHub.GameSessions.Where(gs => gs.Player != null && gs.Player.IsGuest).ToList();
                foreach (var gs in guestSessions)
                {
                    _context.Remove(gs.Player);
                }

                _context.GameSessions.RemoveRange(gameHub.GameSessions);
                _context.GameHubs.Remove(gameHub);

            }
            else
            {
                if (userSession.Player != null && userSession.Player.IsGuest)
                {
                    _context.Remove(userSession.Player);
                }

                _context.GameSessions.Remove(userSession);

            }

            await _context.SaveChangesAsync();

            if (userSession.IsHost)
            {
                await _hubContext.Clients.Group(roomCode).SendAsync("RoomDelete");
            }
            else
            {
                var updatedRoom = await GetRoomDetailsAsync(roomCode);

                await _hubContext.Clients.Group(roomCode).SendAsync("UpdateRoom", updatedRoom);
            }
        }

        // Удаляет комнату и все связанные сессии по коду комнаты
        public async Task DeleteRoomAsync(string roomCode)
        {
            var gameHub = await _context.GameHubs
                .Include(gh => gh.GameSessions)
                    .ThenInclude(gs => gs.Player)
                .FirstOrDefaultAsync(gh => gh.ConnectionKey == roomCode);

            if (gameHub != null)
            {
                var guestSessions = gameHub.GameSessions.Where(gs => gs.Player != null && gs.Player.IsGuest).ToList();
                foreach (var gs in guestSessions)
                {
                    _context.Remove(gs.Player);
                }

                _context.GameSessions.RemoveRange(gameHub.GameSessions);
                _context.GameHubs.Remove(gameHub);

                await _context.SaveChangesAsync();
            }
        }

        //Метод смены игры
        public async Task ChangeGameAsync(string roomCode, int gameId, int userId)
        {
            var hub = await _context.GameHubs
                .Include(gh => gh.GameSessions)
                .FirstOrDefaultAsync(gh => gh.ConnectionKey == roomCode)
                ?? throw new Exception("Комната не найдена");

            var userSession = hub.GameSessions.FirstOrDefault(gs => gs.PlayerId == userId);
            if (userSession == null || !userSession.IsHost)
                throw new Exception("Только хост может менять игру");

            var game = await _context.Games.FindAsync(gameId)
                ?? throw new Exception("Игра не найдена");

            hub.GameId = game.Id;
            await _context.SaveChangesAsync();
        }

        // Метод готовности игрока
        public async Task ToggleReadyAsync(string roomCode, int userId)
        {
            var session = await _context.GameSessions
                .FirstOrDefaultAsync(gs => gs.GameHub.ConnectionKey == roomCode && gs.PlayerId == userId)
                ?? throw new Exception("Сессия не найдена");


            session.IsReady = !session.IsReady;
            await _context.SaveChangesAsync();
        }

        // Метод старта игры
        public async Task StartGameAsync(string roomCode, int userId)
        {
            var hub = await _context.GameHubs
                .Include(gh => gh.Game)
                .Include(gh => gh.GameSessions)
                .FirstOrDefaultAsync(gh => gh.ConnectionKey == roomCode)
                ?? throw new Exception("Комната не найдена");

            var userSession = hub.GameSessions.FirstOrDefault(gs => gs.PlayerId == userId);
            if (userSession == null || !userSession.IsHost)
                throw new Exception("Только хост может начать игру");

            int playersCount = hub.GameSessions.Count;

            if (playersCount < hub.Game.MinPlayers)
                throw new Exception($"Недостаточно игроков! Миномум для этой игры: {hub.Game.MinPlayers}");

            if (playersCount > hub.Game.MaxPlayers)
                throw new Exception($"Слишком много игроков! Максимум для этой игры: {hub.Game.MaxPlayers}");

            var ordinaryPlayers = hub.GameSessions.Where(gs => !gs.IsHost);
            if (ordinaryPlayers.Any(gs => !gs.IsReady))
                throw new Exception("Не все игроки готовы!");

            hub.Status = GameStatus.Playing;
            hub.StartedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
        }
    }
}
