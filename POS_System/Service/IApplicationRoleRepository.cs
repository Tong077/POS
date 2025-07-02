using POS_System.Models;

namespace POS_System.Service
{
    public interface IApplicationRoleRepository
    {
        Task<IEnumerable<ApplicationRole>> GetAllRolesAsync();
        Task<ApplicationRole> GetRoleByIdAsync(Guid id);
        Task<bool> create(ApplicationRole applicationRole);
        Task<bool> update(ApplicationRole applicationRole);
        Task<bool> delete(ApplicationRole applicationRole);


    }
}
