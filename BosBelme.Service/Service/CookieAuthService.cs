using Microsoft.AspNetCore.Authentication.Cookies;

namespace BosBelme.Service.Service
{
    // Сервис для работы с куки
    public class CookieAuthService : ICookieAuthService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CookieAuthService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        // Метод для входа пользователя в систему с использованием куки
        public async Task SignInAsync(RegisterDto dto)
        {
            List<Claim> claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, dto.Id.ToString()),
                new Claim(ClaimTypes.Name, dto.Name),
                new Claim(ClaimTypes.Email, dto?.Email ?? string.Empty) 
            };

            var identity = new ClaimsIdentity(claims,
                CookieAuthenticationDefaults.AuthenticationScheme);

            var principal = new ClaimsPrincipal(identity);

            await _httpContextAccessor.HttpContext!.SignInAsync(
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
            await _httpContextAccessor.HttpContext!.SignOutAsync(
                CookieAuthenticationDefaults.AuthenticationScheme);
        }
    }
}
