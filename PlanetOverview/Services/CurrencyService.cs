using System.Net.Http.Json;
using PlanetOverview.Models;
using PlanetOverview.Models.ApiResponses;
using PlanetOverview.Services.Interfaces;

namespace PlanetOverview.Services;

public class CurrencyService : ICurrencyService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _config;

    public CurrencyService(HttpClient httpClient, IConfiguration config)
    {
        _httpClient = httpClient;
        _config = config;
    }

    public async Task<CurrencyOverview?> GetExchangeRateAsync(string targetCurrency)
    {
        if (string.IsNullOrWhiteSpace(targetCurrency)) return null;
        targetCurrency = targetCurrency.Trim().ToUpperInvariant();
        if (targetCurrency.Length != 3 || !targetCurrency.All(char.IsAsciiLetter)) return null;

        var chaveApi = _config["ApiConfigs:CurrencyApiKey"];
        if (string.IsNullOrWhiteSpace(chaveApi)) return null;
        var url = $"https://api.currencyfreaks.com/v2.0/rates/latest?apikey={Uri.EscapeDataString(chaveApi)}&symbols=BRL,{targetCurrency}";

        var response = await ApiClient.ReadAsync<CurrencyApiResponse>(_httpClient, url);

        if (response?.Rates == null || !response.Rates.ContainsKey(targetCurrency) || !response.Rates.ContainsKey("BRL"))
        {
            return null;
        }

        if (decimal.TryParse(response.Rates[targetCurrency], System.Globalization.NumberStyles.Float,
                System.Globalization.CultureInfo.InvariantCulture, out decimal rate) && rate > 0 &&
            decimal.TryParse(response.Rates["BRL"], System.Globalization.NumberStyles.Float,
                System.Globalization.CultureInfo.InvariantCulture, out decimal brlRate) && brlRate > 0)
        {
            return new CurrencyOverview
            {
                BaseCurrency = "BRL",
                TargetCurrency = targetCurrency,
                // Both rates share the provider's base, so their ratio gives BRL -> target.
                ExchangeRate = rate / brlRate
            };
        }

        return null;
    }
}
