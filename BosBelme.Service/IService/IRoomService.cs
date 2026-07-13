using BosBelme.Data.Entities;

namespace BosBelme.Service.IService
{
    public interface IRoomService
    {
        public Task<GameHubDto> CreateRoomAsync(int userId);

        public Task InviteUserToRoomAsync(string roomCode, int userId);

        Task<RoomDto> GetRoomDetailsAsync(string code);

        Task LeaveRoomAsync(int userId, string roomCode);

        Task DeleteRoomAsync(string roomCode);

    }
}
