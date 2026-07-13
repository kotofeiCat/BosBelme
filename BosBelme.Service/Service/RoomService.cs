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
                .Include(gh => gh.GameSessions)
                    .ThenInclude(gs => gs.Player)
                .FirstOrDefaultAsync(gh => gh.ConnectionKey == code);

            if (gameHub == null) throw new Exception("Такой комнаты не существует");

            var hostSession = gameHub.GameSessions.FirstOrDefault(gs => gs.IsHost)
                ?? throw new Exception("Хост в данной комнате не найден");

            var hostName = hostSession.Player?.Name
                ?? throw new Exception("Хост не найден");

            return new RoomDto
            {
                RoomCode = gameHub.ConnectionKey,
                RoomName = gameHub.Name,
                HostName = hostName,
                Status = gameHub.Status.ToString(),

                Players = gameHub.GameSessions
                    .Select(gs => new RoomPlayerDto
                    {
                        Name = gs.Player.Name,
                        IsHost = gs.IsHost,
                        IsGuest = gs.Player.IsGuest
                    })
                    .ToList()
            };
        }

        // Удаляет пользователя из комнаты. Если пользователь является хостом, удаляет всю комнату и все сессии.
        public async Task LeaveRoomAsync(int userId, string roomCode)
        {
            var gameHub = await _context.GameHubs
                .Include(gs => gs.GameSessions)
                .FirstOrDefaultAsync(gh => gh.ConnectionKey == roomCode)
                ?? throw new Exception("Игровая комната не найдена");

            var userSession = gameHub.GameSessions.FirstOrDefault(gs => gs.PlayerId == userId)
                ?? throw new Exception("Игровая сессия не найдена");

            if (userSession.IsHost)
            {
                _context.GameSessions.RemoveRange(gameHub.GameSessions);
                _context.GameHubs.Remove(gameHub);

                await _hubContext.Clients.Group(roomCode).SendAsync("RoomDelete");
            }
            else
            {
                _context.GameSessions.Remove(userSession);

                var updatedRoom = await GetRoomDetailsAsync(roomCode);

                await _hubContext.Clients.Group(roomCode).SendAsync("UpdateRoom", updatedRoom);
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
                .Include(gs => gs.GameSessions)
                .FirstOrDefaultAsync(gh => gh.ConnectionKey == roomCode);

            if (gameHub != null)
            {
                _context.GameSessions.RemoveRange(gameHub.GameSessions);
                _context.GameHubs.Remove(gameHub);

                await _context.SaveChangesAsync();
            }
        }
    }
}
