using Dapper;
using Microsoft.EntityFrameworkCore;
using POS_System.Data;
using POS_System.Models;

namespace POS_System.Service
{
    public class InventoryService : IInventoryRepository
    {
        private readonly DapperConnection dapper;
        private readonly EntityConntext entityConntext;
        private readonly ICurrencyRepository _currency;
        public InventoryService(DapperConnection dapper, EntityConntext entityConntext, ICurrencyRepository currency)
        {
            this.dapper = dapper;
            this.entityConntext = entityConntext;
            _currency = currency;
        }

        public async Task<bool> Checkstock(int productId, decimal quantity)
        {
            if (productId < 0 || quantity < 0)
            {
                return false;
            }
            var stock = await entityConntext.inventories
                .Where(i => i.ProductId == productId)
                .Select(i => i.StockQuantity)
                .FirstOrDefaultAsync();
            return stock >= quantity;
        }

        public async Task<bool> DecreaseStock(int productId, decimal quantity)
        {
            try
            {
                var inventory = await entityConntext.inventories.FirstOrDefaultAsync(x => x.ProductId == productId);
                if (inventory == null)
                    throw new KeyNotFoundException("Product not found in inventory.");

                if (inventory.StockQuantity < quantity)
                    throw new InvalidOperationException("Insufficient stock!");

                inventory.StockQuantity -= quantity;
                inventory.LastUpdated = DateTime.UtcNow;

                return await entityConntext.SaveChangesAsync() > 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                return false;
            }
        }



        public async Task<IEnumerable<Inventory>> GetAll()
        {
            var inventory = await entityConntext.inventories.AsQueryable()
                  .Include(x => x.Product).ToListAsync();
            return inventory;
        }

        public async Task<Inventory> GetInventoryByIdAsync(int productId)
        {
            return await entityConntext.inventories
                .FirstOrDefaultAsync(x => x.ProductId == productId);
        }

        // InventoryService.cs or Repository
        public async Task<decimal> GetStockQuantity(int productId)
        {
            Console.WriteLine($"Fetching stock quantity for ProductId: {productId}");

            var inventory = await entityConntext.inventories
                .FirstOrDefaultAsync(x => x.ProductId == productId);

            if (inventory == null)
            {
                Console.WriteLine($"No inventory record found for ProductId: {productId}");
                return 0;
            }

            Console.WriteLine($"Stock Quantity Found: {inventory.StockQuantity}");
            return inventory.StockQuantity;
        }

        public async Task<(bool Success, decimal TotalCostPrice, string Currency)> IncreaseStock(List<(int ProductId, decimal Quantity, decimal CostPrice, string Currency, DateTime TransactionDate)> inventoryItems)
        {
            using var transaction = await entityConntext.Database.BeginTransactionAsync();
            try
            {
                decimal combinedTotalCostPrice = 0;
                string finalCurrency = null;

                foreach (var item in inventoryItems)
                {
                    var (productId, quantity, costPrice, currency, transactionDate) = item;

                    if (quantity <= 0) throw new ArgumentException("Quantity must be greater than zero.");
                    if (costPrice < 0) throw new ArgumentException("Cost price cannot be negative.");

                    var inventory = await entityConntext.inventories.FirstOrDefaultAsync(x => x.ProductId == productId);
                    decimal totalCostPrice;

                    if (inventory == null)
                    {
                        inventory = new Inventory
                        {
                            ProductId = productId,
                            StockQuantity = quantity,
                            CostPrice = costPrice,
                            TotalCost = quantity * costPrice,
                            Currency = currency,
                            LastUpdated = transactionDate
                        };
                        entityConntext.inventories.Add(inventory);
                        totalCostPrice = inventory.TotalCost.Value;
                        finalCurrency = currency;
                    }
                    else
                    {
                        string existingCurrency = inventory.Currency ?? currency;
                        decimal costPriceInExistingCurrency = costPrice;

                        if (currency != existingCurrency)
                        {
                            decimal exchangeRate = await _currency.GetExchangeRateAsync(currency, existingCurrency, transactionDate);
                            costPriceInExistingCurrency = costPrice * exchangeRate;
                        }

                        decimal oldStock = inventory.StockQuantity;
                        decimal oldCostPrice = inventory.CostPrice ?? 0;
                        decimal newStock = oldStock + quantity;
                        if (newStock <= 0) throw new ArgumentException("Total stock quantity cannot be zero or negative.");

                        decimal newCostPrice = ((oldStock * oldCostPrice) + (quantity * costPriceInExistingCurrency)) / newStock;
                        decimal oldTotalCost = oldStock * oldCostPrice;
                        decimal newTotalCost = quantity * costPriceInExistingCurrency;
                        totalCostPrice = oldTotalCost + newTotalCost;

                        inventory.StockQuantity = newStock;
                        inventory.CostPrice = Math.Round(newCostPrice, 2);
                        inventory.TotalCost = Math.Round(totalCostPrice, 2);
                        inventory.Currency = existingCurrency;
                        inventory.LastUpdated = transactionDate;

                        finalCurrency = existingCurrency;
                    }

                    combinedTotalCostPrice += totalCostPrice;
                    if (finalCurrency == null) finalCurrency = inventory.Currency;
                    else if (finalCurrency != inventory.Currency) throw new InvalidOperationException("All items must use the same currency for combined total.");
                }

                await entityConntext.SaveChangesAsync();
                await transaction.CommitAsync();
                return (true, combinedTotalCostPrice, finalCurrency);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                Console.WriteLine($"Error: {ex.Message}");
                return (false, 0, inventoryItems.FirstOrDefault().Currency ?? "USD");
            }
        }

