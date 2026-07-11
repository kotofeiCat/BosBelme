using System;
using System.Collections.Generic;
using System.Text;

namespace BosBelme.Service.Dto
{
    public class RoomDto
    {
        public string RoomCode { get; set; } = string.Empty;
        public string RoomName { get; set; } = string.Empty;
        public string HostName { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public List<RoomPlayerDto> Players { get; set; } = new();
    }

    public class RoomPlayerDto
    {
        public string Name { get; set; } = string.Empty;
        public bool IsHost { get; set; }

        public bool IsGuest { get; set; }
    }
}
