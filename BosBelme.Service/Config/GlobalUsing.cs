// Глобальные директивы using для системных классов
global using System.Security.Claims;
global using Microsoft.AspNetCore.Authentication.Cookies;
global using Microsoft.AspNetCore.Authentication;
global using Microsoft.AspNetCore.Http;
global using System.Numerics;
global using System.Collections.Concurrent;
global using Microsoft.Extensions.DependencyInjection;

// Глобальные директивы using для БД
global using Microsoft.EntityFrameworkCore;
global using BosBelme.Data;
global using BosBelme.Data.Entities;

// Глобальные директивы using для внутренних сервисов
global using BosBelme.Service.Extension;
global using BosBelme.Service.IService;
global using BosBelme.Service.Dto;

// Глобальные директивы using для СигналР
global using Microsoft.AspNetCore.SignalR;
global using BosBelme.Service.SignalR;

// Глобальные директивы using для игры
global using EngineGameSession = One_Shot_Bounce.Engine.GameSession;
global using One_Shot_Bounce.Models;