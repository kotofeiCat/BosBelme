namespace BosBelme.Service.IService;

// Интерфейс для реализации менеджера игры
public interface IBounceGameManager
{
    Task<EngineGameSession?> JoinOrCreateSessionAsync(string roomId, string playerId, string playerName);
    Task UpdatePlayerInputAsync(string roomId, string playerId, Vector2 direction);
    Task HandleShootAsync(string roomId, string playerId, float angle);
    Task ActivateShieldAsync(string roomId, string playerId);
    Task<string?> HandlePlayerDisconnectAsync(string playerId);
    void RemoveSession(string roomId);

    Task UpdatePlayerAimAsync(string roomId, string playerId, float angle);
}
