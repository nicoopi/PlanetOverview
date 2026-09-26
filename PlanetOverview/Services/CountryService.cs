using System.Net;
using System.Net.Http.Json;
using PlanetOverview.Models;
using PlanetOverview.Models.ApiResponses;
using PlanetOverview.Services.Interfaces;

namespace PlanetOverview.Services;

public class CountryService : ICountryService
{
    private readonly HttpClient _httpClient;

    public CountryService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<CountryOverview?> GetCountry(string country)
    {
        if (string.IsNullOrWhiteSpace(country))
        {
            return null;
        }

        var encodedCountry = Uri.EscapeDataString(country.Trim());

        var response = await _httpClient.GetAsync(
            $"https://restcountries.com/v3.1/name/{encodedCountry}"
        );

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }

        if (!response.IsSuccessStatusCode)
        {
            return null;
        }

        var countries = await response.Content
            .ReadFromJsonAsync<CountryApiResponse[]>();

        if (countries == null || countries.Length == 0)
        {
            return null;
        }

        var apiCountry = countries.FirstOrDefault(
            c => c.Name?.Common != null &&
                 c.Name.Common.Equals(
                     country.Trim(),
                     StringComparison.OrdinalIgnoreCase
                 )
        ) ?? countries[0];

        if (apiCountry.Name?.Common == null ||
            string.IsNullOrWhiteSpace(apiCountry.Cca2))
        {
            return null;
        }

        var language = apiCountry.Languages != null
            ? string.Join(", ", apiCountry.Languages.Values)
            : null;

        var capital = apiCountry.Capital?.FirstOrDefault();

        return new CountryOverview
        {
            Country = apiCountry.Name.Common,
            CountryName = apiCountry.Name.Official,
            Capital = capital,
            CountryCode = apiCountry.Cca2,
            Population = apiCountry.Population,
            FlagUrl = apiCountry.Flags?.Svg ?? apiCountry.Flags?.Png,
            Language = language,
            TimeZones = apiCountry.Timezones?.ToList() ?? []
        };
    }
}