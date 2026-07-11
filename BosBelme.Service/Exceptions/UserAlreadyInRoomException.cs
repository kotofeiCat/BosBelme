using System;
using System.Collections.Generic;
using System.Text;

namespace BosBelme.Service.Exceptions
{
    //Кастомное исключения, для обрабоктки существующего пользователя при попытке присоединиться к комнате
    public class UserAlreadyInRoomException : Exception
    {
        public UserAlreadyInRoomException(string message) : base(message) { }
    }
}
