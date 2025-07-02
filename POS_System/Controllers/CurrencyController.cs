using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using POS_System.Data;
using POS_System.Models;
using POS_System.Models.DTO;
using POS_System.Service;

namespace POS_System.Controllers
{
  
    public class CurrencyController : Controller
    {
        private readonly ICurrencyRepository _currency;
        private readonly EntityConntext _context;
        private readonly ILogger<CurrencyController> _logger;
      
        public CurrencyController(ICurrencyRepository currency, EntityConntext context, ILogger<CurrencyController> logger)
        {
            _currency = currency;
            _context = context;
            _logger = logger;
        }
       
        public async Task<IActionResult> Index()
        {
            var currencies = await _currency.GetAllCurrenciesAsync();
            return View("Index", currencies);
        }
       
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var model = new CurrencyDTO
            {
                EffectiveDate = DateTime.Now 
            };
            return View("Create", model);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Store(CurrencyDTO currencyDTO)
        {
            if (!ModelState.IsValid)
            {
                return View("Create", currencyDTO);
            }

            try
            {
              
                currencyDTO.FromCurrency = currencyDTO.FromCurrency.ToUpper();
                currencyDTO.ToCurrency = currencyDTO.ToCurrency.ToUpper();

                
                if (currencyDTO.FromCurrency == currencyDTO.ToCurrency)
                {
                    ModelState.AddModelError("", "From Currency and To Currency cannot be the same.");
                    return View("Create", currencyDTO);
                }

                
                if (currencyDTO.ExchangeRate <= 0)
                {
                    ModelState.AddModelError("ExchangeRate", "Exchange rate must be greater than 0.");
                    return View("Create", currencyDTO);
                }

              
                var effectiveDate = currencyDTO.EffectiveDate; 
                var now = DateTime.Now; 

               
                if (effectiveDate < now.AddYears(-1)) 
                {
                    ModelState.AddModelError("EffectiveDateString", "Effective date cannot be more than 1 year in the past.");
                    return View("Create", currencyDTO);
                }

                
                string warningMessage = null;
                var tolerance = TimeSpan.FromMinutes(1);
                if (effectiveDate > now.Add(tolerance))
                {
                    warningMessage = $"The exchange rate for {currencyDTO.FromCurrency} to {currencyDTO.ToCurrency} will take effect on {effectiveDate:yyyy-MM-dd HH:mm:ss} (local time). It will not be applied until then.";
                }

              
                using var transaction = await _context.Database.BeginTransactionAsync();

               
                var previousCurrencies = await _context.currencies
                    .Where(c => (c.FromCurrency == currencyDTO.FromCurrency && c.ToCurrency == currencyDTO.ToCurrency) ||
                                (c.FromCurrency == currencyDTO.ToCurrency && c.ToCurrency == currencyDTO.FromCurrency))
                    .Where(c => c.EffectiveDate < effectiveDate && (c.EndDate == null || c.EndDate > effectiveDate))
                    .OrderByDescending(c => c.EffectiveDate)
                    .ToListAsync();

                var previousDirect = previousCurrencies
                    .FirstOrDefault(c => c.FromCurrency == currencyDTO.FromCurrency && c.ToCurrency == currencyDTO.ToCurrency);
                var previousInverse = previousCurrencies
                    .FirstOrDefault(c => c.FromCurrency == currencyDTO.ToCurrency && c.ToCurrency == currencyDTO.FromCurrency);

                var nowUtc = DateTime.UtcNow;
                if (previousDirect != null)
                {
                    previousDirect.EndDate = effectiveDate;
                    previousDirect.UpdatedAt = nowUtc;
                }
                if (previousInverse != null)
                {
                    previousInverse.EndDate = effectiveDate;
                    previousInverse.UpdatedAt = nowUtc;
                }

              
                var currencies = await _context.currencies
                    .Where(c => (c.FromCurrency == currencyDTO.FromCurrency && c.ToCurrency == currencyDTO.ToCurrency) ||
                                (c.FromCurrency == currencyDTO.ToCurrency && c.ToCurrency == currencyDTO.FromCurrency))
                    .Where(c => c.EffectiveDate == effectiveDate)
                    .ToListAsync();

                var existingCurrency = currencies
                    .FirstOrDefault(c => c.FromCurrency == currencyDTO.FromCurrency && c.ToCurrency == currencyDTO.ToCurrency);
                var inverseCurrency = currencies
                    .FirstOrDefault(c => c.FromCurrency == currencyDTO.ToCurrency && c.ToCurrency == currencyDTO.FromCurrency);

                decimal baseExchangeRate = currencyDTO.ExchangeRate;
                decimal inverseExchangeRate = 1m;

                if (existingCurrency != null)
                {
                    existingCurrency.ExchangeRate = baseExchangeRate;
                    existingCurrency.IsBaseRate = true;
                    existingCurrency.EndDate = null;
                    existingCurrency.UpdatedAt = nowUtc;
                    if (inverseCurrency != null)
                    {
                        inverseCurrency.ExchangeRate = inverseExchangeRate;
                        inverseCurrency.IsBaseRate = false;
                        inverseCurrency.EndDate = null;
                        inverseCurrency.UpdatedAt = nowUtc;
                    }
                    else
                    {
                        var newInverseCurrency = new Currency
                        {
                            FromCurrency = currencyDTO.ToCurrency,
                            ToCurrency = currencyDTO.FromCurrency,
                            ExchangeRate = inverseExchangeRate,
                            IsBaseRate = false,
                            EffectiveDate = effectiveDate,
                            EndDate = null,
                            CreatedAt = nowUtc,
                            UpdatedAt = nowUtc
                        };
                        await _currency.AddCurrencyAsync(newInverseCurrency);
                    }
                }
                else
                {
                    var currency = new Currency
                    {
                        FromCurrency = currencyDTO.FromCurrency,
                        ToCurrency = currencyDTO.ToCurrency,
                        ExchangeRate = baseExchangeRate,
                        IsBaseRate = true,
                        EffectiveDate = effectiveDate,
                        EndDate = null,
                        CreatedAt = nowUtc,
                        UpdatedAt = nowUtc
                    };
                    await _currency.AddCurrencyAsync(currency);

                    if (inverseCurrency == null)
                    {
                        var newInverseCurrency = new Currency
                        {
                            FromCurrency = currencyDTO.ToCurrency,
                            ToCurrency = currencyDTO.FromCurrency,
                            ExchangeRate = inverseExchangeRate,
                            IsBaseRate = false,
                            EffectiveDate = effectiveDate,
                            EndDate = null,
                            CreatedAt = nowUtc,
                            UpdatedAt = nowUtc
                        };
                        await _currency.AddCurrencyAsync(newInverseCurrency);
                    }
                    else
                    {
                        inverseCurrency.ExchangeRate = inverseExchangeRate;
                        inverseCurrency.IsBaseRate = false;
                        inverseCurrency.EndDate = null;
                        inverseCurrency.UpdatedAt = nowUtc;
                    }
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                string successMessage = $"Currency exchange rate for {currencyDTO.FromCurrency} to {currencyDTO.ToCurrency} created/updated successfully! Effective: {effectiveDate:yyyy-MM-dd HH:mm:ss} (local time).";
                if (warningMessage != null)
                {
                    successMessage += " " + warningMessage;
                }
                TempData["SuccessMessage"] = successMessage;

                TempData["toastr-type"] = "success";
                TempData["toastr-message"] = "The Record Has Been Create successful...!";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create/update currency exchange rate for {FromCurrency} to {ToCurrency} effective on {EffectiveDate}",
                    currencyDTO.FromCurrency, currencyDTO.ToCurrency, currencyDTO.EffectiveDate);
                ModelState.AddModelError("", $"An error occurred: {ex.Message}");
                return View("Create", currencyDTO);
            }
        }
       
