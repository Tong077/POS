using Dapper;
using Microsoft.AspNetCore.Http;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using POS_System.Data;
using POS_System.Models;
using System.Security.Claims;

namespace POS_System.Service
{
    public class PosService : IPosRepository
    {
        private readonly EntityConntext _context;
        private readonly DapperConnection _dapper;
        private readonly IInventoryRepository _invenotry;
        private readonly IHttpContextAccessor _httpContextAccessor;
        public PosService(EntityConntext context, DapperConnection dapper, IInventoryRepository inventory, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _dapper = dapper;
            _invenotry = inventory;
            _httpContextAccessor = httpContextAccessor;
        }



        public async Task<Sale?> GetSaleWithDetailsAsync(int id)
        {
            return await _context.sales
               .Include(s => s.SaleDetails!)
                   .ThenInclude(sd => sd.Product)
               .Include(s => s.Customer)
               .FirstOrDefaultAsync(s => s.SaleID == id);
        }


        public async Task<IEnumerable<Sale>> GetAllSalesAsync()
        {
            return await _context.sales
             .Include(s => s.Customer)
             .Include(s => s.SaleDetails)
             .ThenInclude(sd => sd.Product)
             .ToListAsync();
        }

        public async Task<Sale?> GetSaleByIdAsync(int saleId)
        {
            return await _context.sales
             .Include(s => s.SaleDetails)
             .ThenInclude(sd => sd.Product)
             .FirstOrDefaultAsync(s => s.SaleID == saleId);
        }

        //public async Task<(bool Success, string ErrorMessage)> CreateSaleAsync(Sale sale, List<SaleDetail> saleDetails)
        //{
        //    using var transaction = await _context.Database.BeginTransactionAsync();
        //    try
        //    {

        //        var userId = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier).Value;

        //        sale.UserId = userId;
        //        sale.TotalAmount = saleDetails?.Sum(d => d.TotalAmount) ?? 0;

        //        // Add Sale
        //        await _context.sales.AddAsync(sale);
        //        await _context.SaveChangesAsync();

        //        // Assign SaleID and add SaleDetails (if any)
        //        if (saleDetails != null && saleDetails.Any())
        //        {
        //            foreach (var detail in saleDetails)
        //            {
        //                detail.SaleID = sale.SaleID; 
        //            }
        //            await _context.saleDetails.AddRangeAsync(saleDetails);

        //            // Update Stock using DecreaseStock from inventory service
        //            foreach (var detail in saleDetails)
        //            {
        //                if (detail?.ProductID.HasValue == true && detail?.Quantity.HasValue == true)
        //                {
        //                    bool stockDecreased = await _invenotry.DecreaseStock(
        //                        detail.ProductID.Value,
        //                        detail.Quantity.Value
        //                    );

        //                    if (!stockDecreased)
        //                    {
        //                        throw new Exception($"Failed to decrease stock for ProductID: {detail.ProductID}");
        //                    }
        //                }
        //            }
        //        }

        //        await _context.SaveChangesAsync();
        //        await transaction.CommitAsync();
        //        return (true, null);
        //    }
        //    catch (Exception ex)
        //    {
        //        await transaction.RollbackAsync();
        //        return (false, $"Failed to create sale: {ex.Message}");
        //    }
        //}

        public async Task<(bool Success, string ErrorMessage)> CreateSaleAsync(Sale sale, List<SaleDetail> saleDetails)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var userId = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    throw new UnauthorizedAccessException("User is not authenticated.");
                }

                sale.UserId = userId;
                sale.TotalAmount = saleDetails?.Sum(d => d.TotalAmount) ?? 0;

                await _context.sales.AddAsync(sale);
                await _context.SaveChangesAsync();

