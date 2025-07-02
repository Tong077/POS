using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace POS_System.Models
{
    public class Product
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ProductId { get; set; }
        public string? ProductName { get; set; }
        public string? Description { get; set; }
        public string? Image { get; set; }
        public string? Currency { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Price must be a positive number.")]
        public decimal? Price { get; set; }

        [ForeignKey("Category")]
        public int? CategoryId { get; set; }

        [ForeignKey("Supplier")]
        public int? SupplierId { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Stock Quantity must be a positive number.")]
        public decimal? StockQuantity { get; set; }

        public Category? Category { get; set; }
        public Supplier? Supplier { get; set; }

        public Inventory? Inventory { get; set; }


      
        public ICollection<SaleDetail>? SaleDetails { get; set; }
      
    }

}
