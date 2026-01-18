using ExpenseTracker.Data;
using ExpenseTracker.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ExpenseTracker.Controllers
{
    public class IncomeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public IncomeController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Income Page
        public async Task<IActionResult> Index()
        {
            var incomes = await _context.Transactions
                .Where(t => t.Type == "Income")
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
                return RedirectToAction("Index");
            }
            
            // If validation fails, return to the same page
            var incomes = await _context.Transactions
                .Where(t => t.Type == "Income")
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
