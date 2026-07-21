namespace BosBelme.Service.Extension;

public static class GameHubExtension
{
    extension(GameHub gameHub)
    {
        public GameHubDto FromGameHub() => new GameHubDto(gameHub.Id, gameHub.Name, gameHub.ConnectionKey, gameHub.GameId, gameHub.Status);
    }
}
