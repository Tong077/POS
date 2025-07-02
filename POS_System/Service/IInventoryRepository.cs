using POS_System.Models;

namespace POS_System.Service
{
    public interface IInventoryRepository
    {
       
        Task<IEnumerable<Inventory>> GetAll();
        Task<(bool Success, decimal TotalCostPrice, string Currency)> IncreaseStock(List<(int ProductId, decimal Quantity, decimal CostPrice, string Currency, DateTime TransactionDate)> inventoryItems);
        Task<bool> DecreaseStock(int productId, decimal quantity);
        Task<decimal> GetStockQuantity(int productId);
        Task<bool> IsLowStock(int productId, decimal threshold);

        Task<(bool Success, decimal TotalCostPrice, string Currency)> UpdateInventoryAsync(int productId,decimal newStockQuantity,decimal newCostPrice,string newCurrency);

        Task<Inventory> GetInventoryByIdAsync(int productId);
        Task<bool> Checkstock(int productId, decimal quantity);
    }
}
