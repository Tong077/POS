using Dapper;
using Microsoft.EntityFrameworkCore;
using POS_System.Data;
using POS_System.Models;

namespace POS_System.Service
{
    public class PermissionService : IPermissionRepository
    {
        private readonly EntityConntext _Context;
        private readonly DapperConnection _dapper;
        public PermissionService(EntityConntext context, DapperConnection dapper)
        {
            _Context = context;
            _dapper = dapper;
        }
        public async Task<bool> AddPermissionAsync(Permission permission)
        {
            _Context.Permissions.Add(permission);
            return await _Context.SaveChangesAsync() > 0;
        }

        public async Task<bool> DeletePermissionAsync(Permission permission)
        {
            _Context.Permissions.Remove(permission);
            return await _Context.SaveChangesAsync() > 0;
        }

        public async Task<IEnumerable<Permission>> GetAllPermissionsAsync()
        {
            var sql = "SELECT * FROM Permissions";
            var permissions = await _dapper.Connection.QueryAsync<Permission>(sql);
            return permissions;
        }

        public async Task<Permission?> GetPermissionByIdAsync(int id)
        {
            var result = await _Context.Permissions.FirstOrDefaultAsync(p => p.PermissionId == id);
            return result;
        }

        public async Task<bool> UpdatePermissionAsync(Permission permission)
        {
            _Context.Permissions.Update(permission);
            return await _Context.SaveChangesAsync() > 0;
        }
    }
}
