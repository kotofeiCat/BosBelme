namespace BosBelme.Controllers;

public class HomeController : Controller
{

    // Методы для отображения страниц
    public IActionResult Index() => View();

    public IActionResult Help() => View();

    public IActionResult Home() => View();
}
