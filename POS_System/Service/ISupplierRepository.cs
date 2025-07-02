using POS_System.Models;

namespace POS_System.Service
{
    public interface ISupplierRepository
    {
        Task<Supplier> GetById(int SupplierId);
        Task<IEnumerable<Supplier>> GetAll();
      
        Task<bool> Addrang(List<Supplier> suppliers);
        Task<bool> Update(Supplier supplier);
        Task<bool> Delete(Supplier supplier);
    }
}
