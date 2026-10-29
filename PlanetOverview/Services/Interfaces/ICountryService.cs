using PlanetOverview.Models;

namespace PlanetOverview.Services.Interfaces;

public interface ICountryService
{
    Task<IReadOnlyList<CountrySuggestion>?> GetCountries();
    Task<CountryOverview?> GetCountry(string country);
}
