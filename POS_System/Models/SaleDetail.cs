using System.ComponentModel.DataAnnotations.Schema;

namespace POS_System.Models
{
    public class SaleDetail
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int SaleDetailID { get; set; }

        [ForeignKey("Sale")]
        public int? SaleID { get; set; }
        [ForeignKey("Product")]
        public int? ProductID { get; set; }
        public decimal? Quantity { get; set; }
        public decimal? TotalAmount { get; set; }
        public decimal? Price { get; set; }


        public  Sale? Sale { get; set; }
        public  Product? Product { get; set; }
    }
}
