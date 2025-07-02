using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages.Manage;
using POS_System.Data;
using POS_System.Models;
using POS_System.Models.DTO;
using POS_System.Service;
using System.Linq.Expressions;
using System.Text.Json;
using System.Threading.Tasks;
using X.PagedList.Extensions;

namespace POS_System.Controllers
{
    public class PosController : Controller
    {

        private readonly ICustomerRepository _customer;
        private readonly IProductRepository _product;
        private readonly EntityConntext _context;
        private readonly IPosRepository _pos;
        private readonly ICurrencyRepository _currency;
        private readonly ILogger<PosController> _logger;
        public PosController(ICustomerRepository customer, IProductRepository product, EntityConntext context, IPosRepository pos, ICurrencyRepository currency, ILogger<PosController> logger)
        {

            _customer = customer;
            _context = context;
            _product = product;
            _pos = pos;
            _currency = currency;
            _logger = logger;
        }


        public async Task<IActionResult> Index(int? page)
        {
            int pageSize = 9;
            int pageNumber = page ?? 1;

           

            var saledetai = await _pos.GetAll();
            var pageList = saledetai.ToPagedList(pageNumber, pageSize);
            return View("Index", pageList);
        }

        [HttpGet]
        public async Task<IActionResult> Create(string category = "All")
        {
            await PopulateViewData();

            // Current exchange rate
            var exchangeRate = await _currency.GetExchangeRateAsync("KHR", "USD", DateTime.Now);
            ViewBag.ExchangeRate = exchangeRate;

            // Fetch future rate (if any)
            var futureRate = await _context.currencies
                .Where(c => c.FromCurrency == "KHR" && c.ToCurrency == "USD" && c.EffectiveDate > DateTime.Now)
                .OrderBy(c => c.EffectiveDate)
                .Select(c => new { c.ExchangeRate, c.EffectiveDate })
                .FirstOrDefaultAsync();

            ViewBag.FutureExchangeRate = futureRate?.ExchangeRate;
            ViewBag.FutureEffectiveDate = futureRate?.EffectiveDate;

            return View(new SaleDTO());
        }

