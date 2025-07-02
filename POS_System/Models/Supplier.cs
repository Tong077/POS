using System.ComponentModel.DataAnnotations.Schema;

namespace POS_System.Models
{
    public class Supplier
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int SupplierId { get; set; }
        public string SupplierName { get; set; }
        
        public string? Phone { get; set; }
        public string? Email { get; set; }
        public string? Address { get; set; }

     
        public ICollection<Product>? Products { get; set; }
      
    }
}
