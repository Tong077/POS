using Dapper;
using Microsoft.EntityFrameworkCore;
using POS_System.Data;
using POS_System.Models;

namespace POS_System.Service
{
    public class ProductService : IProductRepository
    {
        private readonly DapperConnection dapper;
        private readonly EntityConntext entityConntext;
        public ProductService(DapperConnection dapper, EntityConntext entityConntext)
        {
            this.dapper = dapper;
            this.entityConntext = entityConntext;
        }
        public async Task<bool> Create(Product product)
        {
            entityConntext.products.Add(product);
            return await entityConntext.SaveChangesAsync() > 0;
        }

        public async Task<bool> delete(Product product)
        {
            entityConntext.products.Remove(product);
            return await entityConntext.SaveChangesAsync() > 0;
        }

        public async Task<IEnumerable<Product>> GetAll()
        {
            var sql = @"
                        SELECT 
                                p.ProductId,
                                p.ProductName,
                                p.Description,
                                p.Image,
                                p.Price,
                                p.Currency,
                                p.StockQuantity,
                                c.CategoryId,
                                c.CategoryName,
                                s.SupplierId,
                                s.SupplierName
                        FROM 
                            Products p
                        LEFT JOIN 
                            Categories c ON p.CategoryId = c.CategoryId
                        LEFT JOIN 
                            Suppliers s ON p.SupplierId = s.SupplierId;
                    ";

           var products = await dapper.Connection.QueryAsync<Product, Category,Supplier, Product>(sql,
                (product, Category, Supplier) =>
                {
                    product.Category = Category;
                    product.Supplier = Supplier;
                    return product;
                },
                splitOn : "CategoryID,SupplierId");
            return products;
        }

        public async Task<Product?> GetById(int productId)
        {
            return await entityConntext.products
                .Include(x => x.Category) 
                .Include(x => x.Supplier)
                .FirstOrDefaultAsync(x => x.ProductId == productId);
        }

        public async Task<bool> update(Product product)
        {
            entityConntext.products.Update(product);
            return await entityConntext.SaveChangesAsync() > 0;
        }
    }
}
