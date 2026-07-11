using Microsoft.AspNetCore.Mvc;

namespace BosBelme.Controllers
{
    public class HubController : Controller
    {
        public IActionResult Index() => View();

        public IActionResult JoinRoom() => View();

        public IActionResult CreateRoom() => View();
    }
}
