namespace POS_System.Models.DTO
{
    public class RolePermissionDto
    {
        public int RolePermissionId { get; set; }
        public string? UserID { get; set; }
        public string? DisplayUsername { get; set; }
        public int PermissionId { get; set; }
        public string? PermissionName { get; set; }
    }
}
