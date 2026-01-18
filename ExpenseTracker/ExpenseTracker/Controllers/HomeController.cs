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
            var transactions = await _context.Transactions.ToListAsync();

            var now = DateTime.Now;
            var currentMonth = now.Month;
            var currentYear = now.Year;
            var lastMonth = now.AddMonths(-1).Month;
            var lastMonthYear = now.AddMonths(-1).Year;

            // Current Month Calculations
            var currentMonthTransactions = transactions
                .Where(t => t.Date.Month == currentMonth && t.Date.Year == currentYear)
                .ToList();

            var totalIncome = currentMonthTransactions
                .Where(t => t.Type == "Income")
                .Sum(t => t.Amount);

            var totalExpense = currentMonthTransactions
                .Where(t => t.Type == "Expense")
                .Sum(t => t.Amount);

            var balance = totalIncome - totalExpense;

            // Last Month Calculations
            var lastMonthTransactions = transactions
                .Where(t => t.Date.Month == lastMonth && t.Date.Year == lastMonthYear)
                .ToList();

            var lastMonthIncome = lastMonthTransactions
                .Where(t => t.Type == "Income")
                .Sum(t => t.Amount);

            var lastMonthExpense = lastMonthTransactions
                .Where(t => t.Type == "Expense")
                .Sum(t => t.Amount);

            // Percentage Changes
            var incomeChange = lastMonthIncome > 0
                ? ((totalIncome - lastMonthIncome) / lastMonthIncome * 100)
                : 0;

            var expenseChange = lastMonthExpense > 0
                ? ((totalExpense - lastMonthExpense) / lastMonthExpense * 100)
                : 0;

            // Savings Rate
            var savingsRate = totalIncome > 0
                ? ((totalIncome - totalExpense) / totalIncome * 100)
                : 0;

            // Get recent transactions (last 5)
            var recentTransactions = transactions
                .OrderByDescending(t => t.Date)
                .Take(5)
                .ToList();

            // Expense by Category for Pie Chart
            var expenseByCategory = currentMonthTransactions
                .Where(t => t.Type == "Expense")
                .GroupBy(t => t.Category)
                .Select(g => new
                {
                    Category = g.Key,
                    Amount = g.Sum(t => t.Amount)
                })
                .OrderByDescending(x => x.Amount)
                .ToList();

            // Find highest spending category
            var highestCategory = expenseByCategory.FirstOrDefault();

            // Spending Alert (if spending more than last month)
            string spendingAlert = null;
            if (expenseChange > 20)
            {
                spendingAlert = $" Warning: Your expenses are {expenseChange:F1}% higher than last month!";
            }
            else if (expenseChange > 0)
            {
                spendingAlert = $" Your expenses increased by {expenseChange:F1}% compared to last month.";
            }
            else if (expenseChange < 0)
            {
                spendingAlert = $" Great! You reduced expenses by {Math.Abs(expenseChange):F1}% compared to last month.";
            }

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
            ViewBag.IncomeChange = incomeChange;
            ViewBag.ExpenseChange = expenseChange;
            ViewBag.LastMonthIncome = lastMonthIncome;
            ViewBag.LastMonthExpense = lastMonthExpense;
            ViewBag.SavingsRate = savingsRate;
            ViewBag.HighestCategory = highestCategory;
            ViewBag.SpendingAlert = spendingAlert;
            ViewBag.RecentTransactions = recentTransactions;
            ViewBag.ExpenseByCategory = expenseByCategory;
            ViewBag.MonthlyData = monthlyData;

            return View();
        }
    }
}
