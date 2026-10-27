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
        if (string.IsNullOrWhiteSpace(chaveApi) || string.IsNullOrWhiteSpace(pais)) return null;
        string urlRequisicao = $"https://newsapi.org/v2/everything?q={Uri.EscapeDataString(pais.Trim())}&apiKey={Uri.EscapeDataString(chaveApi)}&pageSize=6&sortBy=publishedAt";

        if (!_httpClient.DefaultRequestHeaders.Contains("User-Agent"))
        {
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "PlanetOverview-App");
        }

        var response = await ApiClient.ReadAsync<NewsApiResponse>(_httpClient, urlRequisicao);

        return response;
    }
}