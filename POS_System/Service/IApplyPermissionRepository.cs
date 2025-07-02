using POS_System.Models;
using POS_System.Models.DTO;

namespace POS_System.Service
{
    public interface IApplyPermissionRepository
    {
        Task<List<string>> GetUserPermissionsAsync(string userId);
        Task<bool> HasPermissionAsync(string userId, string permission);
        Task<bool> DeleteAsync(int id, string userId, int permissionId);
        Task<bool> EditAsync(int id, string userId, int permissionId);
        Task<IEnumerable<RolePermissionDto>> GetAllAsync();
        Task<bool> AddPermissionAsync(string userId, int permissionId);
        Task<RolePermission> GetByIdAsync(int id);
        
    }
}
