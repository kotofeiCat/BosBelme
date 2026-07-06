using System;
using System.Collections.Generic;
using System.Text;

namespace BosBelme.Data.Entities
{
    // Таблица игр
    public class Game
    {
        public int Id { get; set; }

        public string NameGame { get; set; } = string.Empty;

        public int MaxPlayers { get; set; }

        public int MinPlayers { get; set; }

        public virtual ICollection<GameHub> GameHubs { get; set; } = new List<GameHub>();
    }
}
