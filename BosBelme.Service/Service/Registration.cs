using System;
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
        public async Task<RegisterDto> RegistrationUserAsync(string login, string email, string password)
        {
            if(await _context.Users.AnyAsync(u => u.Email == email))
            {
                // Если пользователь с таким email уже существует, выбрасывается исключение.
                throw new UserAlreadyExistsException("Пользователь с такой почтой уже существует.");
            }
            if(await _context.Users.AnyAsync(u => u.Name == login))
            {
                // Если пользователь с таким именем уже существует, выбрасывается другое исключение.
                throw new UserNameAlreadyExistsException("Пользователь с таким именем уже существует.");
            }

            Users user = new Users
            {
                Name = login,
                Email = email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(password)
            };

            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();

            return user.FromUser();
        }

        //Реализация метода регистрации временного пользователя. Возращает нового зарегестрировонного временного пользователя.
        public async Task<RegisterDto> RegistrationUserAsync(string login)
        {
            if (await _context.Users.AnyAsync(u => u.Name == login))
                throw new UserNameAlreadyExistsException("Пользователь с таким именем уже существует.");

            Users user = new Users
            {
                Name = login,
                Email = null,
                PasswordHash = null,
                IsGuest = true
            };

            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();

            return user.FromUser();
        }
    }
}
