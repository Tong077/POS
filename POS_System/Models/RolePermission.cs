using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace POS_System.Models
{
    public class RolePermission
    {
        [Key]
        public int RolePermissionId { get; set; }

        [ForeignKey("ApplicationUser")]
        public string? UserID { get; set; }

        [ForeignKey("Permission")]
        public int PermissionId { get; set; }
        public ApplicationUser? ApplicationUser { get; set; }
        public Permission? Permission { get; set; }
    }
}
