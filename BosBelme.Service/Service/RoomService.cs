using System;
using System.Collections.Generic;
using System.Text;
using BosBelme.Data;
using BosBelme.Data.Entities;
using BosBelme.Service.Extension;

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

            var gameHub = new GameHub
            {
                Name = $"Комната-{String.GetRandomName()}",
                GameId = _context.Games.First().Id,
                ConnectionKey = String.GetRandomString()
            };

            await _context.GameHubs.AddAsync(gameHub);
            await _context.SaveChangesAsync();
            return gameHub;
        }

        
    }
}
