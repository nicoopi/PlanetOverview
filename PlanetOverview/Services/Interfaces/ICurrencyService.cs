using PlanetOverview.Models;

namespace PlanetOverview.Services.Interfaces;

public interface ICurrencyService
{
    Task<CurrencyOverview?> GetExchangeRateAsync(string targetCurrency);
}