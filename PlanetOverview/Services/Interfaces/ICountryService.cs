using System.Collections.Generic;
using System.Threading.Tasks;
using PlanetOverview.Models;

namespace PlanetOverview.Services.Interfaces;

public interface ICountryService
{
    Task<(bool Sucesso, IReadOnlyList<CountrySuggestion>? Dados, string? MensagemErro)> GetCountries();
    
    Task<(bool Sucesso, CountryOverview? Dados, string? MensagemErro)> GetCountry(string country);
}