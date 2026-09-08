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
        var response = await _httpClient.GetFromJsonAsync<WeatherApiResponse>(
            $"https://api.openweathermap.org/data/2.5/weather?q={capital}&appid={chaveApi}&units=metric&lang=pt_br"
        );

        if (response == null)
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