using System;
using System.Collections.Generic;
using System.Text;

namespace BosBelme.Service.Exceptions
{
    //Кастомное исключения, для обрабоктки отсутствия игры при попытке присоединиться к комнате
    public class GameNotFoundException : Exception
    {
        public GameNotFoundException(string message) : base(message) { }
    }
}
