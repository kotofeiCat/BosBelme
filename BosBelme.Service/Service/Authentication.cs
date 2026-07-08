using BosBelme.Service.IService;
using System;
using System.Collections.Generic;
using System.Text;
using BosBelme.Data;
using BosBelme.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace BosBelme.Service.Service
{
    // Сервис для аутентификации пользователей реализует интерфейс IAuthentication
    public class Authentication : IAuthentication
    {
        private readonly ApplicationDbContext _context;

        public Authentication(ApplicationDbContext context)
        {
            _context = context;
        }

        //Метод для аутентификации пользователя. Проверяет наличие пользователя в базе данных по логину и email, а также проверяет соответствие пароля.
        public async Task<Users?> AuthenticationUserAsync(string loginOrEmail, string password)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Name == loginOrEmail || u.Email == loginOrEmail);

            if (user == null || !BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
            {
                return null;
            }

            return user;
        }
    }
}
