using System;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Numerics;
using Microsoft.AspNetCore.SignalR;

namespace BosBelme.Service.SignalR;

// Класс для работы с игрой SignalR
public class BounceHub(IBounceGameManager gameManager) : Hub
{
    // Обработчик присоединения игрока
    public async Task JoinRoom(string roomId)
    {
        var connectionId = Context.ConnectionId;
        var playerName = Context.User?.FindFirstValue(ClaimTypes.Name) ?? "Гость";

        var session = await gameManager.JoinOrCreateSessionAsync(roomId, connectionId, playerName);

        if (session == null)
        {
            await Clients.Caller.SendAsync("OnError", "Комната не найдена или неактивна.");
            return;
        }

        await Groups.AddToGroupAsync(connectionId, roomId);
        await Clients.Group(roomId).SendAsync("PlayerJoined", playerName, connectionId);
        await Clients.Caller.SendAsync("InitGame", session.State);
    }

    // Обработчик передвижения игрока
    public async Task Move(string roomId, float dx, float dy)
    {
        var connectionId = Context.ConnectionId;
        await gameManager.UpdatePlayerInputAsync(roomId, connectionId, new Vector2(dx, dy));
    }

    // Обработчик стрельбы игрока
    public async Task Shoot(string roomId, float angle)
    {
        var connectionId = Context.ConnectionId;
        await gameManager.HandleShootAsync(roomId, connectionId, angle);
    }

    // Обработчик активации щита игрока
    public async Task ActivateShield(string roomId)
    {
        var connectionId = Context.ConnectionId;
        await gameManager.ActivateShieldAsync(roomId, connectionId);
    }

    // Обработчик выхода игрока из игры
    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var connectionId = Context.ConnectionId;

        var affectedRoomId = await gameManager.HandlePlayerDisconnectAsync(connectionId);

        if (!string.IsNullOrEmpty(affectedRoomId))
        {
            await Clients.Group(affectedRoomId).SendAsync("OpponentDisconnected");
            await Groups.RemoveFromGroupAsync(connectionId, affectedRoomId);
        }

        await base.OnDisconnectedAsync(exception);
    }
}