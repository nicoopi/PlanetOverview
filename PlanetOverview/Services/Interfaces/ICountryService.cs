using PlanetOverview.Models;

namespace PlanetOverview.Services.Interfaces;

public interface ICountryService
{
    IReadOnlyList<CountrySuggestion> GetCountries();
    Task<CountryOverview?> GetCountry(string country);
}
