using Dapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using POS_System.Data;
using POS_System.Models;
using POS_System.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace POS_System.Service
{
    public class ApplicationRoleService : IApplicationRoleRepository
    {
        private readonly EntityConntext _context;
        private readonly DapperConnection _dapper;
        private readonly IPolicyRepositovy _policyManager;

        private readonly ILogger<ApplicationRoleService> _logger;

        public ApplicationRoleService(
            EntityConntext context,
            DapperConnection dapper,

            ILogger<ApplicationRoleService> logger, IPolicyRepositovy policy)
        {
            _context = context;
            _dapper = dapper;
            _policyManager = policy;
            _logger = logger;
        }

        public async Task<bool> create(ApplicationRole applicationRole)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                _logger.LogInformation($"Creating role: {applicationRole.RoleName}");

                // Check for duplicate role name in ApplicationRoles
                var existingRole = await _context.ApplicationRoles
                    .FirstOrDefaultAsync(r => r.RoleName.ToLower() == applicationRole.RoleName.ToLower());

                if (existingRole != null)
                {
                    _logger.LogWarning($"Role '{applicationRole.RoleName}' already exists.");
                    await transaction.RollbackAsync();
                    return false;
                }

                // Generate a single GUID for both ApplicationRole and AspNetRole
                var roleId = Guid.NewGuid();
                var roleIdString = roleId.ToString();

                // Add to AspNetRoles first (Identity table)
                var aspNetRole = new IdentityRole
                {
                    Id = roleIdString,
                    Name = applicationRole.RoleName,
                    NormalizedName = applicationRole.RoleName.ToUpper(),
                    ConcurrencyStamp = Guid.NewGuid().ToString()
                };

                _context.Roles.Add(aspNetRole);
                var efResult1 = await _context.SaveChangesAsync() > 0;
                if (!efResult1)
                {
                    _logger.LogError($"Failed to create role in AspNetRoles: {applicationRole.RoleName}");
                    await transaction.RollbackAsync();
                    return false;
                }

                // Add to ApplicationRoles with the SAME Id
                applicationRole.RoleId = roleId;
                _context.ApplicationRoles.Add(applicationRole);
                var efResult2 = await _context.SaveChangesAsync() > 0;
                if (!efResult2)
                {
                    _logger.LogError($"Failed to create role in ApplicationRoles: {applicationRole.RoleName}");
                    await transaction.RollbackAsync();
                    return false;
                }

                // Create corresponding policy
                string policyName = $"{applicationRole.RoleName}Only";
                _logger.LogInformation($"Creating policy: {policyName}");

                _policyManager.AddPolicy(policyName, policyBuilder =>
                {
                    policyBuilder.RequireRole(applicationRole.RoleName);
                });

                await transaction.CommitAsync();
                _logger.LogInformation($"Role '{applicationRole.RoleName}' and policy '{policyName}' created successfully.");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error creating role '{applicationRole.RoleName}' or policy.");
                await transaction.RollbackAsync();
                throw;
            }
        }


        public async Task<bool> delete(ApplicationRole applicationRole)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                _logger.LogInformation($"Deleting role: {applicationRole.RoleName}");

                // Delete corresponding policy
                string policyName = $"{applicationRole.RoleName}Only";
                _logger.LogInformation($"Deleting policy: {policyName}");


                // Remove from ApplicationRoles
                _context.ApplicationRoles.Remove(applicationRole);
                var efResult1 = await _context.SaveChangesAsync() > 0;
                if (!efResult1)
                {
                    _logger.LogError($"Failed to delete role from ApplicationRoles: {applicationRole.RoleName}");
                    await transaction.RollbackAsync();
                    return false;
                }

                // Remove from AspNetRoles
                var aspNetRole = await _context.Roles
                    .FirstOrDefaultAsync(r => r.Id == applicationRole.Id.ToString());
                if (aspNetRole != null)
                {
                    _context.Roles.Remove(aspNetRole);
                    var efResult2 = await _context.SaveChangesAsync() > 0;
                    if (!efResult2)
                    {
                        _logger.LogError($"Failed to delete role from AspNetRoles: {applicationRole.RoleName}");
                        await transaction.RollbackAsync();
                        return false;
                    }
                }

                await transaction.CommitAsync();
                _logger.LogInformation($"Role '{applicationRole.RoleName}' and policy '{policyName}' deleted successfully.");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting role '{applicationRole.RoleName}' or policy.");
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<IEnumerable<ApplicationRole>> GetAllRolesAsync()
        {
            _logger.LogInformation("Retrieving all roles.");
            var sql = "SELECT * FROM ApplicationRoles";
            var role = await _dapper.Connection.QueryAsync<ApplicationRole>(sql);
            return role;
        }

        public async Task<ApplicationRole> GetRoleByIdAsync(Guid id)
        {
            _logger.LogInformation($"Retrieving role by ID: {id}");
            return await _context.ApplicationRoles.FirstOrDefaultAsync(x => x.RoleId == id);
        }

        public async Task<bool> update(ApplicationRole applicationRole)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                _logger.LogInformation($"Updating role: {applicationRole.RoleName}");


                var originalRole = await _context.ApplicationRoles
                    .AsNoTracking()
                    .FirstOrDefaultAsync(r => r.Id == applicationRole.Id);
                if (originalRole == null)
                {
                    _logger.LogError($"Role with ID '{applicationRole.Id}' not found.");
                    await transaction.RollbackAsync();
                    return false;
                }

                // Update ApplicationRoles
                _context.ApplicationRoles.Update(applicationRole);
                var efResult1 = await _context.SaveChangesAsync() > 0;
                if (!efResult1)
                {
                    _logger.LogError($"Failed to update role in ApplicationRoles: {applicationRole.RoleName}");
                    await transaction.RollbackAsync();
                    return false;
                }

                // Update AspNetRoles
                var aspNetRole = await _context.Roles
                    .FirstOrDefaultAsync(r => r.Id == applicationRole.Id.ToString());
                if (aspNetRole != null)
                {
                    aspNetRole.Name = applicationRole.RoleName;
                    aspNetRole.NormalizedName = applicationRole.RoleName.ToUpper();
                    _context.Roles.Update(aspNetRole);
                    var efResult2 = await _context.SaveChangesAsync() > 0;
                    if (!efResult2)
                    {
                        _logger.LogError($"Failed to update role in AspNetRoles: {applicationRole.RoleName}");
                        await transaction.RollbackAsync();
                        return false;
                    }
                }

                // Update policy if RoleName changed
                if (originalRole.RoleName != applicationRole.RoleName)
                {
                    string oldPolicyName = $"{originalRole.RoleName}Only";
                    string newPolicyName = $"{applicationRole.RoleName}Only";
                    _logger.LogInformation($"Updating policy from '{oldPolicyName}' to '{newPolicyName}'");

                }

                await transaction.CommitAsync();
                _logger.LogInformation($"Role '{applicationRole.RoleName}' updated successfully.");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating role '{applicationRole.RoleName}' or policy.");
                await transaction.RollbackAsync();
                throw;
            }
        }
    }
}