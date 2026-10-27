using PlanetOverview.Models;
using PlanetOverview.Models.ApiResponses;
using PlanetOverview.Services.Interfaces;
using Microsoft.Extensions.Caching.Memory;
using System.Net.Http.Headers;
using System.Text.Json;
namespace PlanetOverview.Services;
public class CountryService : ICountryService
{
    private readonly IHttpClientFactory _clients;
    private readonly IConfiguration _config;
    private readonly IMemoryCache _cache;

    public CountryService(IHttpClientFactory clients, IConfiguration config, IMemoryCache cache)
    {
        _clients = clients;
        _config = config;
        _cache = cache;
    }
    private static readonly CountryApiResponse[] Countries = LoadCountries();
    private static readonly CountrySuggestion[] Suggestions = Countries
        .Where(country => country.Name?.Common != null && country.Cca2 != null)
        .Select(country => new CountrySuggestion(
            country.Translations?.GetValueOrDefault("por")?.Common ?? country.Name!.Common!,
            country.Name!.Common!, country.Cca2!))
        .OrderBy(country => country.Name, StringComparer.Create(new System.Globalization.CultureInfo("pt-BR"), false))
        .ToArray();

    public IReadOnlyList<CountrySuggestion> GetCountries() => Suggestions;

    private static CountryApiResponse[] LoadCountries()
    {
        using var stream = typeof(CountryService).Assembly.GetManifestResourceStream("PlanetOverview.Data.countries.json")
            ?? throw new InvalidOperationException("Base de países não encontrada.");
        return System.Text.Json.JsonSerializer.Deserialize<CountryApiResponse[]>(stream,
            new System.Text.Json.JsonSerializerOptions(System.Text.Json.JsonSerializerDefaults.Web)) ?? [];
    }

    private static IEnumerable<string> Names(CountryApiResponse country) =>
        new[] { country.Name?.Common, country.Name?.Official, country.Cca2 }
            .Concat(country.Translations?.Values.SelectMany(t => new[] { t.Common, t.Official }) ?? [])
            .Concat(country.AltSpellings ?? [])
            .OfType<string>();

    private static string Normalize(string value) => string.Concat(value.Normalize(System.Text.NormalizationForm.FormD)
        .Where(c => System.Globalization.CharUnicodeInfo.GetUnicodeCategory(c) != System.Globalization.UnicodeCategory.NonSpacingMark))
        .ToUpperInvariant();

    public async Task<CountryOverview?> GetCountry(string country)
    {
        var reference = FindCountry(country);
        if (reference == null) return null;
        var key = _config["ApiConfigs:CountriesApiKey"];
        if (string.IsNullOrWhiteSpace(key)) return reference;
        var cacheKey = $"country-v5:{reference.CountryCode}";
        if (_cache.TryGetValue<CountryOverview>(cacheKey, out var cached)) return cached;

        try
        {
            using var client = _clients.CreateClient("Countries");
            using var request = new HttpRequestMessage(HttpMethod.Get,
                $"countries/v5/codes.alpha_2/{reference.CountryCode}?response_fields=names,codes,capitals,currencies,population,flag,languages,timezones");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", key);
            using var response = await client.SendAsync(request);
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<CountryV5Response>();
                var live = result?.Data?.Objects?.FirstOrDefault(item =>
                    string.Equals(item.Codes?.Alpha2, reference.CountryCode, StringComparison.OrdinalIgnoreCase));
                if (live?.Names?.Common != null && live.Population.HasValue)
                {
                    var capital = live.Capitals?.FirstOrDefault(item => item.Attributes?.Primary == true)
                        ?? live.Capitals?.FirstOrDefault();
                    var overview = new CountryOverview
                    {
                        Country = live.Names.Common,
                        CountryName = live.Names.Translations?.GetValueOrDefault("por")?.Common ?? reference.CountryName,
                        CountryCode = reference.CountryCode,
                        Capital = capital?.Name,
                        CapitalCoordinates = capital?.Coordinates is { Lat: { } lat, Lng: { } lng }
                            ? [lat, lng] : null,
                        CurrencyCode = live.Currencies?.FirstOrDefault()?.Code,
                        Population = live.Population.Value,
                        FlagUrl = live.Flag?.Svg ?? live.Flag?.Png,
                        Language = live.Languages == null ? null : string.Join(", ", live.Languages
                            .Select(item => item.Name).OfType<string>()),
                        TimeZones = live.Timezones?.ToList() ?? [],
                        DataSource = "REST Countries API v5"
                    };
                    _cache.Set(cacheKey, overview, TimeSpan.FromHours(4));
                    return overview;
                }
            }
        }
        catch (Exception error) when (error is HttpRequestException or OperationCanceledException or JsonException or NotSupportedException)
        {
            // Keep reference data usable during provider outages, with an explicit source label.
        }
        reference.DataSource = "Base local REST Countries (API v5 indisponível)";
        _cache.Set(cacheKey, reference, TimeSpan.FromMinutes(1));
        return reference;
    }

    private static CountryOverview? FindCountry(string country)
    {
        if (string.IsNullOrWhiteSpace(country)) return null;
        var query = Normalize(country.Trim());
        var selected = Countries.FirstOrDefault(c => Names(c).Any(name => Normalize(name) == query))
            ?? Countries.FirstOrDefault(c => Names(c).Any(name => Normalize(name).Contains(query)));
        if (selected?.Name?.Common == null || string.IsNullOrWhiteSpace(selected.Cca2)) return null;
        var portuguese = selected.Translations?.GetValueOrDefault("por");
        return new CountryOverview
        {
            CapitalCoordinates = selected.CapitalInfo?.Latlng,
            Country = selected.Name.Common, CountryName = portuguese?.Common ?? selected.Name.Common,
            Capital = selected.Capital?.FirstOrDefault(), CountryCode = selected.Cca2,
            CurrencyCode = selected.Currencies?.Keys.FirstOrDefault(), Population = selected.Population,
            FlagUrl = selected.Flags?.Svg ?? selected.Flags?.Png,
            Language = selected.Languages == null ? null : string.Join(", ", selected.Languages.Values),
            TimeZones = selected.Timezones?.ToList() ?? []
        };
    }
}
