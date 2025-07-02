namespace POS_System.Models.DTO
{
    public class CurrencyDTO
    {
        public string FromCurrency { get; set; }
        public string ToCurrency { get; set; }
        public decimal ExchangeRate { get; set; }
        public DateTime EffectiveDate { get; set; }
        public DateTime? EndDate { get; set; } // Nullable, null means the rate is effective indefinitely
        public bool IsBaseRate { get; set; } // Indicates if this is the user-entered rate
    
        // Helper property to format EffectiveDate for datetime-local input
        public string EffectiveDateString
        {
            get => EffectiveDate.ToString("yyyy-MM-ddTHH:mm");
            set => EffectiveDate = DateTime.Parse(value);
        }
    }
}
