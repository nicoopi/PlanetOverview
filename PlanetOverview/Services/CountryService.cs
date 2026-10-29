using System.Globalization;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Caching.Memory;
using PlanetOverview.Models;
using PlanetOverview.Models.ApiResponses;
using PlanetOverview.Services.Interfaces;

namespace PlanetOverview.Services;

public sealed class CountryService : ICountryService
{
    private const string ApiBase = "https://api.restcountries.com/countries/v5";
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly IMemoryCache _cache;
    // Moedas sem cotação em provedores de câmbio e exceções de moeda principal.
    private static readonly HashSet<string> UnquotedCurrencies = new(StringComparer.OrdinalIgnoreCase)
        { "CUC", "GGP", "JEP", "IMP", "FOK", "KID", "TVD", "CKD" };
    private static readonly Dictionary<string, string> PreferredCurrency = new(StringComparer.OrdinalIgnoreCase)
        { ["PS"] = "ILS", ["CU"] = "CUP" };

    public CountryService(HttpClient httpClient, IConfiguration configuration, IMemoryCache cache)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        _cache = cache;
    }

    private async Task<List<CountryOverview>?> LoadCountriesAsync()
    {
        if (_cache.TryGetValue("country-catalog", out List<CountryOverview>? cached) && cached != null) return cached;
        var key = _configuration["ApiConfigs:CountriesApiKey"];
        if (string.IsNullOrWhiteSpace(key)) return null;
        var countries = new List<CountryOverview>();
        var baseUrl = (_configuration["ApiConfigs:CountriesBaseUrl"] ?? ApiBase).TrimEnd('/');
        try
        {
            for (var offset = 0; ;)
            {
                using var request = new HttpRequestMessage(HttpMethod.Get, $"{baseUrl}?limit=100&offset={offset}");
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", key.Trim());
                using var response = await _httpClient.SendAsync(request);
                if (!response.IsSuccessStatusCode) return null;
                using var document = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync());
                var objects = GetPath(document.RootElement, "data", "objects");
                if (objects.ValueKind != JsonValueKind.Array) return null;
                foreach (var item in objects.EnumerateArray())
                {
                    var itemCountry = MapV5(item);
                    if (itemCountry != null) countries.Add(itemCountry);
                }
                var count = objects.GetArrayLength();
                if (GetPath(document.RootElement, "data", "meta", "more").ValueKind != JsonValueKind.True) break;
                if (count == 0) return null;
                offset += count;
            }
            if (countries.Count == 0) return null;
            _cache.Set("country-catalog", countries, TimeSpan.FromHours(1));
            return countries;
        }
        catch (Exception ex) when (ex is HttpRequestException or OperationCanceledException or JsonException or InvalidOperationException) { return null; }
    }

    public async Task<IReadOnlyList<CountrySuggestion>?> GetCountries()
    {
        var countries = await LoadCountriesAsync();
        return countries?.Select(c => new CountrySuggestion(c.CountryName ?? c.Country!, c.Country!, c.CountryCode!)).ToList();
    }

    public async Task<CountryOverview?> GetCountry(string country)
    {
        if (string.IsNullOrWhiteSpace(country)) return null;
        var countries = await LoadCountriesAsync();
        if (countries == null) return null;
        var query = Normalize(country.Trim());
        return countries.FirstOrDefault(c => new[] { c.CountryName, c.Country, c.CountryCode }.Any(name => name != null && Normalize(name) == query));
    }

    private static string? PickCurrency(string? alpha2, IEnumerable<string> codes)
    {
        var list = codes.Where(c => !string.IsNullOrWhiteSpace(c)).ToList();
        if (list.Count == 0) return null;
        if (alpha2 != null && PreferredCurrency.TryGetValue(alpha2, out var preferred)
            && list.Contains(preferred, StringComparer.OrdinalIgnoreCase)) return preferred;
        return list.FirstOrDefault(c => !UnquotedCurrencies.Contains(c)) ?? list[0];
    }

    private static CountryOverview? MapV5(JsonElement item)
    {
        var common = Text(item, "names", "common");
        var code = Text(item, "codes", "alpha_2");
        if (string.IsNullOrWhiteSpace(common) || string.IsNullOrWhiteSpace(code)) return null;

        var portuguese = Text(item, "names", "translations", "por", "common");
        var capital = GetPath(item, "capitals");
        JsonElement primaryCapital = default;
        if (capital.ValueKind == JsonValueKind.Array)
        {
            var capitals = capital.EnumerateArray().ToList();
            primaryCapital = capitals.FirstOrDefault(c => GetPath(c, "attributes", "primary").ValueKind == JsonValueKind.True);
            if (primaryCapital.ValueKind != JsonValueKind.Object && capitals.Count > 0) primaryCapital = capitals[0];
        }

        var coords = GetPath(primaryCapital, "coordinates");
        double? lat = Number(coords, "lat") ?? Number(coords, "latitude");
        double? lng = Number(coords, "lng") ?? Number(coords, "lon") ?? Number(coords, "longitude");
        double[]? latlng = lat.HasValue && lng.HasValue ? [lat.Value, lng.Value] : null;

        var currencies = GetPath(item, "currencies");
        string? currencyCode = currencies.ValueKind switch
        {
            JsonValueKind.Array => PickCurrency(code, currencies.EnumerateArray().Select(c => Text(c, "code") ?? "")),
            JsonValueKind.Object => PickCurrency(code, currencies.EnumerateObject().Select(p => p.Name)),
            _ => null
        };

        var languages = GetPath(item, "languages");
        string? language = null;
        if (languages.ValueKind == JsonValueKind.Array)
            language = string.Join(", ", languages.EnumerateArray()
                .Select(l => Text(l, "name") ?? Text(l, "english") ?? Text(l, "native"))
                .Where(s => !string.IsNullOrWhiteSpace(s)).Distinct());
        else if (languages.ValueKind == JsonValueKind.Object)
            language = string.Join(", ", languages.EnumerateObject().Select(p => p.Value.GetString()).Where(s => !string.IsNullOrWhiteSpace(s)));

        var timezones = GetPath(item, "timezones");
        var timezoneList = timezones.ValueKind == JsonValueKind.Array
            ? timezones.EnumerateArray().Where(x => x.ValueKind == JsonValueKind.String).Select(x => x.GetString()!).ToList()
            : new List<string>();

        return new CountryOverview
        {
            Country = common,
            CountryName = portuguese ?? common,
            DataSource = "REST Countries API v5",
            Capital = Text(primaryCapital, "name"),
            CountryCode = code,
            CapitalCoordinates = latlng,
            CurrencyCode = currencyCode,
            Population = Number(item, "population") is double population ? (long)population : 0,
            FlagUrl = Text(item, "flag", "url_svg") ?? Text(item, "flag", "url_png"),
            Language = language,
            TimeZones = timezoneList
        };
    }

    private static JsonElement GetPath(JsonElement element, params string[] path)
    {
        foreach (var segment in path)
        {
            if (element.ValueKind != JsonValueKind.Object || !element.TryGetProperty(segment, out element))
                return default;
        }
        return element;
    }

    private static string? Text(JsonElement element, params string[] path)
    {
        var value = GetPath(element, path);
        return value.ValueKind == JsonValueKind.String ? value.GetString() : null;
    }

    private static double? Number(JsonElement element, params string[] path)
    {
        var value = GetPath(element, path);
        return value.ValueKind == JsonValueKind.Number && value.TryGetDouble(out var number) ? number : null;
    }

    private static string Normalize(string value) => string.Concat(value.Normalize(NormalizationForm.FormD)
        .Where(c => CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark))
        .ToUpperInvariant();

}
