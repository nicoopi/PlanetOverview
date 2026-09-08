using PlanetOverview.Models.ApiResponses;

namespace PlanetOverview.Services.Interfaces;

public interface INewsService
{
    Task<NewsApiResponse?> GetNewsAsync(string pais);
}