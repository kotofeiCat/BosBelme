using BosBelme.ViewModels;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace BosBelme.Controllers
{
    public class HomeController : Controller
    {
        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Help()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Home()
        {
            return View();
        }
    }
}
