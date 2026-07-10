using System;
using System.Collections.Generic;
using System.Text;
using BosBelme.Data.Entities;
using BosBelme.Service.Dto;

namespace BosBelme.Service.IService
{
    public interface ICookieAuthService
    {
        Task SignInAsync(RegisterDto dto);
    }
}