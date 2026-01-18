using Microsoft.AspNetCore.Mvc;

namespace ExpenseTracker.Controllers
{
    public class SettingsController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
