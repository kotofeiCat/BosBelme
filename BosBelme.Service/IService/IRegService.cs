namespace BosBelme.Service.IService;

// Интерфейс для регистрации пользователей в системе. Определяет методы для регистрации обычных и временных пользователей.
public interface IRegService
{
    Task<RegisterDto> RegistrationUserAsync(string login, string password, string email);
    Task<RegisterDto> RegistrationUserAsync(string login);
}
