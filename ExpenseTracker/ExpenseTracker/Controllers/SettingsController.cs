using ExpenseTracker.Data;
using ExpenseTracker.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ExpenseTracker.Controllers
{
    public class SettingsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public SettingsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Settings Page
        public async Task<IActionResult> Index()
        {
            // Get or create default settings
            var settings = await _context.UserSettings.FirstOrDefaultAsync();
            
            if (settings == null)
            {
                // Create default settings if none exist
                settings = new UserSettings
                {
                    UserName = "User",
                    Email = "",
                    MonthlyBudget = 0,
                    Currency = "BDT",
                    CurrencySymbol = "৳"
                };
                _context.UserSettings.Add(settings);
                await _context.SaveChangesAsync();
            }

            // Calculate budget usage
            var currentMonth = DateTime.Now.Month;
            var currentYear = DateTime.Now.Year;
            
            var monthlyExpense = await _context.Transactions
                .Where(t => t.Type == "Expense" 
                    && t.Date.Month == currentMonth 
                    && t.Date.Year == currentYear)
                .SumAsync(t => t.Amount);

            ViewBag.MonthlyExpense = monthlyExpense;
            ViewBag.BudgetRemaining = settings.MonthlyBudget - monthlyExpense;
            ViewBag.BudgetPercentage = settings.MonthlyBudget > 0 
                ? (monthlyExpense / settings.MonthlyBudget * 100) 
                : 0;

            return View(settings);
        }

        // POST: Update Settings
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateSettings(UserSettings settings)
        {
            if (ModelState.IsValid)
            {
                var existingSettings = await _context.UserSettings.FirstOrDefaultAsync();
                
                if (existingSettings != null)
                {
                    existingSettings.UserName = settings.UserName;
                    existingSettings.Email = settings.Email;
                    existingSettings.MonthlyBudget = settings.MonthlyBudget;
                    existingSettings.Currency = settings.Currency;
                    
                    // Update currency symbol based on selection
                    existingSettings.CurrencySymbol = settings.Currency switch
                    {
                        "BDT" => "৳",
                        "USD" => "$",
                        "EUR" => "€",
                        "GBP" => "£",
                        "INR" => "₹",
                        _ => "৳"
                    };
                    
                    existingSettings.UpdatedAt = DateTime.Now;
                    
                    _context.Update(existingSettings);
                    await _context.SaveChangesAsync();
                    
                    TempData["SuccessMessage"] = "Settings updated successfully!";
                }
                
                return RedirectToAction("Index");
            }
            
            return View("Index", settings);
        }
    }
}
