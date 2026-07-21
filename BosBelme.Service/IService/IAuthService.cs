namespace BosBelme.Service.IService;

// Интерфейс для реализации аунтификации
public interface IAuthService
{
    Task<RegisterDto> AuthenticationUserAsync(string loginOrEmail, string password);
}
