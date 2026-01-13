using System.ComponentModel.DataAnnotations;

namespace ExpenseTracker.Models
{
    public class Transaction
    {
        [Key]
        public int TransactionId { get; set; }

        [Required]
        public string Type { get; set; } // "Income" or "Expense"

        [Required]
        public string Category { get; set; } // e.g., "Salary", "Food", "Transport"

        [Required]
        public decimal Amount { get; set; }

        public string? Description { get; set; }

        [Required]
        public DateTime Date { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}