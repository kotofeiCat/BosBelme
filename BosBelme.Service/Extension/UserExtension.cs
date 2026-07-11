using BosBelme.Data.Entities;
using BosBelme.Service.Dto;

namespace BosBelme.Service.Extension
{
    // Класс расширения для сущности Users.
    public static class UserExtension
    {
        extension(Users user)
        {
            // Метод для преобразования объекта Users в объект RegisterDto. Возвращает новый объект RegisterDto с идентификатором, именем и email пользователя.
            public RegisterDto FromUser() => new RegisterDto(user.Id, user.Name, user?.Email);
        }
    }
}
