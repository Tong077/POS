using Microsoft.EntityFrameworkCore;
using POS_System.Data;
using System.Security.Claims;

namespace POS_System.Service
{
    public class UserPermissionService : IUserPermissionService
    {
        private readonly EntityConntext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public UserPermissionService(EntityConntext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<List<string>> GetUserPermissionsAsync(string userId)
        {
            var permissions = await _context.RolePermissions
                .Where(rp => rp.UserID == userId)
                .Include(rp => rp.Permission)
                .Select(rp => rp.Permission.Name)
                .Where(name => !string.IsNullOrEmpty(name))
                .ToListAsync();

            return permissions;
        }

        public async Task<List<Claim>> GetUserClaimsAsync(string userId)
        {
            var permissions = await GetUserPermissionsAsync(userId);
            var claims = new List<Claim>();

            foreach (var permission in permissions)
            {
                claims.Add(new Claim("Permission", permission));
            }

            return claims;
        }

        public async Task<bool> HasPermissionAsync(string userId, string permission)
        {
            return await _context.RolePermissions
                .Include(rp => rp.Permission)
                .AnyAsync(rp => rp.UserID == userId && rp.Permission.Name == permission);
        }

        public async Task RefreshUserPermissionsAsync(string userId)
        {
           
            var permissions = await GetUserPermissionsAsync(userId);
            var session = _httpContextAccessor.HttpContext?.Session;

            if (session != null)
            {
                session.SetString("UserPermissions", string.Join(",", permissions));
            }
        }
    }
}

