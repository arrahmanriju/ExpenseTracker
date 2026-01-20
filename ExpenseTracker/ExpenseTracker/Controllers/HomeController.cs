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

        public async Task<IActionResult> Index(int? month, int? year)
        {
            // Default to current month and year if not provided
            int selectedMonth = month ?? DateTime.Now.Month;
            int selectedYear = year ?? DateTime.Now.Year;

            // Get all transactions for the selected month
            var transactions = await _context.Transactions
                .Where(t => t.Date.Month == selectedMonth && t.Date.Year == selectedYear)
                .ToListAsync();

            // Calculate totals for the selected month
            var totalIncome = transactions
                .Where(t => t.Type == "Income")
                .Sum(t => t.Amount);

            var totalExpense = transactions
                .Where(t => t.Type == "Expense")
                .Sum(t => t.Amount);

            var balance = totalIncome - totalExpense;

            // Get recent transactions from the selected month (last 10)
            var recentTransactions = transactions
                .OrderByDescending(t => t.Date)
                .Take(10)
                .ToList();

            // Expense by Category for Pie Chart (selected month only)
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

            // Income by Category (selected month only)
            var incomeByCategory = transactions
                .Where(t => t.Type == "Income")
                .GroupBy(t => t.Category)
                .Select(g => new
                {
                    Category = g.Key,
                    Amount = g.Sum(t => t.Amount)
                })
                .OrderByDescending(x => x.Amount)
                .ToList();

            // Monthly trend data (last 6 months for comparison)
            var monthlyData = new List<object>();
            for (int i = 5; i >= 0; i--)
            {
                var targetDate = new DateTime(selectedYear, selectedMonth, 1).AddMonths(-i);

                var allTransactionsForMonth = await _context.Transactions
                    .Where(t => t.Date.Month == targetDate.Month && t.Date.Year == targetDate.Year)
                    .ToListAsync();

                var monthIncome = allTransactionsForMonth
                    .Where(t => t.Type == "Income")
                    .Sum(t => t.Amount);

                var monthExpense = allTransactionsForMonth
                    .Where(t => t.Type == "Expense")
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
            ViewBag.IncomeByCategory = incomeByCategory;
            ViewBag.MonthlyData = monthlyData;
            ViewBag.SelectedMonth = selectedMonth;
            ViewBag.SelectedYear = selectedYear;

            return View();
        }
    }
}