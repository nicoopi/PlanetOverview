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
    public async Task<WeatherOverview?> GetWeather(string capital)
    {   
        var chaveApi = _config["ApiConfigs:WeatherApiKey"];
        if (string.IsNullOrWhiteSpace(chaveApi) || string.IsNullOrWhiteSpace(capital)) return null;
        var response = await ApiClient.ReadAsync<WeatherApiResponse>(_httpClient,
            $"https://api.openweathermap.org/data/2.5/weather?q={Uri.EscapeDataString(capital.Trim())}&appid={Uri.EscapeDataString(chaveApi)}&units=metric&lang=pt_br"
        );

        if (response?.Main == null || response.Weather == null || response.Weather.Length == 0)
        {
            return null;
        }

        var weatheroverview = new WeatherOverview
        {
            Temp = response.Main.Temp,
            Description = response.Weather[0].Description
        };

        return weatheroverview;
    }
}