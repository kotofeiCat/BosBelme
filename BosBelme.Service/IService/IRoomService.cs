namespace BosBelme.Service.IService;

// Сервис для реализации системы комнат
public interface IRoomService
{
    Task<GameHubDto> CreateRoomAsync(int userId);

    Task InviteUserToRoomAsync(string roomCode, int userId);

    Task<RoomDto> GetRoomDetailsAsync(string code);

    Task<bool> LeaveRoomAsync(int userId, string roomCode);

    Task DeleteRoomAsync(string roomCode);

    Task ChangeGameAsync(string roomCode, int gameId, int userId);

    Task ToggleReadyAsync(string roomCode, int userId);

    Task StartGameAsync(string roomCode, int userId);

    Task<bool> IsInRoom(int userId);

    Task<string?> RoomCode(int userId);
}
