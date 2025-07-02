using POS_System.Models;

namespace POS_System.Service
{
    public interface IProductRepository
    {
        Task<Product> GetById(int ProductId);
        Task<IEnumerable<Product>> GetAll();
        Task<bool> Create(Product product);
        Task<bool> update(Product product);
        Task<bool> delete(Product product);

    }
}
