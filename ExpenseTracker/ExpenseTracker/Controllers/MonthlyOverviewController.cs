using ExpenseTracker.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ExpenseTracker.Controllers
{
    public class MonthlyOverviewController : BaseController
    {
        private readonly ApplicationDbContext _context;

        public MonthlyOverviewController(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(int? month, int? year)
        {
            // Default to current month if not specified
            var selectedMonth = month ?? DateTime.Now.Month;
            var selectedYear = year ?? DateTime.Now.Year;

            // Get all transactions for selected month
            var transactions = await _context.Transactions
                .Where(t => t.Date.Month == selectedMonth && t.Date.Year == selectedYear)
                .OrderByDescending(t => t.Date)
                .ToListAsync();

            // Calculate totals
            var totalIncome = transactions
                .Where(t => t.Type == "Income")
                .Sum(t => t.Amount);

            var totalExpense = transactions
                .Where(t => t.Type == "Expense")
                .Sum(t => t.Amount);

            var balance = totalIncome - totalExpense;

            // Income by Category
            var incomeByCategory = transactions
                .Where(t => t.Type == "Income")
                .GroupBy(t => t.Category)
                .Select(g => new
                {
                    Category = g.Key,
                    Amount = g.Sum(t => t.Amount),
                    Count = g.Count()
                })
                .OrderByDescending(x => x.Amount)
                .ToList();

            // Expense by Category
            var expenseByCategory = transactions
                .Where(t => t.Type == "Expense")
                .GroupBy(t => t.Category)
                .Select(g => new
                {
                    Category = g.Key,
                    Amount = g.Sum(t => t.Amount),
                    Count = g.Count()
                })
                .OrderByDescending(x => x.Amount)
                .ToList();

            // Generate month/year options for dropdown (last 12 months)
            var monthYearOptions = new List<object>();
            for (int i = 0; i < 12; i++)
            {
                var date = DateTime.Now.AddMonths(-i);
                monthYearOptions.Add(new
                {
                    Month = date.Month,
                    Year = date.Year,
                    Display = date.ToString("MMMM yyyy")
                });
            }

            // Pass data to view
            ViewBag.SelectedMonth = selectedMonth;
            ViewBag.SelectedYear = selectedYear;
            ViewBag.MonthName = new DateTime(selectedYear, selectedMonth, 1).ToString("MMMM yyyy");
            ViewBag.TotalIncome = totalIncome;
            ViewBag.TotalExpense = totalExpense;
            ViewBag.Balance = balance;
            ViewBag.Transactions = transactions;
            ViewBag.IncomeByCategory = incomeByCategory;
            ViewBag.ExpenseByCategory = expenseByCategory;
            ViewBag.MonthYearOptions = monthYearOptions;

            return View();
        }
    }
}