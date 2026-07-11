using System;
using System.Collections.Generic;
using System.Text;

namespace BosBelme.Data.Entities
{
    // Модель таблица в нашей бд для хранения информации о комнатах
    public class GameHub
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public string ConnectionKey { get; set; } = string.Empty;

        public int GameId { get; set; }

        public GameStatus Status { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? StartedAt { get; set; }

        public DateTime? FinishedAt { get; set; }

        public virtual Game Game { get; set; } = null!;

        public virtual ICollection<GameSession> GameSessions { get; set; } = new List<GameSession>();
    }
}
