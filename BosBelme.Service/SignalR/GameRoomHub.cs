using System;
using System.Collections.Generic;
using System.Text;

namespace BosBelme.Service.SignalR
{
    //Класс для игровых комнат сигналР
    public class GameRoomHub : Hub
    {
        private readonly IRoomService _roomService;

        public GameRoomHub(IRoomService roomService)
        {
            _roomService = roomService;
        }

        //Метод для добовления пользователей в группу.
        public async Task JoinRoom(string roomCode)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, roomCode);

            var roomDetails = await _roomService.GetRoomDetailsAsync(roomCode);

            await Clients.Group(roomCode).SendAsync("UpdateRoom", roomDetails);
        }

        //Метод для выхода из группы.
        public async Task LeaveRoom(string roomCode, int userId)
        {
            await _roomService.LeaveRoomAsync(userId, roomCode);

            await Groups.RemoveFromGroupAsync(Context.ConnectionId, roomCode);

            try
            {
                var roomDetails = await _roomService.GetRoomDetailsAsync(roomCode);

                await Clients.Group(roomCode).SendAsync("UpdateRoom", roomDetails);
            }
            catch
            {
                await Clients.Group(roomCode).SendAsync("RoomDelete");
            }
        }
    }
}
