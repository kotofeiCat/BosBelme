namespace BosBelme.Data.Entities;

// Таблица игр
public class Game
{
    public int Id { get; set; }

    public string NameGame { get; set; } = string.Empty;

    public string Discription { get; set; } = string.Empty;

    public bool IsStrictRange { get; set; }

    public int? MinPlayers { get; set; }

    public int? MaxPlayers { get; set; }

    
    public virtual ICollection<GameHub> GameHubs { get; set; } = new List<GameHub>();


    public virtual ICollection<PlayersCount> PlayerCounts { get; set; } = new List<PlayersCount>();
}
