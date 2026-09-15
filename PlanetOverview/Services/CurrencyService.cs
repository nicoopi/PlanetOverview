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
        targetCurrency = targetCurrency.ToUpper();
        
        var chaveApi = _config["ApiConfigs:CurrencyApiKey"];
        var url = $"https://api.currencyfreaks.com/v2.0/rates/latest?apikey={chaveApi}&symbols={targetCurrency}";

        var response = await _httpClient.GetFromJsonAsync<CurrencyApiResponse>(url);

        if (response == null || !response.Rates.ContainsKey(targetCurrency))
        {
            return null;
        }

        if (decimal.TryParse(response.Rates[targetCurrency], System.Globalization.CultureInfo.InvariantCulture, out decimal rate))
        {
            return new CurrencyOverview
            {
                BaseCurrency = response.Base,
                TargetCurrency = targetCurrency,
                ExchangeRate = rate
            };
        }

        return null;
    }
}