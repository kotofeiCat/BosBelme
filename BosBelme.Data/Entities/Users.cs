using System;
using System.Collections.Generic;
using System.Text;

namespace BosBelme.Data.Entities
{
    // Модель таблица в нашей бд для хранения информации о пользователях, которая будет использоваться для аутентификации и авторизации
    public class Users
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public string? PasswordHash { get; set; } = string.Empty;

        public string? Email { get; set; } = string.Empty;

        public bool IsGuest { get; set; } = false;

        public virtual ICollection<GameSession> GameSessions { get; set; } = new List<GameSession>();
    }
}