        // Single-item overload (optional)
        public async Task<(bool Success, decimal TotalCostPrice, string Currency)> IncreaseStock(int productId, decimal quantity, decimal costPrice, string currency)
        {
            var inventoryItems = new List<(int ProductId, decimal Quantity, decimal CostPrice, string Currency, DateTime TransactionDate)>
            {
                (productId, quantity, costPrice, currency, DateTime.UtcNow)
            };
            return await IncreaseStock(inventoryItems);
        }


        public async Task<bool> IsLowStock(int productId, decimal threshold)
        {
            var stock = await GetStockQuantity(productId);
            return stock <= threshold;
        }

        public async Task<(bool Success, decimal TotalCostPrice, string Currency)> UpdateInventoryAsync(int productId, decimal newStockQuantity, decimal newCostPrice, string newCurrency)
        {
            using var transaction = await entityConntext.Database.BeginTransactionAsync();
            try
            {
                var existingInventory = await GetInventoryByIdAsync(productId);
                if (existingInventory == null)
                {
                    throw new KeyNotFoundException("Product not found in inventory.");
                }

               
                if (newStockQuantity < 0)
                {
                    throw new ArgumentException("Stock quantity cannot be negative.");
                }
                if (newCostPrice < 0)
                {
                    throw new ArgumentException("Cost price cannot be negative.");
                }
                if (newCurrency != "USD" && newCurrency != "KHR")
                {
                    throw new ArgumentException("Currency must be either 'USD' or 'KHR'.");
                }

                // Get the old stock quantity and old cost price
                decimal oldStockQuantity = existingInventory.StockQuantity;
                decimal oldCostPrice = existingInventory.CostPrice ?? 0;

                // Calculate the new stock quantity (the difference)
                decimal quantityDifference = newStockQuantity - oldStockQuantity;

                if (quantityDifference > 0 && newCostPrice != oldCostPrice)
                {
                    // Weighted Average Cost Calculation
                    decimal totalOldCost = oldStockQuantity * oldCostPrice;
                    decimal totalNewCost = quantityDifference * newCostPrice;

                    decimal combinedQuantity = oldStockQuantity + quantityDifference;
                    decimal averageCost = Math.Round((totalOldCost + totalNewCost) / combinedQuantity, 2);

                    existingInventory.StockQuantity = combinedQuantity;
                    existingInventory.CostPrice = averageCost;
                    existingInventory.TotalCost = Math.Round(combinedQuantity * averageCost, 2);
                    existingInventory.Currency = newCurrency;
                    existingInventory.LastUpdated = DateTime.UtcNow;

                    await entityConntext.SaveChangesAsync();
                    await transaction.CommitAsync();

                    return (true, existingInventory.TotalCost.Value, newCurrency);
                }

                else
                {
                    // If no stock change, update cost and currency directly
                    existingInventory.CostPrice = Math.Round(newCostPrice, 2);
                    existingInventory.Currency = newCurrency;
                    existingInventory.TotalCost = Math.Round(newStockQuantity * newCostPrice, 2);
                    existingInventory.LastUpdated = DateTime.UtcNow;

                    await entityConntext.SaveChangesAsync();
                    await transaction.CommitAsync();
                    return (true, existingInventory.TotalCost.Value, newCurrency);
                }
            }
            catch (Exception ex)
            {
                // Rollback the transaction in case of an error
                await transaction.RollbackAsync();
                Console.WriteLine($"Error: {ex.Message}");

                // Log or rethrow the exception if necessary for further processing
                return (false, 0, "USD"); // Default currency in case of error
            }
        }


    }
}
