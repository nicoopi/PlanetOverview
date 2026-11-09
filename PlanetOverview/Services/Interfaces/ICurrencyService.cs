using System.Threading.Tasks;
using PlanetOverview.Models;

namespace PlanetOverview.Services.Interfaces;

public interface ICurrencyService
{
    Task<(bool Sucesso, CurrencyOverview? Dados, string? MensagemErro)> GetExchangeRateAsync(string targetCurrency);
}