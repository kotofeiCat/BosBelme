namespace BosBelme.ViewModels
{
    public class RoomViewModel
    {
        public string RoomCode { get; set; } = string.Empty;
        public string RoomName { get; set; } = string.Empty;
        public string HostName { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;

        public int GameId { get; set; }
        public string GameName { get; set; } = string.Empty;
        public int MinPlayers { get; set; }
        public int MaxPlayers { get; set; }

        public List<GameSelectViewModel> AvailableGames { get; set; } = new();

        public List<RoomPlayerViewModel> Players { get; set; } = new();
    }

    public class RoomPlayerViewModel
    {
        public int Id { get; set; } 
        public string Name { get; set; } = string.Empty;
        public bool IsHost { get; set; }
        public bool IsGuest { get; set; }
        public bool IsReady { get; set; } 
    }

    public class GameSelectViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }
}
