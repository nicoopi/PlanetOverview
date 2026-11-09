using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
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

    public async Task<(bool Sucesso, NewsApiResponse? Dados, string? MensagemErro)> GetNewsAsync(string pais)
    {
        string? chaveApi = _config["ApiConfigs:NewsApiKey"];
        
        if (string.IsNullOrWhiteSpace(chaveApi)) 
            return (false, null, "Chave da API de notícias não configurada.");
            
        if (string.IsNullOrWhiteSpace(pais)) 
            return (false, null, "O nome do país não foi informado.");

        // Como atualizamos o CountryService, agora ele devolve uma Tupla. Vamos ler o Sucesso e os Dados dela!
        var resultadoPais = await _countries.GetCountry(pais);
        
        if (!resultadoPais.Sucesso || resultadoPais.Dados == null) 
            return (false, null, "Não foi possível carregar as informações do país base para buscar as notícias.");

        var country = resultadoPais.Dados;

        var names = new[] { country.CountryName, country.Country }
            .OfType<string>().Where(name => !string.IsNullOrWhiteSpace(name))
            .Distinct(StringComparer.OrdinalIgnoreCase).ToArray();
            
        var query = string.Join(" OR ", names.Select(name => $"\"{name}\""));
        
        string urlRequisicao = $"https://newsapi.org/v2/everything?q={Uri.EscapeDataString(query)}&searchIn=title&language=pt&apiKey={Uri.EscapeDataString(chaveApi)}&pageSize=24&sortBy=publishedAt";

        if (!_httpClient.DefaultRequestHeaders.Contains("User-Agent"))
        {
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "PlanetOverview-App");
        }

        try
        {
            var response = await ApiClient.ReadAsync<NewsApiResponse>(_httpClient, urlRequisicao);

            if (response?.Status != "ok") 
                return (false, null, "A API de notícias rejeitou a requisição ou não retornou status 'ok'.");

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
            
            return (true, response, null);
        }
        catch (HttpRequestException)
        {
            return (false, null, "Serviço de notícias temporariamente indisponível. Verifique a conexão.");
        }
        catch (Exception)
        {
            return (false, null, "Ocorreu um erro interno ao processar as notícias.");
        }
    }

    private static string Normalize(string value) => string.Concat(value.Normalize(NormalizationForm.FormD)
        .Where(character => CharUnicodeInfo.GetUnicodeCategory(character) != UnicodeCategory.NonSpacingMark));
}