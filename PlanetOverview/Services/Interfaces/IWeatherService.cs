using PlanetOverview.Models;

namespace PlanetOverview.Services.Interfaces;

public interface IWeatherService
{
    Task<(bool Sucesso, WeatherOverview? Dados, string? MensagemErro)> GetWeather(string capital);
}