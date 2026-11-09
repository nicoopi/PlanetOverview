using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
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

    public async Task<(bool Sucesso, CurrencyOverview? Dados, string? MensagemErro)> GetExchangeRateAsync(string targetCurrency)
    {
        if (string.IsNullOrWhiteSpace(targetCurrency)) 
            return (false, null, "A moeda de destino não foi informada.");
            
        targetCurrency = targetCurrency.Trim().ToUpperInvariant();
        
        if (targetCurrency.Length != 3 || !targetCurrency.All(char.IsAsciiLetter)) 
            return (false, null, "Código de moeda inválido.");

        var chaveApi = _config["ApiConfigs:CurrencyApiKey"];
        
        if (string.IsNullOrWhiteSpace(chaveApi)) 
            return (false, null, "Chave da API de moedas não configurada.");
            
        var url = $"https://api.currencyfreaks.com/v2.0/rates/latest?apikey={Uri.EscapeDataString(chaveApi)}&symbols=BRL,{targetCurrency}";

        try
        {
            var response = await ApiClient.ReadAsync<CurrencyApiResponse>(_httpClient, url);

            if (response?.Rates == null || !response.Rates.ContainsKey(targetCurrency) || !response.Rates.ContainsKey("BRL"))
            {
                return (false, null, $"Não foi possível encontrar a cotação para a moeda {targetCurrency}.");
            }

            if (decimal.TryParse(response.Rates[targetCurrency], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out decimal rate) && rate > 0 &&
                decimal.TryParse(response.Rates["BRL"], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out decimal brlRate) && brlRate > 0)
            {
                var currencyOverview = new CurrencyOverview
                {
                    BaseCurrency = "BRL",
                    TargetCurrency = targetCurrency,
                    // Ambas as taxas compartilham a base do provedor, então a razão dá BRL -> target.
                    ExchangeRate = rate / brlRate
                };

                return (true, currencyOverview, null);
            }

            return (false, null, "Falha ao processar os valores de cotação retornados pela API.");
        }
        catch (HttpRequestException)
        {
            return (false, null, "Serviço de cotação temporariamente indisponível. Verifique a conexão.");
        }
        catch (Exception)
        {
            return (false, null, "Ocorreu um erro interno ao processar a cotação da moeda.");
        }
    }
}