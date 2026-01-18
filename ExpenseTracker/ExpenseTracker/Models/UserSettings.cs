using System.ComponentModel.DataAnnotations;

namespace ExpenseTracker.Models
{
    public class UserSettings
    {
        [Key]
        public int SettingsId { get; set; }

        [Required]
        public string UserName { get; set; } = "User";

        public string? Email { get; set; }

        public string? ProfilePicture { get; set; }

        
        public decimal MonthlyBudget { get; set; } = 0;

        [Required]
        public string Currency { get; set; } = "BDT";

        public string CurrencySymbol { get; set; } = "৳";

        public DateTime UpdatedAt { get; set; } = DateTime.Now;
    }
}
