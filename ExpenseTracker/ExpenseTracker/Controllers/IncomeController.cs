using Microsoft.AspNetCore.Mvc;

namespace ExpenseTracker.Controllers
{
    public class IncomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
