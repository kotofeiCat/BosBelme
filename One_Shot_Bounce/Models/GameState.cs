using System;
using System.Collections.Generic;
using System.Text;

namespace One_Shot_Bounce.Models;

public enum MatchStatus
{
    WaitingForPlayers, 
    Warmup,            
    RoundInProgress,   
    RoundEnded,        
    MatchOver          
}

public class GameState
{
    public string RoomId { get; init; } = string.Empty;

    public MatchStatus Status { get; set; } = MatchStatus.WaitingForPlayers;

    public Player? Player1 { get; set; }
    public Player? Player2 { get; set; }

    public List<Bullet> ActiveBullets { get; } = new();

    public GameMap CurrentMap { get; set; } = new();

    public Dictionary<string, int> Scores { get; } = new();

    public float StatusTimer { get; set; }
}
