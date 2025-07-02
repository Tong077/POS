using System.ComponentModel.DataAnnotations;

namespace POS_System.Models.DTO
{
    public class LoginDTO
    {
        public string? Email { get; set; }
        public string? Password { get; set; }
        public string? ReturnUrl { get; set; } // Added property to fix CS0117 error  
    }
}
