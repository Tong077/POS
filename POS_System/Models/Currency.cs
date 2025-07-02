using System.ComponentModel.DataAnnotations.Schema;

namespace POS_System.Models
{
    public class Currency
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int CurrencyId { get; set; }
        public string? FromCurrency { get; set; } // e.g., "USD"
        public string? ToCurrency { get; set; }   // e.g., "KHR"
        public decimal? ExchangeRate { get; set; } // e.g., 4200 (1 USD = 4200 KHR)
        public DateTime EffectiveDate { get; set; } // When this rate takes effect
        public DateTime? EndDate { get; set; } // Nullable, null means the rate is effective indefinitely
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public bool IsBaseRate { get; set; } // Indicates if this is the user-entered rate
    }
}
