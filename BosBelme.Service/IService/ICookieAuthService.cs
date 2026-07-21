namespace BosBelme.Service.IService;

// Интерфейс для реализации куки аунтификации
public interface ICookieAuthService
{
    Task SignInAsync(RegisterDto dto);

    Task SignOutAsync();
}