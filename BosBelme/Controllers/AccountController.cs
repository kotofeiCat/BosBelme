using System.Security.Claims;

namespace BosBelme.Controllers
{
    public class AccountController : Controller
    {
        // объекты сервисов
        private readonly IAuthService _authService;
        private readonly IRegService _regService;
        private readonly ICookieAuthService _cookieAuthService;

        public AccountController(
            IAuthService authService,
            IRegService regService,
            ICookieAuthService cookieAuthService)
        {
            _authService = authService;
            _regService = regService;
            _cookieAuthService = cookieAuthService;
        }

        // Методы для отображения страниц регистрации и входа
        public IActionResult Register() => View();
        public IActionResult Login() => View();

        // Метод для отображения профиля
        public IActionResult Profile()
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                var userid = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var email = User.FindFirstValue(ClaimTypes.Email);
                var name = User.FindFirstValue(ClaimTypes.Name);
                
                ViewBag.UserId = userid;
                ViewBag.Email = email;
                ViewBag.UserName = name;

                return View();
            }
            else
            {
                return RedirectToAction("Register", "Account");
            }
        }

        // Метод для обработки POST-запросов регистрации
        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            // Если пользователь уже авторизован выгоняем в профиль
            if (User.Identity?.IsAuthenticated == true) return RedirectToAction("Profile", "Account");

            // Проверка валидности данных от пользователя
            if (!ModelState.IsValid) return View(model);

            // Проверка совпадения паролей
            if (model.Password != model.ConfirmPassword)
            {
                ModelState.AddModelError(string.Empty, "Пароли не совпадают");
                return View(model);
            }

            try
            {
                // Регистрация нового пользователя
                var user = await _regService.RegistrationUserAsync(model.Name, model.Email, model.Password);

                await _cookieAuthService.SignInAsync(user.FromUser());

                return RedirectToAction("Profile", "Account");
            }
            catch (Exception ex) when
            (ex is UserAlreadyExistsException or UserNameAlreadyExistsException)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return View(model);
            }
        }

        // Метод для обработки POST-запросов входа
        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            // Если пользователь уже авторизован выгоняем в профиль
            if (User.Identity?.IsAuthenticated == true) return RedirectToAction("Profile", "Account");

            // Проверка валидности данных от пользователя
            if (!ModelState.IsValid) return View(model);

            try
            {
                // Авторизация нового пользователя
                var user = await _authService.AuthenticationUserAsync(model.NameOrEmail, model.Password);

                await _cookieAuthService.SignInAsync(user.FromUser());

                return RedirectToAction("Profile", "Account");
            }
            catch (Exception ex) when 
            (ex is UserNotExistsException or UserPasswordWrongException)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return View(model);
            }
        }

        public async Task<IActionResult> Logout()
        {
            await _cookieAuthService.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }
    }
}