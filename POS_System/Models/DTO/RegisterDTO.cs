using System.ComponentModel.DataAnnotations;

namespace POS_System.Models.DTO
{
    public class RegisterDTO
    {
        [Required(ErrorMessage = "user name is required.")]
        public string? Username { get; set; }

        [Required(ErrorMessage = "email is required.")]
        [EmailAddress(ErrorMessage = "Invalid email address.")]
        public string? Email { get; set; }

        [Required(ErrorMessage = "password is required.")]
        [StringLength(100, ErrorMessage = "Password must be at least 6 characters long.", MinimumLength = 6)]
        public string Password { get; set; }

        public string? SelectedRole { get; set; }
        public IFormFile? Image { get; set; } 
    }
}
