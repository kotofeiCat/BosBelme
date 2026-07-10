using System;
using System.Collections.Generic;
using System.Text;

namespace BosBelme.Service.Exceptions
{
    //Кастомное исключения, для обрабоктки существующего имени при регистрации
    public class UserNameAlredyExistsException : Exception
    {
        public UserNameAlredyExistsException(string message) : base(message) { }
    }
}