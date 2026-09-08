using System.Net.Http.Json;
using Microsoft.Extensions.Configuration;
using PlanetOverview.Models.ApiResponses;
using PlanetOverview.Services.Interfaces;

namespace PlanetOverview.Services;

public class NewsService : INewsService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _config;

    public NewsService(HttpClient httpClient, IConfiguration config)
    {
        _httpClient = httpClient;
        _config = config;
    }

    public async Task<NewsApiResponse?> GetNewsAsync(string pais)
    {
        string? chaveApi = _config["ApiConfigs:NewsApiKey"];
        string urlRequisicao = $"https://newsapi.org/v2/everything?q={pais}&apiKey={chaveApi}";

        if (!_httpClient.DefaultRequestHeaders.Contains("User-Agent"))
        {
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "PlanetOverview-App");
        }

        var response = await _httpClient.GetFromJsonAsync<NewsApiResponse>(urlRequisicao);

        return response;
    }
}