using POS_System.Models;

namespace POS_System.Service
{
    public interface ICustomerRepository
    {
        Task<Customer> GetById(int customerId); 
        Task<IEnumerable<Customer>> GetAll();
        Task<bool> addnew(Customer customer);
        Task<bool> update(Customer customer);
        Task<bool> delete(Customer customer);
    }
}
