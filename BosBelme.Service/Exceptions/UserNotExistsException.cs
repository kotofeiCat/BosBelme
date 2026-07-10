using System;
using System.Collections.Generic;
using System.Text;

namespace BosBelme.Service.Exceptions
{
    //Кастомное исключения, для обработки ситуации когда пользователя не существует
    public class UserNotExistsException : Exception
    {
        public UserNotExistsException(string message) : base(message) { }
    }
}
