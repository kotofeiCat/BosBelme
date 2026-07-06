using System;
using System.Collections.Generic;
using System.Text;

namespace BosBelme.Data.Entities
{
    // Конкретная игровая сессия, с игроками и хабом
    public class GameSession
    {
        public int GameHubId { get; set; }

        public int IdPlayer { get; set; }

        public bool IsWinner { get; set; } = false;

        public bool IsHost { get; set; } = false;

        public virtual GameHub GameHub { get; set; } = null!;

        public virtual Users Player { get; set; } = null!;
    }
}
