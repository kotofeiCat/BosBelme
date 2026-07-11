using System;
using System.Collections.Generic;
using System.Text;
using BosBelme.Data;
using BosBelme.Data.Entities;

namespace BosBelme.Service.Service
{
    public class RoomService
    {
        private readonly AppDbContext _context;

        public RoomService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<GameHub> CreateRoom()
        {
            string randomString = Guid.NewGuid().ToString("N").Substring(0, 10);

            var gameHub = new GameHub
            {
                Name = $"Комната-{randomString}",
                GameId = _context.Games.First().Id,
                ConnectionKey = randomString
            };

            await _context.GameHubs.AddAsync(gameHub);
            await _context.SaveChangesAsync();
            return gameHub;
        }

        
    }
}
