using System.Security.Claims;
using BosBelme.Service.Dto;

namespace BosBelme.Controllers
{
    // Контроллер для управления игровыми комнатами и взаимодействия с пользователями
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

        // Отображает главную страницу контроллера
        public async Task<IActionResult> Index()
        {
            string? userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId is not null)
            {
                string? roomCode = await _roomService.RoomCode(int.Parse(userId));
                if (roomCode is not null)
                {
                    return RedirectToAction("Hub",  new { code = roomCode });
                }

                return View();
            }

            return View();
        } 

        // Отображает страницу для ввода кода
        public async Task<IActionResult> JoinRoom()
        {
            string? userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId is not null)
            {
                string? roomCode = await _roomService.RoomCode(int.Parse(userId));
                if (roomCode is not null)
                {
                    return RedirectToAction("Hub",  new { code = roomCode });
                }

                return View();
            }

            return View();
        }

        // Отображает страницу игровой комнаты по коду
        [Authorize]
        public async Task<IActionResult> Hub(string code)
        {
            if (string.IsNullOrEmpty(code)) return RedirectToAction("Index");

            // Если игрок не находится в комнате он не может увидеть ее
            int userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
            
            string? roomCode = await _roomService.RoomCode(userId);

            if (code != roomCode)
            {
                return RedirectToAction("Index");
            }


            var model = await _roomService.GetRoomDetailsAsync(code);

            if (model == null) return RedirectToAction("Index");

            ViewData["Title"] = model.RoomName;

            var viewModel = new RoomViewModel
            {
                RoomCode = model.RoomCode,
                RoomName = model.RoomName,
                HostName = model.HostName,
                Status = model.Status,

                GameId = model.GameId,
                GameName = model.GameName,
                PlayersCounts = model.PlayersCounts,

                AvailableGames = model.AvailableGames.Select(g => new GameSelectViewModel
                {
                    Id = g.Id,
                    Name = g.Name 
                })
                .ToList(),

                Players = model.Players.Select(p => new RoomPlayerViewModel
                {
                    Id = p.Id,
                    Name = p.Name,
                    IsHost = p.IsHost,
                    IsGuest = p.IsGuest,
                    IsReady = p.IsReady
                })
                .ToList()
            };

            return View(viewModel);
        }

        // Метод для отображения страницы ввода имени
        public async Task<IActionResult> EnterName(string? roomCode = null)
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                int userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
                string? roomCodePlayer = await _roomService.RoomCode(userId);
                if (roomCodePlayer is not null)
                return RedirectToAction("Hub",  new { code = roomCodePlayer });
                

                if (!string.IsNullOrEmpty(roomCode))
                {
                    try
                    {
                        await _roomService.InviteUserToRoomAsync(roomCode, userId);
                    }
                    catch { }
                    return RedirectToAction("Hub", new { code = roomCode });
                }
                return RedirectToAction("Index");
            }

            var model = new EnterNameViewModel { RoomCode = roomCode };
            return View(model);
        }

        // POST: Создает новую игровую комнату и перенаправляет пользователя в нее
        [HttpPost]
        public async Task<IActionResult> CreateRoom(JoinRoomViewModel model)
        {

            if (User.Identity?.IsAuthenticated == true)
            {
                int userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
                string? roomCodePlayer = await _roomService.RoomCode(userId);
                if (roomCodePlayer is not null)
                return RedirectToAction("Hub",  new { code = roomCodePlayer });

                var authHub = await _roomService.CreateRoomAsync(userId);

                return RedirectToAction("Hub", new { code = authHub.ConnectionKey });
            }

            return RedirectToAction("EnterName");
        }

        // POST: Позволяет пользователю присоединиться к существующей игровой комнате по коду
        [HttpPost]
        public async Task<IActionResult> JoinRoom(JoinRoomViewModel model)
        {
            if (!ModelState.IsValid)
            {
                ModelState.AddModelError(String.Empty, "Код комнаты не может быть пустым.");
                return View(model);
            }

            try
            {
                var room = await _roomService.GetRoomDetailsAsync(model.RoomCode);

                return RedirectToAction("EnterName", new { roomCode = model.RoomCode });
            }
            catch
            {
                ModelState.AddModelError(String.Empty, "Такой комнаты не существует. Проверьте код.");
                return View(model);
            }
        }

        // POST: Подтверждает входа гостя в комнату
        [HttpPost]
        public async Task<IActionResult> EnterName(EnterNameViewModel model)
        {
            // Если пользователь уже авторизован выгоняем на главную
            if (User.Identity?.IsAuthenticated == true) return RedirectToAction("Index");

            if (!ModelState.IsValid) return View(model);
            

            RegisterDto user;

            try
            {
                user = await _registeredServices.RegistrationUserAsync(model.PlayerName);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View(model);
            }

            await _cookieAuthService.SignInAsync(user);

            if (!string.IsNullOrEmpty(model.RoomCode))
            {
                try
                {
                    await _roomService.InviteUserToRoomAsync(model.RoomCode, user.Id);
                    return RedirectToAction("Hub", new { code = model.RoomCode });
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", ex.Message);
                    return View(model);
                }
            }

            var guestHub = await _roomService.CreateRoomAsync(user.Id);
            return RedirectToAction("Hub", new { code = guestHub.ConnectionKey });
        }

        [HttpPost]
        [Authorize]
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
