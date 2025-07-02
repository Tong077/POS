using System.ComponentModel.DataAnnotations.Schema;

namespace POS_System.Models
{
    // Sales Models
    public class Sale
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int SaleID { get; set; }
        public DateTime SaleDate { get; set; } = DateTime.Now;
        public decimal? TotalAmount { get; set; }
        public string? Currency { get; set; }

        [ForeignKey("Customer")]
        public int? CustomerID { get; set; }
        [ForeignKey("ApplicationUser")]
        public string? UserId { get; set; }
       

        public  Customer? Customer { get; set; }
        public  ICollection<SaleDetail>? SaleDetails { get; set; }
        public ApplicationUser? ApplicationUser { get; set; }

    }

}
