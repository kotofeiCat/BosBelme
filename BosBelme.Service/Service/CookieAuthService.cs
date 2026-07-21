namespace BosBelme.Service.Service;

// Сервис для работы с куки
public class CookieAuthService(IHttpContextAccessor httpContextAccessor) : ICookieAuthService
{
    // Метод для входа пользователя в систему с использованием куки
    public async Task SignInAsync(RegisterDto dto)
    {
        List<Claim> claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, dto.Id.ToString()),
            new Claim(ClaimTypes.Name, dto.Name),
            new Claim(ClaimTypes.Email, dto.Email ?? string.Empty),
            new Claim(ClaimTypes.IsPersistent, (!dto.IsGuest).ToString())
        };

        var identity = new ClaimsIdentity(claims,
            CookieAuthenticationDefaults.AuthenticationScheme);

        var principal = new ClaimsPrincipal(identity);

        await httpContextAccessor.HttpContext!.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            principal,
            new AuthenticationProperties
            {
                IsPersistent = true,
                ExpiresUtc = DateTime.UtcNow.AddDays(30)
            });
    }

    // Метод для выхода пользователя из системы
    public async Task SignOutAsync()
    {
        await httpContextAccessor.HttpContext!.SignOutAsync(
            CookieAuthenticationDefaults.AuthenticationScheme);
    }
}
