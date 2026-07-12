using System;
using System.Collections.Generic;
using System.Text;
using BosBelme.Data.Entities;

namespace BosBelme.Service.IService
{
    public interface IAuthService
    {
        Task<RegisterDto> AuthenticationUserAsync(string loginOrEmail, string password);
    }
}
