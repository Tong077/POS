using POS_System.Models;

namespace POS_System.Service
{
    public interface ICurrencyRepository
    {

       
        Task<bool> AddCurrencyAsync(Currency currency);

        Task<decimal> GetExchangeRateAsync(string fromCurrency, string toCurrency, DateTime date);

       
        Task<IEnumerable<Currency>> GetAllCurrenciesAsync();
        Task<Currency> GetById(int CurrencyId);

       
        Task<bool> UpdateCurrencyAsync(Currency currency);

        Task<Currency> GetLatestExchangeRateAsync(string fromCurrency, string toCurrency);
        Task<bool> DeleteCurrencyAsync(Currency currency);
    }
}
