using POS_System.Models;

namespace POS_System.Service
{
    public interface ICateogyRepository
    {
        Task<Category> GetByt(int categoryId);
        Task<IEnumerable<Category>> GetAll();
        Task<bool> addnew(List<Category> category);
        Task<bool> update(Category category);
        Task<bool> delete(Category category);
    }
}
