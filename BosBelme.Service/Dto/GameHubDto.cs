using System;
using System.Collections.Generic;
using System.Text;

namespace BosBelme.Service.Dto
{
    public record GameHubDto(int Id, string Name, string ConnectionKey, int GameId, GameStatus Status);
}