                if (saleDetails != null && saleDetails.Any())
                {
                    foreach (var detail in saleDetails)
                    {
                        detail.SaleID = sale.SaleID;
                    }
                    await _context.saleDetails.AddRangeAsync(saleDetails);

                    foreach (var detail in saleDetails)
                    {
                        if (detail?.ProductID.HasValue == true && detail?.Quantity.HasValue == true)
                        {
                            bool stockDecreased = await _invenotry.DecreaseStock(
                                detail.ProductID.Value,
                                detail.Quantity.Value
                            );

                            if (!stockDecreased)
                            {
                                throw new Exception($"Failed to decrease stock for ProductID: {detail.ProductID}");
                            }
                        }
                    }
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                return (true, null);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return (false, $"Failed to create sale: {ex.Message}");
            }
        }

        public async Task<IEnumerable<SaleDetail>> GetAll()
        {
            var sql = @"
                            SELECT 
                              
                                sd.SaleDetailID,
                                sd.Quantity,
                                sd.TotalAmount,
                                sd.Price,

                               
                                s.SaleID,
                                s.SaleDate,
                                s.CustomerID,
                                s.UserId,
                                s.Currency,

                               
                                c.CustomerID,
                                c.CustomerName,

                               
                                p.ProductID,
                                p.ProductName, 
                                p.Image,

                             
                                u.Id AS UserId,
                                u.DisplayUsername

                            FROM saleDetails sd
                            LEFT JOIN sales s ON sd.SaleID = s.SaleID
                            LEFT JOIN customers c ON s.CustomerID = c.CustomerID
                            LEFT JOIN products p ON sd.ProductID = p.ProductID
                            LEFT JOIN AspNetUsers u ON s.UserId = u.Id";


            var saleDetails = await _dapper.Connection.QueryAsync<SaleDetail, Sale, Customer, Product, ApplicationUser, SaleDetail>(
                 sql,
                 (saleDetail, sale, customer, product, user) =>
                 {
                     saleDetail.Sale = sale;
                     if (sale != null)
                     {
                         sale.Customer = customer;
                         sale.ApplicationUser = user;
                     }
                     saleDetail.Product = product;
                     return saleDetail;
                 },
                 splitOn: "SaleID,CustomerID,ProductID,UserId"
             );


            return saleDetails;
        }

        public async Task<int> GetTotalProductsSold(int month, int year)
        {
            var totalProducts = (from sale in _context.sales
                                 join saleDetail in _context.saleDetails
                                 on sale.SaleID equals saleDetail.SaleID
                                 where sale.SaleDate.Month == month && sale.SaleDate.Year == year
                                 select saleDetail.Quantity ?? 0).Sum();

            return (int)totalProducts;
        }

        public async Task<decimal> GetProfit(int month, int year, string currency)
        {
            var totalAmount = await _context.saleDetails
                .Where(sd => sd.Sale != null &&
                             sd.Sale.SaleDate.Month == month &&
                             sd.Sale.SaleDate.Year == year &&
                             sd.Sale.Currency == currency)
                .SumAsync(sd => sd.TotalAmount ?? 0);

            return totalAmount;
        }

        public async Task<List<ProductSales>> GetProductSalesData(int month, int year)
        {
            var productSales = await (from sale in _context.sales
                                      join saleDetail in _context.saleDetails
                                      on sale.SaleID equals saleDetail.SaleID
                                      join product in _context.products
                                      on saleDetail.ProductID equals product.ProductId
                                      where sale.SaleDate.Month == month && sale.SaleDate.Year == year
                                      group new { saleDetail, product } by product.ProductName into g
                                      select new ProductSales
                                      {
                                          ProductName = g.Key,
                                          Quantity = g.Sum(x => x.saleDetail.Quantity ?? 0)
                                      })
                                     .OrderBy(s => s.ProductName)
                                     .ToListAsync();
            return productSales;
        }
        public async Task<List<DailySales>> GetDailySalesData(int month, int year)
        {
            var dailySales = await (from sale in _context.sales
                                    join saleDetail in _context.saleDetails
                                    on sale.SaleID equals saleDetail.SaleID
                                    where sale.SaleDate.Month == month && sale.SaleDate.Year == year
                                    group saleDetail by sale.SaleDate.Day into g
                                    select new DailySales
                                    {
                                        Date = new DateTime(year, month, g.Key),
                                        Quantity = g.Sum(sd => sd.Quantity ?? 0)
                                    })
                                    .OrderBy(s => s.Date)
                                    .ToListAsync();

            // Ensure all days in the month are represented (fill gaps with 0)
            var daysInMonth = DateTime.DaysInMonth(year, month);
            var allDays = Enumerable.Range(1, daysInMonth).Select(day => new DailySales
            {
                Date = new DateTime(year, month, day),
                Quantity = dailySales.FirstOrDefault(s => s.Date.Day == day)?.Quantity ?? 0
            }).OrderBy(s => s.Date).ToList();

            return allDays;
        }
    }
}