using Microsoft.AspNetCore.Identity;

namespace POS_System.Models
{
    public class ApplicationRole
    {
        public int Id { get; set; }
        public Guid RoleId { get; set; }
        public string? RoleName { get; set; }
        public string? Description { get; set; }
        public bool IsActive { get; set; }
    }
}
