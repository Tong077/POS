using System.ComponentModel.DataAnnotations;

namespace POS_System.Models
{
    public class Permission
    {
         [Key]
        public int PermissionId { get; set; }
        public string? Name { get; set; }
    }
}
