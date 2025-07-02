using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using POS_System.Data;
using POS_System.Models;
using POS_System.Service;

namespace POS_System.Controllers
{
    public class InventoryController : Controller
    {
        private readonly EntityConntext _context;
        private readonly IInventoryRepository _inventory;
        private readonly IProductRepository product;
        private readonly ICurrencyRepository _currency;
        public InventoryController(IInventoryRepository inventory, IProductRepository product, EntityConntext context,ICurrencyRepository currency)
        {
            _inventory = inventory;
            this.product = product;
            _context = context;
            _currency = currency;
        }
        public async Task <IActionResult> Index()
        {
            var inventory = await _inventory.GetAll();
            return View("Index", inventory);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var products = await this.product.GetAll();
            ViewBag.Products = new SelectList(products, "ProductId", "ProductName");

           
            var currencies = new List<SelectListItem>
    {
        new SelectListItem { Value = "KHR", Text = "KHR" },
        new SelectListItem { Value = "USD", Text = "USD" }
    };
            ViewBag.Currencies = new SelectList(currencies, "Value", "Text");

            return View("Create", new List<Inventory> { new Inventory() });
        }

        [HttpPost]
        public async Task<IActionResult> Store(List<Inventory> inventories)
        {
            var products = await this.product.GetAll();
            ViewBag.Products = new SelectList(products, "ProductId", "ProductName");

            var currencies = new List<SelectListItem>
    {
        new SelectListItem { Value = "KHR", Text = "KHR" },
        new SelectListItem { Value = "USD", Text = "USD" }
    };
            ViewBag.Currencies = new SelectList(currencies, "Value", "Text");

            if (!ModelState.IsValid)
            {
                return View("Create", inventories);
            }

           
            var validCurrencies = new List<string> { "KHR", "USD" };

            var inventoryItems = new List<(int ProductId, decimal Quantity, decimal CostPrice, string Currency, DateTime TransactionDate)>();
            foreach (var inventory in inventories)
            {
                if (string.IsNullOrEmpty(inventory.Currency) || !validCurrencies.Contains(inventory.Currency))
                {
                    ModelState.AddModelError("Currency", "Please select a valid currency (KHR or USD).");
                    return View("Create", inventories);
                }
                if (!inventory.ProductId.HasValue)
                {
                    ModelState.AddModelError("", "Product ID is required for all items.");
                    return View("Create", inventories);
                }
                inventoryItems.Add((inventory.ProductId.Value, inventory.StockQuantity, inventory.CostPrice ?? 0, inventory.Currency, DateTime.UtcNow));
            }

            var (success, totalCostPrice, finalCurrency) = await _inventory.IncreaseStock(inventoryItems);
            if (success)
            {
                //TempData["SuccessMessage"] = $"Inventory updated successfully! Total Cost Price: {totalCostPrice:F2} {finalCurrency}";
                TempData["toastr-type"] = "success";
                TempData["toastr-message"] = "Inventory Added successfully! Total Cost Price...!";
                return RedirectToAction("Index");
            }

          
            TempData["toastr-type"] = "wearning";
            TempData["toastr-message"] = "Failed to update inventory. Check your input and try again...!";
            return View("Create", inventories);
        }
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var inventory = await _inventory.GetInventoryByIdAsync(id);
            if (inventory == null)
            {
                return NotFound();
            }

            var products = await this.product.GetAll();
            var currencies = new List<SelectListItem>
    {
        new SelectListItem { Text = "USD", Value = "USD" },
        new SelectListItem { Text = "KHR", Value = "KHR" }
    };

            ViewBag.Products = new SelectList(products, "ProductId", "ProductName", inventory.ProductId);
            ViewBag.Currencies = currencies; 

            return View("Edit", inventory);
        }


        [HttpPost]
        public async Task<IActionResult> Update(Inventory inventory)
        {
            var products = await this.product.GetAll();
            ViewBag.Products = new SelectList(products, "ProductId", "ProductName", inventory.ProductId);

            if (!ModelState.IsValid)
            {
                return View("Edit", inventory);
            }

            var (success, totalCostPrice, finalCurrency) = await _inventory.UpdateInventoryAsync(
                inventory.ProductId!.Value,
                inventory.StockQuantity,
                inventory.CostPrice ?? 0,
                inventory.Currency!
            );

            if (success)
            {
                TempData["toastr-type"] = "success";
                TempData["toastr-message"] = "Inventory Added successfully! Total Cost Price...!";
                //TempData["SuccessMessage"] = $"Inventory updated successfully! Total Cost Price: {totalCostPrice:F2} {finalCurrency}";
                return RedirectToAction("Index");
            }
            TempData["toastr-type"] = "success";
            TempData["toastr-message"] = "Failed to update inventory. Please try again...!";
           
            return View("Edit", inventory);
        }


        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var inventory = await _inventory.GetInventoryByIdAsync(id);
            if (inventory == null)
            {
                return NotFound();
            }
           

            var products = await this.product.GetAll();
            var product = products.FirstOrDefault(p => p.ProductId == inventory.ProductId);
            ViewBag.ProductName = product?.ProductName ?? "Unknown Product";

            return View("Details", inventory);
        }

        [HttpGet]
        public async Task<IActionResult> GetCurrentExchangeRate()
        {
            var rate = await _currency.GetExchangeRateAsync("USD", "KHR", DateTime.UtcNow);
            return Json(new { exchangeRate = rate });
        }

        [HttpGet]
        public async Task<IActionResult> GetAllExchangeRates()
        {
            var rates = await _currency.GetAllCurrenciesAsync();
            var latestRates = rates
                .Where(c => c.EffectiveDate <= DateTime.UtcNow)
                .GroupBy(c => new { c.FromCurrency, c.ToCurrency })
                .Select(g => g.OrderByDescending(c => c.EffectiveDate).First())
                .ToDictionary(
                    c => $"{c.FromCurrency}-{c.ToCurrency}",
                    c => c.ExchangeRate
                );
            return Json(latestRates);
        }

        [HttpGet]
        public async Task<IActionResult> GetInventoryDetails(int productId)
        {
            try
            {
                var inventory = await _inventory.GetInventoryByIdAsync(productId);
                if (inventory == null)
                {
                    return Json(new { exists = false });
                }
                return Json(new
                {
                    exists = true,
                    stockQuantity = inventory.StockQuantity,
                    costPrice = inventory.CostPrice,
                    totalCost = inventory.TotalCost,
                    currency = inventory.Currency
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }


        [HttpGet]
        public async Task<IActionResult> GetLowStockNotifications()
        {
            var lowStockThreshold = 5;
            var lowStockItems = await _context.inventories
                .Where(i => i.StockQuantity <= lowStockThreshold)
                .Include(i => i.Product)
                .Select(i => new
                {
                    ProductName = i.Product != null ? i.Product.ProductName : "Unknown Product",
                    StockQuantity = i.StockQuantity,
                    ImageUrl = i.Product != null && !string.IsNullOrEmpty(i.Product.Image)
                        ? $"/images/{i.Product.Image}"
                        : "/images/default-product.png" // Fallback image
                })
                .ToListAsync();

            return Ok(lowStockItems);
        }

    }


}

