using System;
using System.Collections.Generic;
using System.Text;
using BosBelme.Data;
using BosBelme.Data.Entities;

namespace BosBelme.Service.IService
{
    public interface IRegistration
    {
        Task<Users> RegistrationUserAsync(string login, string password, string email);
    }
}
