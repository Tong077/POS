using Dapper;
using Microsoft.EntityFrameworkCore;
using POS_System.Data;
using POS_System.Models;

namespace POS_System.Service
{
    public class CurrencyService : ICurrencyRepository
    {
        private readonly EntityConntext _context;
        private readonly DapperConnection _connect;
        public CurrencyService(EntityConntext context, DapperConnection connect)
        {
            _context = context;
            _connect = connect;
        }
        public async Task<bool> AddCurrencyAsync(Currency currency)
        {
            _context.currencies.Add(currency);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> DeleteCurrencyAsync(Currency currency)
        {
            _context.currencies.Remove(currency);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<IEnumerable<Currency>> GetAllCurrenciesAsync()
        {
            var sql = "SELECT * FROM currencies";
            var currencies = _connect.Connection.QueryAsync<Currency>(sql);
            return await currencies;
        }

        public async Task<Currency> GetById(int CurrencyId)
        {
            var currency = await _context.currencies.FirstOrDefaultAsync(c => c.CurrencyId == CurrencyId);
            return currency!;
        }

        public async Task<decimal> GetExchangeRateAsync(string fromCurrency, string toCurrency, DateTime date)
        {
            if (string.IsNullOrWhiteSpace(fromCurrency) || string.IsNullOrWhiteSpace(toCurrency))
                throw new ArgumentException("Currency codes cannot be null or empty.");
            if (date == DateTime.MinValue)
                throw new ArgumentException("Date cannot be DateTime.MinValue.");

            fromCurrency = fromCurrency.ToUpper();
            toCurrency = toCurrency.ToUpper();

            if (fromCurrency == toCurrency)
                return 1m;

            // Convert date to local time since EffectiveDate is stored as local
            var localDate = date.Kind == DateTimeKind.Utc ? date.ToLocalTime() : date;

            var rate = await _context.currencies
                .Where(c => c.FromCurrency == fromCurrency && c.ToCurrency == toCurrency && c.EffectiveDate <= localDate)
                .OrderByDescending(c => c.EffectiveDate)
                .Select(c => c.ExchangeRate)
                .FirstOrDefaultAsync();

            if (rate.HasValue && rate > 0)
                return rate.Value;

            var reverseRate = await _context.currencies
                .Where(c => c.FromCurrency == toCurrency && c.ToCurrency == fromCurrency && c.EffectiveDate <= localDate)
                .OrderByDescending(c => c.EffectiveDate)
                .Select(c => c.ExchangeRate)
                .FirstOrDefaultAsync();

            if (reverseRate.HasValue && reverseRate > 0)
                return 1m / reverseRate.Value;

            throw new InvalidOperationException(
                $"No exchange rate found for {fromCurrency} to {toCurrency} or its reverse (effective on or before {localDate:yyyy-MM-dd HH:mm}).");
        }
        public async Task<Currency> GetLatestExchangeRateAsync(string fromCurrency, string toCurrency)
        {
            return await _context.currencies
                .Where(c => c.FromCurrency == fromCurrency && c.ToCurrency == toCurrency)
                .OrderByDescending(c => c.EffectiveDate)
                .FirstOrDefaultAsync();
        }

        public async Task<bool> UpdateCurrencyAsync(Currency currency)
        {
            _context.currencies.Update(currency);
            return await _context.SaveChangesAsync() > 0;
        }
    }
}
