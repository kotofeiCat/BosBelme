namespace BosBelme.Service.Dto;

// Дто для передачи данных о комнате
public record RoomDto
{
    public string RoomCode { get; set; } = string.Empty;
    public string RoomName { get; set; } = string.Empty;
    public string HostName { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;


    public int GameId { get; set; }
    public string GameName { get; set; } = string.Empty;

    public List<int> PlayersCounts { get; set; } = new();


    public List<GameSelectDto> AvailableGames { get; set; } = new();

    public List<RoomPlayerDto> Players { get; set; } = new();
}

public record RoomPlayerDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool IsHost { get; set; }
    public bool IsGuest { get; set; }
    public bool IsReady { get; set; }
}

public record GameSelectDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public bool IsStrictRange { get; set; }

    public int? MinPlayers { get; set; }

    public int? MaxPlayers { get; set; }
}

public record GamePlayersDto
{
    public int Count { get; set; }
}
