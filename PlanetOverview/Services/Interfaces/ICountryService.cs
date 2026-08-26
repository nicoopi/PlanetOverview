using PlanetOverview.Models;

namespace PlanetOverview.Services.Interfaces;

public interface ICountryService
{
    Task<CountryOverview?> GetCountry(string country);
}