        [HttpPost]
        public async Task<IActionResult> Store(SaleDTO saleDTO)
        {
            await PopulateViewData();

            try
            {
                if (saleDTO == null || string.IsNullOrEmpty(saleDTO.Currency))
                {

                    TempData["toastr-type"] = "error";
                    TempData["toastr-message"] = "Please select a currency...!";
                    return View("Create", saleDTO);
                }
                if (saleDTO.Currency != "USD" && saleDTO.Currency != "KHR")
                {

                    TempData["toastr-type"] = "wearning";
                    TempData["toastr-message"] = "Invalid currency. Use 'USD' or 'KHR'...!";
                    return View("Create", saleDTO);
                }

                var sale = new Sale
                {
                    CustomerID = saleDTO?.CustomerID,
                    Currency = saleDTO!.Currency,
                    SaleDate = DateTime.UtcNow,
                };

                // Fetch exchange rate at SaleDate
                var exchangeRate = await _currency.GetExchangeRateAsync("KHR", "USD", sale.SaleDate.ToLocalTime());
                ViewBag.ExchangeRate = exchangeRate;

                // Fetch future rate for display
                var futureRate = await _context.currencies
                    .Where(c => c.FromCurrency == "KHR" && c.ToCurrency == "USD" && c.EffectiveDate > DateTime.Now)
                    .OrderBy(c => c.EffectiveDate)
                    .Select(c => new { c.ExchangeRate, c.EffectiveDate })
                    .FirstOrDefaultAsync();

                ViewBag.FutureExchangeRate = futureRate?.ExchangeRate;
                ViewBag.FutureEffectiveDate = futureRate?.EffectiveDate;

                // Process SaleDetails from JSON
                var saleDetailsJson = Request.Form["SaleDetailsJson"];
                List<SaleDetail> saleDetails = new List<SaleDetail>();

                if (!string.IsNullOrEmpty(saleDetailsJson))
                {
                    var saleDetailDTOs = JsonSerializer.Deserialize<List<SaleDetailDTO>>(saleDetailsJson,
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                    saleDetails = saleDetailDTOs?.Select(detail => new SaleDetail
                    {
                        ProductID = detail?.ProductID,
                        Quantity = detail?.Quantity,
                        Price = detail?.Price ?? 0,
                        TotalAmount = detail?.TotalAmount ?? 0
                    }).ToList() ?? new List<SaleDetail>();
                }

                if (!saleDetails.Any())
                {
                    ModelState.AddModelError("", "No sale details provided.");

                    return View("Create", saleDTO);
                }

                foreach (var detail in saleDetails)
                {
                    if (saleDTO.Currency == "KHR" && (detail.Price < 100 || detail.TotalAmount < 100))
                    {

                        TempData["toastr-type"] = "error";
                        TempData["toastr-message"] = "KHR prices and totals should be in larger units (e.g., 100s or 1000s)...!";
                        return View("Create", saleDTO);
                    }
                    if (saleDTO.Currency == "USD" && (detail.Price > 1000 || detail.TotalAmount > 1000))
                    {

                        TempData["toastr-type"] = "error";
                        TempData["toastr-message"] = "USD prices and totals seem unusually high...!";
                        return View("Create", saleDTO);
                    }
                }

                var (success, errorMessage) = await _pos.CreateSaleAsync(sale, saleDetails);
                if (success)
                {
                    TempData["toastr-type"] = "success";
                    TempData["toastr-message"] = "Sale Has Been Create Success Fully...!";
                    return RedirectToAction("Index");
                }

                ModelState.AddModelError("", errorMessage ?? "Failed to create sale. Please try again.");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"An unexpected error occurred: {ex.Message}");
            }

            return View("Create", saleDTO);
        }
        private async Task PopulateViewData(string category = "All")
        {
            ViewBag.Customers = new SelectList(await _customer.GetAll(), "CustomerId", "CustomerName");
            ViewBag.Categories = await _context.categories.ToListAsync();

            var productsQuery = _context.products.Include(c => c.Category).AsQueryable();
            if (category != "All")
            {
                productsQuery = productsQuery.Where(p => p.Category!.CategoryName == category);
            }

            ViewBag.Products = await productsQuery.ToListAsync();
        }
        [HttpGet]
        public async Task<IActionResult> GetExchangeRate(string currency)
        {
            try
            {
                decimal exchangeRate;
                if (currency == "KHR")
                {
                    exchangeRate = await _currency.GetExchangeRateAsync("KHR", "USD", DateTime.Now);
                }
                else // USD
                {
                    exchangeRate = await _currency.GetExchangeRateAsync("USD", "KHR", DateTime.Now);
                }
                return Json(new { exchangeRate });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = $"Failed to fetch exchange rate: {ex.Message}" });
            }
        }

       
        [HttpGet]
        public async Task<IActionResult> pos(string category = "All")
        {
            await PopulateViewData();

            // Current exchange rate
            var exchangeRate = await _currency.GetExchangeRateAsync("KHR", "USD", DateTime.Now);
            ViewBag.ExchangeRate = exchangeRate;

            // Fetch future rate (if any)
            var futureRate = await _context.currencies
                .Where(c => c.FromCurrency == "KHR" && c.ToCurrency == "USD" && c.EffectiveDate > DateTime.Now)
                .OrderBy(c => c.EffectiveDate)
                .Select(c => new { c.ExchangeRate, c.EffectiveDate })
                .FirstOrDefaultAsync();

            ViewBag.FutureExchangeRate = futureRate?.ExchangeRate;
            ViewBag.FutureEffectiveDate = futureRate?.EffectiveDate;

            return View(new SaleDTO());
        }

        [HttpPost]
        public async Task<IActionResult> pos(SaleDTO saleDTO)
        {
            await PopulateViewData();

            try
            {
                if (saleDTO == null || string.IsNullOrEmpty(saleDTO.Currency))
                {

                    TempData["toastr-type"] = "error";
                    TempData["toastr-message"] = "Please select a currency...!";
                    return View("Create", saleDTO);
                }
                if (saleDTO.Currency != "USD" && saleDTO.Currency != "KHR")
                {

                    TempData["toastr-type"] = "wearning";
                    TempData["toastr-message"] = "Invalid currency. Use 'USD' or 'KHR'...!";
                    return View("pos", saleDTO);
                }

                var sale = new Sale
                {
                    CustomerID = saleDTO?.CustomerID,
                    Currency = saleDTO!.Currency,
                    SaleDate = DateTime.UtcNow,
                };

                // Fetch exchange rate at SaleDate
                var exchangeRate = await _currency.GetExchangeRateAsync("KHR", "USD", sale.SaleDate.ToLocalTime());
                ViewBag.ExchangeRate = exchangeRate;

                // Fetch future rate for display
                var futureRate = await _context.currencies
                    .Where(c => c.FromCurrency == "KHR" && c.ToCurrency == "USD" && c.EffectiveDate > DateTime.Now)
                    .OrderBy(c => c.EffectiveDate)
                    .Select(c => new { c.ExchangeRate, c.EffectiveDate })
                    .FirstOrDefaultAsync();

                ViewBag.FutureExchangeRate = futureRate?.ExchangeRate;
                ViewBag.FutureEffectiveDate = futureRate?.EffectiveDate;

                // Process SaleDetails from JSON
                var saleDetailsJson = Request.Form["SaleDetailsJson"];
                List<SaleDetail> saleDetails = new List<SaleDetail>();

                if (!string.IsNullOrEmpty(saleDetailsJson))
                {
                    var saleDetailDTOs = JsonSerializer.Deserialize<List<SaleDetailDTO>>(saleDetailsJson,
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                    saleDetails = saleDetailDTOs?.Select(detail => new SaleDetail
                    {
                        ProductID = detail?.ProductID,
                        Quantity = detail?.Quantity,
                        Price = detail?.Price ?? 0,
                        TotalAmount = detail?.TotalAmount ?? 0
                    }).ToList() ?? new List<SaleDetail>();
                }

                if (!saleDetails.Any())
                {
                    TempData["toastr-type"] = "error";
                    TempData["toastr-message"] = "No sale details provided.";

                    return View("pos", saleDTO);
                }

                foreach (var detail in saleDetails)
                {
                    if (saleDTO.Currency == "KHR" && (detail.Price < 100 || detail.TotalAmount < 100))
                    {

                        TempData["toastr-type"] = "error";
                        TempData["toastr-message"] = "KHR prices and totals should be in larger units (e.g., 100s or 1000s)...!";
                        return View("Create", saleDTO);
                    }
                    if (saleDTO.Currency == "USD" && (detail.Price > 1000 || detail.TotalAmount > 1000))
                    {

                        TempData["toastr-type"] = "error";
                        TempData["toastr-message"] = "USD prices and totals seem unusually high...!";
                        return View("pos", saleDTO);
                    }
                }

                var (success, errorMessage) = await _pos.CreateSaleAsync(sale, saleDetails);
                if (success)
                {
                    TempData["toastr-type"] = "success";
                    TempData["toastr-message"] = "Sale Has Been Create Success Fully...!";
                    return RedirectToAction("pos");
                }

                ModelState.AddModelError("", errorMessage ?? "Failed to create sale. Please try again.");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"An unexpected error occurred: {ex.Message}");
            }

            return View("pos", saleDTO);
        }


        [HttpGet]
        public IActionResult AddCustomerModal()
        {
            return View("AddCustomerModal");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddCustomer([FromBody] Customer customer) // <-- CHANGE IS HERE: Use [FromBody] Customer
        {
            try
            {
              
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Where(x => x.Value.Errors.Any())
                                           .ToDictionary(
                                               kvp => kvp.Key,
                                               kvp => kvp.Value.Errors.Select(e => e.ErrorMessage).FirstOrDefault()
                                           );
                    return Json(new { success = false, errors = errors });
                }

              
                await _customer.addnew(customer); 

                return Json(new
                {
                    success = true,
                    customerId = customer.CustomerId, 
                    customerName = customer.CustomerName
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding customer.");
                
                return StatusCode(500, new { success = false, message = $"An unexpected error occurred: {ex.Message}" });
            }
        }

       
    }



}














