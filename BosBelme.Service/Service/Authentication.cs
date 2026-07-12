using BosBelme.Service.IService;
using System;
using System.Collections.Generic;
using System.Text;
using BosBelme.Data;
using BosBelme.Data.Entities;
using BosBelme.Service.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace BosBelme.Service.Service
{
    // Сервис для аутентификации пользователей реализует интерфейс IAuthService
    public class Authentication : IAuthService
    {
        private readonly AppDbContext _context;

        public Authentication(AppDbContext context)
        {
            _context = context;
        }

        //Метод для аутентификации пользователя. Проверяет наличие пользователя в базе данных по логину и email, а также проверяет соответствие пароля.
        public async Task<RegisterDto> AuthenticationUserAsync(string loginOrEmail, string password)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Name == loginOrEmail || u.Email == loginOrEmail);

            if (user == null)
            {
                throw new UserNotExistsException("Пользователя не существует.");
            }
            if (!BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
            {
                throw new UserPasswordWrongException("Неверный пароль при авторизации.");
            }

            return user.FromUser();
        }
    }
}
