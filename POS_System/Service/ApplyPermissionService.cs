
using Dapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Build.Framework;
using Microsoft.EntityFrameworkCore;
using POS_System.Data;
using POS_System.Models;
using POS_System.Models.DTO;
using static Dapper.SqlMapper;

namespace POS_System.Service
{
    public class ApplyPermissionService : IApplyPermissionRepository
    {
        private readonly EntityConntext _context;
        private readonly DapperConnection _dapper;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IHttpContextAccessor _httpContextAccessor;
        public ApplyPermissionService(EntityConntext context, DapperConnection dapperConnection, UserManager<ApplicationUser> userManager, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _dapper = dapperConnection;
            _userManager = userManager;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<bool> AddPermissionAsync(string userId, int permissionId)
        {
            var exists = await _context.RolePermissions
                .AnyAsync(rp => rp.UserID == userId && rp.PermissionId == permissionId);

            if (exists) return false;

            var rolePermission = new RolePermission
            {
                UserID = userId,
                PermissionId = permissionId
            };

            _context.RolePermissions.Add(rolePermission);
            return await _context.SaveChangesAsync() > 0;
        }

        

        public async Task<bool> DeleteAsync(int id, string userId, int permissionId)
        {
            var rolePermission = await _context.RolePermissions.FindAsync(id);
            if (rolePermission == null)
                return false;

            var permission = await _context.Permissions.FindAsync(permissionId);
            if (permission == null)
                return false;

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return false;

            rolePermission.UserID = userId;
            rolePermission.PermissionId = permissionId;

            _context.RolePermissions.Remove(rolePermission);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> EditAsync(int id, string userId, int permissionId)
        {
            var rolePermission = await _context.RolePermissions.FindAsync(id);
            if (rolePermission == null)
                return false;

            var permission = await _context.Permissions.FindAsync(permissionId);
            if (permission == null)
                return false;

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return false;

            rolePermission.UserID = userId;
            rolePermission.PermissionId = permissionId;

            _context.RolePermissions.Update(rolePermission);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<RolePermissionDto>> GetAllAsync()
        {
            return await _context.RolePermissions
                .Join(_context.Permissions,
                    rp => rp.PermissionId,
                    p => p.PermissionId,
                    (rp, p) => new { rp, p })
                .Join(_context.Users,
                    x => x.rp.UserID,
                    u => u.Id,
                    (x, u) => new RolePermissionDto
                    {
                        RolePermissionId = x.rp.RolePermissionId,
                        UserID = x.rp.UserID,
                        DisplayUsername = u.UserName,
                        PermissionId = x.rp.PermissionId,
                        PermissionName = x.p.Name
                    })
                .ToListAsync();
        }

        public async Task<RolePermission> GetByIdAsync(int id)
        {
            var permission = await _context.RolePermissions
                .Include(rp => rp.Permission)
                .FirstOrDefaultAsync(rp => rp.RolePermissionId == id);

            return permission!;
        }

        public async Task<List<string>> GetUserPermissionsAsync(string userId)
        {
            return await _context.RolePermissions
                .Where(rp => rp.UserID == userId)
                .Join(_context.Permissions,
                    rp => rp.PermissionId,
                    p => p.PermissionId,
                    (rp, p) => p.Name)
                .ToListAsync();
        }


        public async Task<bool> HasPermissionAsync(string userId, string permission)
        {
            var result = await _context.RolePermissions
                .Join(_context.Permissions,
                    rp => rp.PermissionId,
                    p => p.PermissionId,
                    (rp, p) => new { rp.UserID, p.Name })
                .AnyAsync(x => x.UserID == userId && x.Name == permission);
            System.Diagnostics.Debug.WriteLine($"HasPermissionAsync: User {userId}, Permission {permission}, Result: {result}");
            return result;
        }


    }
}

