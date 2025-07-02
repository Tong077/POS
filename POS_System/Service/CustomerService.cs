using Dapper;
using Microsoft.EntityFrameworkCore;
using POS_System.Data;
using POS_System.Models;

namespace POS_System.Service
{
    public class CustomerService : ICustomerRepository
    {
        private readonly DapperConnection dapper;
        private readonly EntityConntext entity;
        public CustomerService(DapperConnection dapper, EntityConntext entity)
        {
            this.dapper = dapper;
            this.entity = entity;
        }
        public async Task<bool> addnew(Customer customer)
        {
           entity.customers.Add(customer);
            return await entity.SaveChangesAsync() > 0;
        }

        public async Task<bool> delete(Customer customer)
        {
            entity.customers.Remove(customer);
            return await entity.SaveChangesAsync() > 0;
        }

        public async Task<IEnumerable<Customer>> GetAll()
        {
            var customer = dapper.Connection.QueryAsync<Customer>("SELECT * FROM Customers");
            return await customer;
        }

        public async Task<Customer> GetById(int customerId)
        {
           var cus =  entity.customers.FirstOrDefaultAsync(x => x.CustomerId == customerId);
            return await cus;
        }

        public async Task<bool> update(Customer customer)
        {
            entity.customers.Update(customer);
            return await entity.SaveChangesAsync() > 0;
        }
    }
}