        [HttpGet]
        public async Task<IActionResult> Edit(int CurrencyId)
        {
            var currency = await _currency.GetById(CurrencyId);
            if (currency == null)
            {
                return NotFound();
            }
            return View("Edit", currency);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(int CurrencyId, string fromCurrency, string toCurrency, decimal exchangeRate, DateTime effectiveDate)
        {
            // Validation
            if (string.IsNullOrWhiteSpace(fromCurrency) || string.IsNullOrWhiteSpace(toCurrency))
            {
                ModelState.AddModelError("", "From Currency and To Currency are required.");
            }
            if (fromCurrency == toCurrency)
            {
                ModelState.AddModelError("", "From Currency and To Currency must be different.");
            }
            if (exchangeRate <= 0)
            {
                ModelState.AddModelError("ExchangeRate", "Exchange rate must be greater than zero.");
            }
            if (effectiveDate < DateTime.UtcNow.Date)
            {
                ModelState.AddModelError("EffectiveDate", "Effective date cannot be in the past.");
            }

            if (!ModelState.IsValid)
            {
                var currencyForView = new Currency
                {
                    CurrencyId = CurrencyId,
                    FromCurrency = fromCurrency,
                    ToCurrency = toCurrency,
                    ExchangeRate = exchangeRate,
                    EffectiveDate = effectiveDate
                };
                return View("Edit", currencyForView);
            }

            try
            {
                // Fetch the existing currency (already tracked by EF Core)
                var existingCurrency = await _currency.GetById(CurrencyId);
                if (existingCurrency == null)
                {
                    return NotFound();
                }

              
                existingCurrency.FromCurrency = fromCurrency;
                existingCurrency.ToCurrency = toCurrency;
                existingCurrency.ExchangeRate = exchangeRate;
                existingCurrency.EffectiveDate = effectiveDate;

               
                await _currency.UpdateCurrencyAsync(existingCurrency);

                // Check if the inverse currency pair already exists
                var inverseExists = await _context.currencies
                    .AnyAsync(c => c.FromCurrency == toCurrency && c.ToCurrency == fromCurrency && c.EffectiveDate == effectiveDate);

                if (!inverseExists)
                {
                    // Create a new record for the inverse currency pair
                    var inverseCurrency = new Currency
                    {
                        FromCurrency = toCurrency, 
                        ToCurrency = fromCurrency,
                        ExchangeRate = 1M / exchangeRate, 
                        EffectiveDate = effectiveDate
                    };

                    await _currency.AddCurrencyAsync(inverseCurrency);
                }

                TempData["toastr-type"] = "success";
                TempData["toastr-message"] = "The Record Has Been Update successfully...!";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["toastr-type"] = "waerning";
                TempData["toastr-message"] = "The Record Has Been Update successful...!";
                var currencyForView = new Currency
                {
                    CurrencyId = CurrencyId,
                    FromCurrency = fromCurrency,
                    ToCurrency = toCurrency,
                    ExchangeRate = exchangeRate,
                    EffectiveDate = effectiveDate
                };
                return View("Edit", currencyForView);
            }
        }
    }
    
}
