using Microsoft.AspNetCore.Mvc;

namespace KaliteWeb.UI.Controllers
{
    public class DashboardController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
