namespace POS_System.Models.DTO
{
    public class UserViewModel
    {
        public string Id { get; set; }
        public string Username { get; set; }
        public string? DisplayUsername { get; set; }
        public string Email { get; set; }
        public string ImagePath { get; set; }
        public List<string> Roles { get; set; } = new List<string>();
    }
}
