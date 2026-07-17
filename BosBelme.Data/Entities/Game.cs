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

        public string Discription { get; set; } = string.Empty;

        
        public virtual ICollection<GameHub> GameHubs { get; set; } = new List<GameHub>();

        public virtual ICollection<PlayersCount> PlayersCount { get; set; } = new List<PlayersCount>();
    
    }
}
