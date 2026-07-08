using System;
using System.Collections.Generic;
using System.Text;
using BosBelme.Data.Entities;

namespace BosBelme.Service.IService
{
    public interface IAuthentication
    {
        Task<Users?> AuthenticationUserAsync(string loginOrEmail, string password);
    }
}
