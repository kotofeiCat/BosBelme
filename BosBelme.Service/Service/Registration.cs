using System;
using System.Collections.Generic;
using System.Text;
using BosBelme.Data;
using BosBelme.Data.Entities;
using BosBelme.Service.Exceptions;
using BosBelme.Service.IService;
using Microsoft.EntityFrameworkCore;

namespace BosBelme.Service.Service
{
    //Класс для регистрации нового пользователя в системе. Реализует интерфейс IRegistration.
    public class Registration : IRegistration
    {
        //Подключение к базе данных через контекст.
        private readonly ApplicationDbContext _context;

        public Registration(ApplicationDbContext context)
        {
            _context = context;
        }

        //Реализация метода регистрации нового пользователя. Возращает нового зарегестрировонного пользователя.
        public async Task<Users> RegistrationUserAsync(string login, string password, string email)
        {
            if(await _context.Users.AnyAsync(u => u.Name == login || u.Email == email))
            {
                // Если пользователь с таким логином или email уже существует, выбрасывается исключение UserAlreadyExistsException.
                throw new UserAlreadyExistsException("Пользователь уже существует с таким логином или email.");
            }

            var user = new Users
            {
                Name = login,
                Email = email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(password)
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return user;
        }
    }
}
