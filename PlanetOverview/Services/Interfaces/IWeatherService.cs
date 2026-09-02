using PlanetOverview.Models;

namespace PlanetOverview.Services.Interfaces;

public interface IWeatherService
{
    Task<WeatherOverview?> GetWeather(string capital);
}