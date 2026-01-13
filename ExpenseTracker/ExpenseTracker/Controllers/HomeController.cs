using ExpenseTracker.Data;
using ExpenseTracker.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ExpenseTracker.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            // Get all transactions
            var transactions = await _context.Transactions.ToListAsync();

            // Calculate totals
            var totalIncome = transactions
                .Where(t => t.Type == "Income")
                .Sum(t => t.Amount);

            var totalExpense = transactions
                .Where(t => t.Type == "Expense")
                .Sum(t => t.Amount);

            var balance = totalIncome - totalExpense;

            // Get recent transactions (last 5)
            var recentTransactions = transactions
                .OrderByDescending(t => t.Date)
                .Take(5)
                .ToList();

            // Pass data to view
            ViewBag.TotalIncome = totalIncome;
            ViewBag.TotalExpense = totalExpense;
            ViewBag.Balance = balance;
            ViewBag.RecentTransactions = recentTransactions;

            return View();
        }
    }
}
