using ExpenseTracker.Data;
using ExpenseTracker.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ExpenseTracker.Controllers
{
    public class IncomeController : BaseController
    {
        private readonly ApplicationDbContext _context;

        public IncomeController(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        // GET: Income Page
        public async Task<IActionResult> Index(int? month, int? year)
        {
            // Default to current month and year if not provided
            int selectedMonth = month ?? DateTime.Now.Month;
            int selectedYear = year ?? DateTime.Now.Year;

            // Filter incomes for the selected month and year
            var incomes = await _context.Transactions
                .Where(t => t.Type == "Income"
                    && t.Date.Month == selectedMonth
                    && t.Date.Year == selectedYear)
                .OrderByDescending(t => t.Date)
                .ToListAsync();

            // Group by category and sum
            var incomeByCategory = incomes
                .GroupBy(t => t.Category)
                .Select(g => new
                {
                    Category = g.Key,
                    Total = g.Sum(t => t.Amount),
                    Count = g.Count()
                })
                .OrderByDescending(x => x.Total)
                .ToList();

            ViewBag.IncomeByCategory = incomeByCategory;
            ViewBag.TotalIncome = incomes.Sum(t => t.Amount);
            ViewBag.SelectedMonth = selectedMonth;
            ViewBag.SelectedYear = selectedYear;

            return View(incomes);
        }

        // POST: Add Income
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddIncome(Transaction transaction)
        {
            if (ModelState.IsValid)
            {
                transaction.Type = "Income";
                transaction.CreatedAt = DateTime.Now;

                _context.Add(transaction);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Income added successfully!";
                return RedirectToAction("Index", "Home");
            }

            // If validation fails, return to the same page with current month data
            int currentMonth = DateTime.Now.Month;
            int currentYear = DateTime.Now.Year;

            var incomes = await _context.Transactions
                .Where(t => t.Type == "Income"
                    && t.Date.Month == currentMonth
                    && t.Date.Year == currentYear)
                .OrderByDescending(t => t.Date)
                .ToListAsync();

            return View("Index", incomes);
        }

        // GET: Delete Income
        public async Task<IActionResult> DeleteIncome(int id)
        {
            var transaction = await _context.Transactions.FindAsync(id);
            if (transaction != null)
            {
                _context.Transactions.Remove(transaction);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Income deleted successfully!";
            }
            return RedirectToAction("Index");
        }
    }
}