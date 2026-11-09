using System.Threading.Tasks;
using PlanetOverview.Models;

namespace PlanetOverview.Services.Interfaces;

public interface ITimeService
{
    Task<(bool Sucesso, TimeOverview? Dados, string? MensagemErro)> GetByCoordinatesAsync(double latitude, double longitude);
    
    Task<(bool Sucesso, TimeOverview? Dados, string? MensagemErro)> GetCurrentTimeAsync(string timeZone);
}