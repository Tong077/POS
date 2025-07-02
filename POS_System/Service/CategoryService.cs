using Dapper;
using Microsoft.EntityFrameworkCore;
using POS_System.Data;
using POS_System.Models;

namespace POS_System.Service
{
    public class CategoryService : ICateogyRepository
    {
        private readonly DapperConnection dapper;
        private readonly EntityConntext entity;
        public CategoryService(DapperConnection dapper, EntityConntext entity)
        {
            this.dapper = dapper;
            this.entity = entity;
        }

        public async Task<bool> addnew(List<Category> category)
        {
          await entity.categories.AddRangeAsync(category);
            return await entity.SaveChangesAsync() > 0;
        }

        public async Task<bool> delete(Category category)
        {
            entity.categories.Remove(category);
            return await entity.SaveChangesAsync() > 0;
        }

        public async Task<IEnumerable<Category>> GetAll()
        {
            var sql = "SELECT * FROM Categories";
            var cate= dapper.Connection.QueryAsync<Category>(sql);
            return await cate;

        }

        public async Task<Category> GetByt(int categoryId)
        {
            var cate = entity.categories.FirstOrDefaultAsync(x => x.CategoryId == categoryId);
            return await cate;
        }

        public async Task<bool> update(Category category)
        {
            entity.categories.Update(category);
            return await entity.SaveChangesAsync() > 0;
        }
    }
}
