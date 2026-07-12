using BosBelme.Data.Entities;

namespace BosBelme.Service.IService
{
    public interface IRoomService
    {
        public Task<GameHubDto> CreateRoomAsync(int userId);

        public Task InviteUserToRoomAsync(int gameHubId, int userId);

        Task<RoomDto?> GetRoomDetailsAsync(string code);
    }
}
