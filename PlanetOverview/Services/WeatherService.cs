using System.Net.Http.Json;
using PlanetOverview.Models;
using PlanetOverview.Models.ApiResponses;
using PlanetOverview.Services.Interfaces;

namespace PlanetOverview.Services;

public class WeatherService : IWeatherService
{
    private readonly HttpClient _httpClient;

    public WeatherService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }
    public async Task<WeatherOverview?> GetWeather(string capital)
    {   
        var response = await _httpClient.GetFromJsonAsync<WeatherApiResponse>(
            $"https://api.openweathermap.org/data/2.5/weather?q={capital}&appid=COLOCAR_CHAVE_AQUI&units=metric&lang=pt_br"
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