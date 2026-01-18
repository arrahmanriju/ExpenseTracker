using ExpenseTracker.Data;
using ExpenseTracker.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ExpenseTracker.Controllers
{
    public class ExpenseController : BaseController
    {
        private readonly ApplicationDbContext _context;

        public ExpenseController(ApplicationDbContext context) :base(context)
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
                return RedirectToAction("AddExpense");
            }
            return View(transaction);
        }

        // GET: Fixed Expense Page
        public async Task<IActionResult> FixedExpense()
        {
            var fixedExpenses = await _context.Transactions
                .Where(t => t.Type == "Expense" && t.IsFixed == true)
                .OrderByDescending(t => t.Date)
                .ToListAsync();

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
