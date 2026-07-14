using System;
using System.Collections.Generic;
using System.Text;

namespace BosBelme.Service.SignalR
{
    // Класс для игровых комнат SignalR
    public class GameRoomHub : Hub
    {
        private readonly IRoomService _roomService;

        public GameRoomHub(IRoomService roomService)
        {
            _roomService = roomService;
        }

        // Метод для добавления пользователей в группу
        public async Task JoinRoom(string roomCode)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, roomCode);

            await BroadcastRoomUpdate(roomCode);

        }

        // Метод обновления информации
        private async Task BroadcastRoomUpdate(string roomCode)
        {
            var roomDetails = await _roomService.GetRoomDetailsAsync(roomCode);
            await Clients.Group(roomCode).SendAsync("UpdateRoom", roomDetails);
        }

        // Метод для выхода из группы
        public async Task LeaveRoom(string roomCode, int userId)
        {
            await _roomService.LeaveRoomAsync(userId, roomCode);

            await Groups.RemoveFromGroupAsync(Context.ConnectionId, roomCode);
        }

        // Метод для смены игры
        public async Task ChangeGame(string roomCode, int gameId)
        {
            try
            {
                int userId = GetCurrentUserId();
                await _roomService.ChangeGameAsync(roomCode, gameId, userId);
                await BroadcastRoomUpdate(roomCode);
            }
            catch (Exception ex)
            {
                await Clients.Caller.SendAsync("OnError", ex.Message);
            }
        }

        // Метод игрок нажал готово
        public async Task ToggleReady(string roomCode)
        {
            try
            {
                int userId = GetCurrentUserId();
                await _roomService.ToggleReadyAsync(roomCode, userId);
                await BroadcastRoomUpdate(roomCode);
            }
            catch (Exception ex)
            {
                await Clients.Caller.SendAsync("OnError", ex.Message);
            }
        }

        // Метод для начала игры
        public async Task StartGame(string roomCode)
        {
            try
            {
                int userId = GetCurrentUserId();
                await _roomService.StartGameAsync(roomCode, userId);

                await Clients.Group(roomCode).SendAsync("GameStarted");
            }
            catch (Exception ex)
            {
                await Clients.Caller.SendAsync("OnError", ex.Message);
            }
        }

        // Метод для получения ID пользователя
        private int GetCurrentUserId()
        {
            return int.Parse(Context.User?.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
        }
    }
}
