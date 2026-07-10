using System;
using System.Collections.Generic;
using System.Text;

namespace BosBelme.Service.Exceptions
{
    //Кастомное исключения, для обрабоктки существующего имени при регистрации
    public class UserNameAlreadyExistsException : Exception
    {
        public UserNameAlreadyExistsException(string message) : base(message) { }
    }
}