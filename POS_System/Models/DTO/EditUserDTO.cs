namespace POS_System.Models.DTO
{
    public class EditUserDTO
    {
        public string? Id { get; set; }
        public string? Username { get; set; }
        public string? DisplayUsername { get; set; }
        public string? Email { get; set; }
        public string? SelectedRole { get; set; }
        public IFormFile? Image { get; set; }
        public string? CurrentImagePath { get; set; }
    }
}
