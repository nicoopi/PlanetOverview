using System.Net.Http.Json;
using Microsoft.Extensions.Configuration;
using PlanetOverview.Models.ApiResponses;
using PlanetOverview.Services.Interfaces;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace PlanetOverview.Services;

public class NewsService : INewsService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _config;
    private readonly ICountryService _countries;

    public NewsService(HttpClient httpClient, IConfiguration config, ICountryService countries)
    {
        _httpClient = httpClient;
        _config = config;
        _countries = countries;
    }

    public async Task<NewsApiResponse?> GetNewsAsync(string pais)
    {
        string? chaveApi = _config["ApiConfigs:NewsApiKey"];
        if (string.IsNullOrWhiteSpace(chaveApi) || string.IsNullOrWhiteSpace(pais)) return null;
        var country = await _countries.GetCountry(pais);
        if (country == null) return null;
        var names = new[] { country.CountryName, country.Country }
            .OfType<string>().Where(name => !string.IsNullOrWhiteSpace(name))
            .Distinct(StringComparer.OrdinalIgnoreCase).ToArray();
        var query = string.Join(" OR ", names.Select(name => $"\"{name}\""));
        // A title match avoids articles that merely mention the country deep in the body.
        string urlRequisicao = $"https://newsapi.org/v2/everything?q={Uri.EscapeDataString(query)}&searchIn=title&language=pt&apiKey={Uri.EscapeDataString(chaveApi)}&pageSize=24&sortBy=publishedAt";

        if (!_httpClient.DefaultRequestHeaders.Contains("User-Agent"))
        {
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "PlanetOverview-App");
        }

        var response = await ApiClient.ReadAsync<NewsApiResponse>(_httpClient, urlRequisicao);

        if (response?.Status != "ok") return null;
        var patterns = names.Select(name => $@"(?<![\p{{L}}\p{{N}}]){Regex.Escape(Normalize(name))}(?![\p{{L}}\p{{N}}])").ToArray();
        response.Articles = (response.Articles ?? [])
            .Where(article => !string.IsNullOrWhiteSpace(article.Title) &&
                patterns.Any(pattern => Regex.IsMatch(Normalize(article.Title), pattern, RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)) &&
                Uri.TryCreate(article.Url, UriKind.Absolute, out var url) && (url.Scheme == "https" || url.Scheme == "http"))
            .DistinctBy(article => article.Url)
            .Take(6).ToList();
        response.TotalResults = response.Articles.Count;
        response.CountryName = country.CountryName;
        response.CountryCode = country.CountryCode;
        return response;
    }

    private static string Normalize(string value) => string.Concat(value.Normalize(NormalizationForm.FormD)
        .Where(character => CharUnicodeInfo.GetUnicodeCategory(character) != UnicodeCategory.NonSpacingMark));
}
