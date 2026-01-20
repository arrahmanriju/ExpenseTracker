using ExpenseTracker.Data;
using ExpenseTracker.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ExpenseTracker.Controllers
{
    public class ExpenseController : BaseController
    {
        private readonly ApplicationDbContext _context;

        public ExpenseController(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        // GET: Add Expense Page
        public IActionResult AddExpense()
        {
            return View();
        }

        // POST: Save Expense
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddExpense(Transaction transaction)
        {
            if (ModelState.IsValid)
            {
                transaction.Type = "Expense";
                transaction.CreatedAt = DateTime.Now;

                _context.Add(transaction);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Expense added successfully!";
                return RedirectToAction("Index", "Home");
            }
            return View(transaction);
        }

        // GET: Fixed Expense Page
        public async Task<IActionResult> FixedExpense(int? month, int? year)
        {
            // Default to current month and year if not provided
            int selectedMonth = month ?? DateTime.Now.Month;
            int selectedYear = year ?? DateTime.Now.Year;

            // Filter fixed expenses for the selected month and year
            var fixedExpenses = await _context.Transactions
                .Where(t => t.Type == "Expense"
                    && t.IsFixed == true
                    && t.Date.Month == selectedMonth
                    && t.Date.Year == selectedYear)
                .OrderByDescending(t => t.Date)
                .ToListAsync();

            // Calculate total for the month
            var totalFixed = fixedExpenses.Sum(t => t.Amount);

            // Group by category
            var expenseByCategory = fixedExpenses
                .GroupBy(t => t.Category)
                .Select(g => new
                {
                    Category = g.Key,
                    Total = g.Sum(t => t.Amount),
                    Count = g.Count()
                })
                .OrderByDescending(x => x.Total)
                .ToList();

            ViewBag.ExpenseByCategory = expenseByCategory;
            ViewBag.TotalFixedExpense = totalFixed;
            ViewBag.SelectedMonth = selectedMonth;
            ViewBag.SelectedYear = selectedYear;

            return View(fixedExpenses);
        }

        // GET: Delete Fixed Expense
        public async Task<IActionResult> DeleteFixedExpense(int id)
        {
            var transaction = await _context.Transactions.FindAsync(id);
            if (transaction != null)
            {
                _context.Transactions.Remove(transaction);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Fixed expense deleted successfully!";
            }
            return RedirectToAction("FixedExpense");
        }
    }
}