using System;
using System.Collections.Generic;
using System.Text;

namespace BosBelme.Service.Exceptions
{
    //Кастомное исключения, для обрабоктки существующего пользователя при регистрации
    public class UserAlreadyExistsException : Exception
    {
        public UserAlreadyExistsException(string message) : base(message) { }
    }
}
