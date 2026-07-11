using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Internal;
using System.Security.Claims;

namespace BosBelme.Controllers
{
    // Контроллер для управления игровыми комнатами и взаимодействия с пользователями.
    public class HubController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IRegService _registeredServices;
        private readonly IRoomService _roomService;

        public HubController(AppDbContext context, IRegService registeredServices, IRoomService roomService)
        {
            _context = context;
            _registeredServices = registeredServices;
            _roomService = roomService;
        }

        // Отображает главную страницу контроллера.

        public IActionResult Index() => View();

        public IActionResult JoinRoom() => View();

        public async Task<IActionResult> CreateRoom()
        {
            if(User?.Identity?.IsAuthenticated == true)
            {
                int userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");

                var gameHub = await _roomService.CreateRoomAsync(userId);

                return RedirectToAction("Room", new { code = gameHub.ConnectionKey });
            }


            return View();
        }


        // POST: Создает новую игровую комнату и перенаправляет пользователя в нее.
        [HttpPost]
        public async Task<IActionResult> CreateRoom(CreateRoomViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var user = await _registeredServices.RegistrationTempUserAsync(model.PlayerName);

            var gameHub = await _roomService.CreateRoomAsync(user.Id);

            return RedirectToAction("Room", new {code = gameHub.ConnectionKey});
        }

        // POST: Позволяет пользователю присоединиться к существующей игровой комнате по коду.
        [HttpPost]
        public async Task<IActionResult> JoinRoom(JoinRoomCodeViewModel model)
        {
            return View();
        }

        // GET: Отображает страницу игровой комнаты по коду.
        public async Task<IActionResult> Room(string code)
        {
            if (string.IsNullOrEmpty(code)) return RedirectToAction("Index");

            var gameHub = await _context.GameHubs.Include(gh => gh.GameSessions)
                .ThenInclude(gs => gs.Player)
                .FirstOrDefaultAsync(gh => gh.ConnectionKey == code);

            if (gameHub == null) return RedirectToAction("Index");

            var hostSession = gameHub.GameSessions.FirstOrDefault(gs => gs.IsHost);
            string hostName = hostSession?.Player?.Name ?? "Не назначен";

            var model = new RoomViewModel
            {
                RoomCode = gameHub.ConnectionKey,
                RoomName = gameHub.Name,
                HostName = hostName,
                Status = gameHub.Status.ToString(),

                Players = gameHub.GameSessions
                    .Select(gs => new RoomPlayerViewModel
                    {
                        Name = gs.Player?.Name ?? "Неизвестный",
                        IsHost = gs.IsHost,
                        IsGuest = gs.Player?.IsGuest ?? true
                    })
                    .ToList()
            };

            ViewData["Tittle"] = $"Комната {gameHub.Name}";

            return View(model);
        }
    }
}
