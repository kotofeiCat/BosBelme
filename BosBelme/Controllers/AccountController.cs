using BosBelme.Models;
using BosBelme.Service.Service;
using BosBelme.Service.IService;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace BosBelme.Controllers
{
    public class AccountController : Controller
    {

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }
    }
}