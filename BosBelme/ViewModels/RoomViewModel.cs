namespace BosBelme.ViewModels
{
    public class RoomViewModel
    {
        public string RoomCode { get; set; } = string.Empty;
        public string RoomName { get; set; } = string.Empty;
        public string HostName { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public List<RoomPlayerViewModel> Players { get; set; } = new();
    }

    public class RoomPlayerViewModel
    {
        public string Name { get; set; } = string.Empty;
        public bool IsHost { get; set; }
        public bool IsGuest { get; set; }
    }
}
