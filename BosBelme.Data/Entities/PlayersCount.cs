namespace BosBelme.Data.Entities;

// Таблица игр
public class PlayersCount
{
    public int Id { get; set; }

    public int GameId { get; set; }

    public int Count { get; set; }
    
    public virtual Game? Game { get; set; }
}