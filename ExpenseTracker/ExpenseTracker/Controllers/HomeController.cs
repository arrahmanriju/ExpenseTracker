using ExpenseTracker.Data;
using ExpenseTracker.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ExpenseTracker.Controllers
{
    public class HomeController : BaseController
    {
        private readonly ApplicationDbContext _context;

        public HomeController(ApplicationDbContext context) : base(context)
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
                .ToList();

            // Expense by Category for Pie Chart
            var expenseByCategory = transactions
                .Where(t => t.Type == "Expense")
                .GroupBy(t => t.Category)
                .Select(g => new
                {
                    Category = g.Key,
                    Amount = g.Sum(t => t.Amount)
                })
                .OrderByDescending(x => x.Amount)
                .ToList();

            // Monthly trend data (last 6 months)
            var monthlyData = new List<object>();
            for (int i = 5; i >= 0; i--)
            {
                var targetDate = DateTime.Now.AddMonths(-i);
                var monthIncome = transactions
                    .Where(t => t.Type == "Income" && t.Date.Month == targetDate.Month && t.Date.Year == targetDate.Year)
                    .Sum(t => t.Amount);
                var monthExpense = transactions
                    .Where(t => t.Type == "Expense" && t.Date.Month == targetDate.Month && t.Date.Year == targetDate.Year)
                    .Sum(t => t.Amount);

                monthlyData.Add(new
                {
                    Month = targetDate.ToString("MMM yyyy"),
                    Income = monthIncome,
                    Expense = monthExpense
                });
            }

            // Pass data to view
            ViewBag.TotalIncome = totalIncome;
            ViewBag.TotalExpense = totalExpense;
            ViewBag.Balance = balance;
            ViewBag.RecentTransactions = recentTransactions;
            ViewBag.ExpenseByCategory = expenseByCategory;
            ViewBag.MonthlyData = monthlyData;

            return View();
        }
    }
}

