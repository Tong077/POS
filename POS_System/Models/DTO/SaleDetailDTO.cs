namespace POS_System.Models.DTO
{
    public class SaleDetailDTO
    {
        public int? ProductID { get; set; }
        public decimal? Quantity { get; set; }
        public decimal? Price { get; set; }
        public decimal? TotalAmount { get; set; }
    }
}
