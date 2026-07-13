using System;
namespace BosBelme.Service.Service
{
    // Сервис для работы с комнатами и игровыми сессиями
    public class RoomService : IRoomService
    {
        private readonly AppDbContext _context;

        public RoomService(AppDbContext context)
        {
            _context = context;
        }

        // Создает новую игровую комнату и добавляет пользователя в нее
        public async Task<GameHub> CreateRoomAsync(int userId)
        {
            var defaultGame = await _context.Games.FirstOrDefaultAsync()
                ?? throw new Exception("Игра не найдена.");

            var gameHub = new GameHub
            {
                Name = $"Комната-{String.GetRandomName()}",
                GameId = defaultGame.Id,
                ConnectionKey = String.GetRandomString()
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
            return gameHub;
        }

        // Добавляет пользователя в существующую игровую комнату
        public async Task InviteUserToRoomAsync(int gameHubId, int userId)
        {
            if (await _context.GameSessions.AnyAsync(gs => gs.PlayerId == userId))
                throw new Exception($"Пользователь уже состоит в комнате.");

            var gameSession = new GameSession
            {
                GameHubId = gameHubId,
                PlayerId = userId
            };

            await _context.GameSessions.AddAsync(gameSession);
            await _context.SaveChangesAsync();
        }

        // Возвращает информацию о комнате
        public async Task<RoomDto> GetRoomDetailsAsync(string code)
        {
            if (string.IsNullOrEmpty(code)) throw new Exception("Укажите код комнаты");

            var gameHub = await _context.GameHubs
                .AsNoTracking()
                .Include(gh => gh.GameSessions)
                    .ThenInclude(gs => gs.Player)
                .FirstOrDefaultAsync(gh => gh.ConnectionKey == code);

            if (gameHub == null) throw new Exception("Такой комнаты не существует");

            var hostSession = gameHub.GameSessions.First(gs => gs.IsHost);
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
    }
}
