using POS_System.Models;

namespace POS_System.Service
{
    public interface IPosRepository
    {
        Task<(bool Success, string ErrorMessage)> CreateSaleAsync(Sale sale, List<SaleDetail> saleDetails);
        Task<Sale?> GetSaleByIdAsync(int saleId);
        Task<IEnumerable<Sale>> GetAllSalesAsync();
        Task<Sale?> GetSaleWithDetailsAsync(int id);

        Task<IEnumerable<SaleDetail>> GetAll();
        Task<int> GetTotalProductsSold(int month, int year);

        Task<decimal> GetProfit(int month, int year, string currency);
        Task<List<ProductSales>> GetProductSalesData(int month, int year);
        Task<List<DailySales>> GetDailySalesData(int month, int year);

    }
}
