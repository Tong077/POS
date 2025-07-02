using System.ComponentModel.DataAnnotations.Schema;

namespace POS_System.Models
{
    public class Category
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int CategoryId { get; set; }
        public string CategoryName { get; set; }
        public string? Description { get; set; }

       
        public ICollection<Product>? Products { get; set; }
    }
}
