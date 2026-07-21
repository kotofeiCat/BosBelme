namespace BosBelme.Service.Dto;

// Дто для передачи данных о игровом хабе
public record GameHubDto(int Id, string Name, string ConnectionKey, int GameId, GameStatus Status);
