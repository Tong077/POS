namespace POS_System.Models.DTO
{
    public class SaleDTO
    {
        public int? CustomerID { get; set; } // Nullable for validation
        public string? Currency { get; set; }
        public List<SaleDetailDTO> SaleDetails { get; set; } = new();
    }
}
