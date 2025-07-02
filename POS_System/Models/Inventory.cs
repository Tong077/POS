using System.ComponentModel.DataAnnotations.Schema;

namespace POS_System.Models
{
    public class Inventory
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int InventoryId { get; set; }

        [ForeignKey("Product")]
        public int? ProductId { get; set; }
        public decimal StockQuantity { get; set; }
      
        public decimal? CostPrice { get; set; }
        public decimal? TotalCost { get; set; }

        public string? Currency { get; set; }

        public DateTime LastUpdated { get; set; }

      
        public Product? Product { get; set; }
    }
}
