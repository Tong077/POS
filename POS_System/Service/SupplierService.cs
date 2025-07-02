using Dapper;
using Microsoft.EntityFrameworkCore;
using POS_System.Data;
using POS_System.Models;

namespace POS_System.Service
{
    public class SupplierService : ISupplierRepository
    {
        private readonly DapperConnection dapper;
        private readonly EntityConntext context;
        public SupplierService(DapperConnection dapper, EntityConntext context)
        {
            this.dapper = dapper;
            this.context = context;
        }

        public async Task<bool> Addrang(List<Supplier> suppliers)
        {
            context.suppliers.AddRange(suppliers);
            return await context.SaveChangesAsync() > 0;

        }


        public async Task<bool> Delete(Supplier supplier)
        {
            context.suppliers.Remove(supplier);
            return await context.SaveChangesAsync() > 0;
        }

        public async Task<IEnumerable<Supplier>> GetAll()
        {
            var sql = "SELECT * FROM suppliers";
            var supplier = dapper.Connection.QueryAsync<Supplier>(sql);
            return await supplier;
        }

        public async Task<Supplier?> GetById(int SupplierId)
        {
            var supplier = await context.suppliers.FirstOrDefaultAsync(x => x.SupplierId == SupplierId);
            return supplier;
        }

        public async Task<bool> Update(Supplier supplier)
        {
            context.suppliers.Update(supplier);
            return await context.SaveChangesAsync() > 0;

        }
    }
}
