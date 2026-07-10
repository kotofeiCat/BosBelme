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
    //Класс для регистрации нового пользователя в системе. Реализует интерфейс IRegService.
    public class Registration : IRegService
    {
        //Подключение к базе данных через контекст.
        private readonly AppDbContext _context;

        public Registration(AppDbContext context)
        {
            _context = context;
        }

        //Реализация метода регистрации нового пользователя. Возращает нового зарегестрировонного пользователя.
        public async Task<Users> RegistrationUserAsync(string login, string email, string password)
        {
            if(await _context.Users.AnyAsync(u => u.Email == email))
            {
                // Если пользователь с таким email уже существует, выбрасывается исключение.
                throw new UserAlreadyExistsException("Пользователь с таким email уже существует.");
            }
            if(await _context.Users.AnyAsync(u => u.Name == login))
            {
                // Если пользователь с таким именем уже существует, выбрасывается другое исключение.
                throw new UserAlreadyExistsException("Пользователь с таким именем уже существует.");
            }

            Users user = new Users
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
