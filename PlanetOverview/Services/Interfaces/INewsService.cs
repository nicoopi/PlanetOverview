using System.Threading.Tasks;
using PlanetOverview.Models.ApiResponses;

namespace PlanetOverview.Services.Interfaces;

public interface INewsService
{
    Task<(bool Sucesso, NewsApiResponse? Dados, string? MensagemErro)> GetNewsAsync(string pais);
}