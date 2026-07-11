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
            var defaultGame = await _context.Games.FirstOrDefaultAsync() ?? throw new GameNotFoundException("Игра не найдена.");

            var gameHub = new GameHub
            {
                Name = $"Комната-{String.GetRandomName()}",
                GameId = defaultGame.Id,
                ConnectionKey = String.GetRandomString()
            };

            await _context.GameHubs.AddAsync(gameHub);

            var gameSession = new GameSession
            {
                GameHub = gameHub,
                IdPlayer = userId
            };

            await _context.GameSessions.AddAsync(gameSession);


            await _context.SaveChangesAsync();
            return gameHub;
        }


        // Приглашает пользователя в существующую игровую комнату
        public async Task InviteUserToRoomAsync(int gameHubId, int userId)
        {
            if (await _context.GameSessions.AnyAsync(gs => gs.IdPlayer == userId))
                throw new UserAlreadyInRoomException($"Пользователь уже состоит в комнате.");

            var gameSession = new GameSession
            {
                GameHubId = gameHubId,
                IdPlayer = userId
            };

            await _context.GameSessions.AddAsync(gameSession);
            await _context.SaveChangesAsync();
        }

        public async Task<RoomDto?> GetRoomDetailsAsync(string code)
        {
            if (string.IsNullOrEmpty(code)) return null;

            var gameHub = await _context.GameHubs
                .AsNoTracking()
                .Include(gh => gh.GameSessions)
                    .ThenInclude(gs => gs.Player)
                .FirstOrDefaultAsync(gh => gh.ConnectionKey == code);

            if (gameHub == null) return null;

            var hostSession = gameHub.GameSessions.FirstOrDefault(gs => gs.IsHost);
            string hostName = hostSession?.Player?.Name ?? "Не назначен";

            return new RoomDto
            {
                RoomCode = gameHub.ConnectionKey,
                RoomName = gameHub.Name,
                HostName = hostName,
                Status = gameHub.Status.ToString(),

                Players = gameHub.GameSessions
                    .Select(gs => new RoomPlayerDto
                    {
                        Name = gs.Player?.Name ?? "Неизвестный",
                        IsHost = gs.IsHost,
                        IsGuest = gs.Player?.IsGuest ?? true
                    })
                    .ToList()
            };
        }
    }
}
