using System;
using System.Collections.Generic;
using System.Text;

namespace BosBelme.Service.Exceptions
{
    //Кастомное исключения, для обрабоктки неверного пароля при авторизации
    public class UserPasswordWrongException : Exception
    {
        public UserPasswordWrongException(string message) : base(message) { }
    }
}