using BosBelme.Data.Entities;
using BosBelme.Service.Dto;

namespace BosBelme.Service.Extension
{
    // Класс расширения для сущности User.
    public static class UserExtension
    {
        extension(User user)
        {
            // Метод для преобразования объекта User в объект RegisterDto. Возвращает новый объект RegisterDto с идентификатором, именем и email пользователя.
            public RegisterDto FromUser() => new RegisterDto(user.Id, user.Name, user.Email, user.IsGuest);
        }
    }
}
