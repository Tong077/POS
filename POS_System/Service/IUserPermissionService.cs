using System.Security.Claims;

namespace POS_System.Service
{
    public interface IUserPermissionService
    {
        Task<List<string>> GetUserPermissionsAsync(string userId);
        Task<List<Claim>> GetUserClaimsAsync(string userId);
        Task<bool> HasPermissionAsync(string userId, string permission);
        Task RefreshUserPermissionsAsync(string userId);
    }
}
