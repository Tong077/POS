using System.ComponentModel.DataAnnotations.Schema;

namespace POS_System.Models
{
    public class Customer
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int CustomerId { get; set; }
        public string CustomerName { get; set; }
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public string? Address { get; set; }


        public List<Sale>? Sales { get; set; }

    }
}
