using ExpenseTracker.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;

namespace ExpenseTracker.Controllers
{
    public class BaseController : Controller
    {
        private readonly ApplicationDbContext _context;

        public BaseController(ApplicationDbContext context)
        {
            _context = context;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            base.OnActionExecuting(context);
            
            // Get currency settings and pass to all views
            var settings = _context.UserSettings.FirstOrDefault();
            if (settings != null)
            {
                ViewBag.CurrencySymbol = settings.CurrencySymbol;
                ViewBag.Currency = settings.Currency;
            }
            else
            {
                ViewBag.CurrencySymbol = "৳";
                ViewBag.Currency = "BDT";
            }
        }
    }
}
