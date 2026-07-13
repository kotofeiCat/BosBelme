using System.Security.Claims;

namespace BosBelme.Controllers
{
    // Контроллер для управления игровыми комнатами и взаимодействия с пользователями.
    public class HubController : Controller
    {
        private readonly IRegService _registeredServices;
        private readonly IRoomService _roomService;
        private readonly ICookieAuthService _cookieAuthService;

        public HubController(IRegService registeredServices, IRoomService roomService, ICookieAuthService cookieAuthService)
        {
            _registeredServices = registeredServices;
            _roomService = roomService;
            _cookieAuthService = cookieAuthService;
        }

        // Отображает главную страницу контроллера.

        public IActionResult Index() => View();

        public IActionResult JoinRoom() => View();

        public async Task<IActionResult> EnterName(string? roomCode = null)
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                if (!string.IsNullOrEmpty(roomCode))
                {
                    int userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
                    try
                    {
                        await _roomService.InviteUserToRoomAsync(roomCode, userId);
                    }
                    catch { }
                    return RedirectToAction("Room", new { code = roomCode });
                }
                return RedirectToAction("Index");
            }

            var model = new EnterNameViewModel { RoomCode = roomCode };
            return View(model);
        }

        // POST: Создает новую игровую комнату и перенаправляет пользователя в нее.
        [HttpPost]
        public async Task<IActionResult> CreateRoom(JoinRoomViewModel model)
        {

            if (User.Identity?.IsAuthenticated == true)
            {
                int userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");

                var authHub = await _roomService.CreateRoomAsync(userId);

                return RedirectToAction("Room", new { code = authHub.ConnectionKey });
            }

            return RedirectToAction("EnterName");
        }

        // POST: Позволяет пользователю присоединиться к существующей игровой комнате по коду.
        [HttpPost]
        public async Task<IActionResult> JoinRoom(JoinRoomViewModel model)
        {
            if (!ModelState.IsValid)
            {
                ModelState.AddModelError("", "Код комнаты не может быть пустым.");
                return View(model);
            }

            try
            {
                var room = await _roomService.GetRoomDetailsAsync(model.RoomCode);

                return RedirectToAction("EnterName", new { roomCode = model.RoomCode });
            }
            catch
            {
                ModelState.AddModelError("", "Такой комнаты не существует. Проверьте код.");
                return View(model);
            }
        }

        // POST: Подтверждает входа гостя в комнату
        [HttpPost]
        public async Task<IActionResult> EnterName(EnterNameViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var user = await _registeredServices.RegistrationUserAsync(model.PlayerName);
            await _cookieAuthService.SignInAsync(user);

            if (!string.IsNullOrEmpty(model.RoomCode))
            {
                try
                {
                    await _roomService.InviteUserToRoomAsync(model.RoomCode, user.Id);
                    return RedirectToAction("Room", new { code = model.RoomCode });
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", ex.Message);
                    return View(model);
                }
            }

            var guestHub = await _roomService.CreateRoomAsync(user.Id);
            return RedirectToAction("Room", new { code = guestHub.ConnectionKey });
        }


        // GET: Отображает страницу игровой комнаты по коду.
        public async Task<IActionResult> Room(string code)
        {
            if (string.IsNullOrEmpty(code)) return RedirectToAction("Index");

            var model = await _roomService.GetRoomDetailsAsync(code);

            if (model == null) return RedirectToAction("Index");

            ViewData["Title"] = $"Комната {model.RoomName}";

            var viewModel = new RoomViewModel
            {
                RoomCode = model.RoomCode,
                RoomName = model.RoomName,
                HostName = model.HostName,
                Status = model.Status,
                Players = model.Players.Select(p => new RoomPlayerViewModel
                {
                    Name = p.Name,
                    IsHost = p.IsHost,
                    IsGuest = p.IsGuest
                }).ToList()
            };

            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> LeaveRoom(string code)
        {
            if (User?.Identity?.IsAuthenticated == true)
            {
                int userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
                await _roomService.LeaveRoomAsync(userId, code);
            }
            return RedirectToAction("Index");
        }
    }
}
