using System;
using System.Collections.Generic;
using System.Text;
using BosBelme.Data;
using BosBelme.Data.Entities;

namespace BosBelme.Service.IService
{
    // Интерфейс для регистрации пользователей в системе. Определяет методы для регистрации обычных и временных пользователей.
    public interface IRegService
    {
        Task<Users> RegistrationUserAsync(string login, string password, string email);
        Task<Users> RegistrationUserAsync(string login);
    }
}
