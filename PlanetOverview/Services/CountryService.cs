using System.Net.Http.Json;
using PlanetOverview.Models;
using PlanetOverview.Models.ApiResponses;
using PlanetOverview.Services.Interfaces;

namespace PlanetOverview.Services;

public class CountryService : ICountryService
{
    private readonly HttpClient _httpClient;
    private readonly IWeatherService _weatherService;

    public CountryService(HttpClient httpClient, IWeatherService weatherService)
    {
        _httpClient = httpClient;
        _weatherService = weatherService;
    }
    public async Task<CountryOverview?> GetCountry(string country)
    {
        var response = await _httpClient.GetFromJsonAsync<CountryApiResponse[]>(
            $"https://restcountries.com/v3.1/name/{country}"
        );

        if (response == null || response.Length == 0)
        {
            return null;
        }

        var apiCountry = response[0];

        if (apiCountry.Capital == null || apiCountry.Capital.Length == 0)
        {
            return null;
        }

        var dadosClima = await _weatherService.GetWeather(apiCountry.Capital[0]);

        var countryOverview = new CountryOverview
        {
            Country = apiCountry.Name.Common,
            Capital = apiCountry.Capital[0],
            CountryCode = apiCountry.Cca2,
            Population = apiCountry.Population,
            Weather = dadosClima,
        };

        return countryOverview;
    }
}