namespace BosBelme.Controllers;

public class AccountController(IAuthService authService, IRegService regService, ICookieAuthService cookieAuthService) : Controller
{
    // Методы для отображения страниц регистрации и входа
    public IActionResult Register()
    {
        // Если пользователь уже авторизован выгоняем в профиль 
        if ((User.FindFirstValue(ClaimTypes.IsPersistent) ?? "") == "True") return RedirectToAction("Profile", "Account");

        return View();
    }

    public IActionResult Login()
    {
        // Если пользователь уже авторизован выгоняем в профиль 
        if ((User.FindFirstValue(ClaimTypes.IsPersistent) ?? "") == "True") return RedirectToAction("Profile", "Account");

        return View();
    }

    // Метод для отображения профиля
    [Authorize]
    public IActionResult Profile()
    {
        string IsPersistent = User.FindFirstValue(ClaimTypes.IsPersistent) ?? "";
        
        if (IsPersistent == "False")
        {
            return RedirectToAction("Register");
        }

        var model = new ProfileViewModel
        {
            Id = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "",
            Email = User.FindFirstValue(ClaimTypes.Email) ?? "",
            Name = User.FindFirstValue(ClaimTypes.Name) ?? "",
        };

        return View(model);
    }

    // Метод для обработки POST-запросов регистрации
    [HttpPost]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        // Если пользователь уже авторизован выгоняем в профиль 
        if ((User.FindFirstValue(ClaimTypes.IsPersistent) ?? "") == "True") return RedirectToAction("Profile", "Account");

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
            var user = await regService.RegistrationUserAsync(model.Name, model.Email, model.Password);

            await cookieAuthService.SignInAsync(user);

            return RedirectToAction("Profile", "Account");
        }
        catch (Exception ex)
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
        if ((User.FindFirstValue(ClaimTypes.IsPersistent) ?? "") == "True") return RedirectToAction("Profile", "Account");
        
        // Проверка валидности данных от пользователя
        if (!ModelState.IsValid) return View(model);

        try
        {
            // Авторизация нового пользователя
            var user = await authService.AuthenticationUserAsync(model.NameOrEmail, model.Password);

            await cookieAuthService.SignInAsync(user);
            
            return RedirectToAction("Profile", "Account");
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(model);
        }
    }

    [Authorize]
    public async Task<IActionResult> Logout()
    {
        await cookieAuthService.SignOutAsync();
        return RedirectToAction("Index", "Home");
    }
}