using Microsoft.AspNetCore.Identity;

namespace POS_System.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string? DisplayUsername { get; set; }
        public string? ImagePath { get; set; } 
        public ICollection<RolePermission>? Roles { get; set; }

    }
}
