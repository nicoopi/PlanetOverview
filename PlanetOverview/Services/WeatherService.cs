using System.Net.Http.Json;
using PlanetOverview.Models;
using PlanetOverview.Models.ApiResponses;
using PlanetOverview.Services.Interfaces;

namespace PlanetOverview.Services;

public class WeatherService : IWeatherService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _config;

    public WeatherService(HttpClient httpClient, IConfiguration config)
        {
            _httpClient = httpClient;
            _config = config;
        }
        public async Task<(bool Sucesso, WeatherOverview? Dados, string? MensagemErro)> GetWeather(string capital)
    {   
        var chaveApi = _config["ApiConfigs:WeatherApiKey"];
        
        if (string.IsNullOrWhiteSpace(chaveApi)) 
            return (false, null, "Chave da API não configurada.");
            
        if (string.IsNullOrWhiteSpace(capital)) 
            return (false, null, "Capital não informada.");

        try
        {
            var response = await ApiClient.ReadAsync<WeatherApiResponse>(_httpClient,
                $"https://api.openweathermap.org/data/2.5/weather?q={Uri.EscapeDataString(capital.Trim())}&appid={Uri.EscapeDataString(chaveApi)}&units=metric&lang=pt_br"
            );

            if (response?.Main == null || response.Weather == null || response.Weather.Length == 0)
            {
                // A internet funcionou, mas a cidade não foi encontrada na API
                return (false, null, $"Não foi possível encontrar o clima para {capital}.");
            }

            var weatheroverview = new WeatherOverview
            {
                Temp = response.Main.Temp,
                Description = response.Weather[0].Description
            };

            // Sucesso absoluto
            return (true, weatheroverview, null);
        }
        catch (HttpRequestException)
        {
            // Erro de rede capturado!
            return (false, null, "Serviço de clima temporariamente indisponível. Verifique a conexão.");
        }
        catch (Exception)
        {
            // Qualquer outro erro bizarro
            return (false, null, "Ocorreu um erro interno ao buscar as informações do clima.");
        }
}
}