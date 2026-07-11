using System;
using System.Collections.Generic;
using System.Text;
using BosBelme.Data;
using BosBelme.Data.Entities;
using BosBelme.Service.Extension;
using BosBelme.Service.Exceptions;
using BosBelme.Service.IService;
using Microsoft.EntityFrameworkCore;

namespace BosBelme.Service.Service
{
    // Сервис для работы с комнатами и игровыми сессиями
    public class RoomService : IRoomService
    {
        private readonly AppDbContext _context;

        public RoomService(AppDbContext context)
        {
            _context = context;
        }

        // Создает новую игровую комнату и добавляет пользователя в нее
        public async Task<GameHub> CreateRoomAsync(int userId)
        {
            var defaultGame = await _context.Games.FirstOrDefaultAsync() ?? throw new GameNotFoundException("Игра не найдена.");

            var gameHub = new GameHub
            {
                Name = $"Комната-{String.GetRandomName()}",
                GameId = defaultGame.Id,
                ConnectionKey = String.GetRandomString()
            };

            await _context.GameHubs.AddAsync(gameHub);

            var gameSession = new GameSession
            {
                GameHub = gameHub,
                IdPlayer = userId
            };

            await _context.GameSessions.AddAsync(gameSession);


            await _context.SaveChangesAsync();
            return gameHub;
        }


        // Приглашает пользователя в существующую игровую комнату
        public async Task InviteUserToRoomAsync(int gameHubId, int userId)
        {
            if (await _context.GameSessions.AnyAsync(gs => gs.IdPlayer == userId))
                throw new UserAlreadyInRoomException($"Пользователь уже состоит в комнате.");

            var gameSession = new GameSession
            {
                GameHubId = gameHubId,
                IdPlayer = userId
            };

            await _context.GameSessions.AddAsync(gameSession);
            await _context.SaveChangesAsync();
        }
    }
}
