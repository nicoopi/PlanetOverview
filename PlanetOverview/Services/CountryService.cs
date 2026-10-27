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
    private static readonly CountryApiResponse[] LocalCountries = LoadLocalCountries();

    // Índice calculado uma vez (antes, cada busca normalizava ~25 mil strings).
    private static readonly (CountryApiResponse Country, HashSet<string> Names)[] LocalIndex =
        LocalCountries.Select(c => (c, LocalNames(c).Select(Normalize).ToHashSet())).ToArray();

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

    private static CountryApiResponse[] LoadLocalCountries()
    {
        using var stream = typeof(CountryService).Assembly.GetManifestResourceStream("PlanetOverview.Data.countries.json")
            ?? throw new InvalidOperationException("Base local de países não encontrada.");
        return JsonSerializer.Deserialize<CountryApiResponse[]>(stream,
            new JsonSerializerOptions(JsonSerializerDefaults.Web)) ?? [];
    }

    public async Task<CountryOverview?> GetCountry(string country)
    {
        if (string.IsNullOrWhiteSpace(country)) return null;

        // 1) Resolve o nome digitado (pt/en/sigla) para UM país na base local, de forma exata.
        //    Evita o erro de busca parcial da API (ex.: "Estados Unidos" casar com "Estados Unidos Mexicanos").
        var local = FindLocalCountry(country);
        if (local?.CountryCode == null) return null;

        var cacheKey = $"country:{local.CountryCode}";
        if (_cache.TryGetValue(cacheKey, out CountryOverview? cached) && cached != null) return cached;

        // 2) Consulta a v5 por código ISO (endpoint documentado de leitura) e completa campos ausentes com a base local.
        var live = await GetLiveCountryAsync(local.CountryCode);
        var result = live == null ? local : Merge(live, local);

        _cache.Set(cacheKey, result, live != null ? TimeSpan.FromHours(1) : TimeSpan.FromMinutes(5));
        return result;
    }

    private async Task<CountryOverview?> GetLiveCountryAsync(string alpha2)
    {
        var apiKey = _configuration["ApiConfigs:CountriesApiKey"];
        if (string.IsNullOrWhiteSpace(apiKey)) return null;
        try
        {
            var baseUrl = (_configuration["ApiConfigs:CountriesBaseUrl"] ?? ApiBase).TrimEnd('/');
            using var request = new HttpRequestMessage(HttpMethod.Get,
                $"{baseUrl}/codes.alpha_2/{Uri.EscapeDataString(alpha2)}");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKey.Trim());
            using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(5)); // não deixa a tela esperando 20s
            using var response = await _httpClient.SendAsync(request, timeout.Token);
            if (!response.IsSuccessStatusCode) return null;

            using var document = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync(timeout.Token), cancellationToken: timeout.Token);
            var objects = GetPath(document.RootElement, "data", "objects");
            if (objects.ValueKind != JsonValueKind.Array) return null;
            foreach (var item in objects.EnumerateArray())
                if (string.Equals(Text(item, "codes", "alpha_2"), alpha2, StringComparison.OrdinalIgnoreCase))
                    return MapV5(item);
        }
        catch (Exception ex) when (ex is HttpRequestException or OperationCanceledException or JsonException or InvalidOperationException)
        {
            // A indisponibilidade da API externa não deve derrubar o restante do painel.
        }
        return null;
    }

    // Campo a campo: o que a v5 não trouxe (ou veio em formato inesperado) vem da base local.
    private static CountryOverview Merge(CountryOverview live, CountryOverview local)
    {
        var filled = false;
        T? Pick<T>(T? fromLive, T? fromLocal, Func<T?, bool> isEmpty)
        {
            if (!isEmpty(fromLive)) return fromLive;
            if (!isEmpty(fromLocal)) filled = true;
            return fromLocal;
        }
        var merged = new CountryOverview
        {
            Country = live.Country,
            CountryCode = live.CountryCode,
            CountryName = live.CountryName,
            Population = live.Population > 0 ? live.Population : local.Population,
            Capital = Pick(live.Capital, local.Capital, string.IsNullOrWhiteSpace),
            CapitalCoordinates = Pick(live.CapitalCoordinates, local.CapitalCoordinates, c => c is not { Length: >= 2 }),
            CurrencyCode = Pick(live.CurrencyCode, local.CurrencyCode, string.IsNullOrWhiteSpace),
            Language = Pick(live.Language, local.Language, string.IsNullOrWhiteSpace),
            FlagUrl = Pick(live.FlagUrl, local.FlagUrl, string.IsNullOrWhiteSpace),
            TimeZones = live.TimeZones.Count > 0 ? live.TimeZones : local.TimeZones,
        };
        if (live.Population <= 0) filled = true;
        merged.DataSource = filled ? "REST Countries API v5 + base local (campos complementares)" : "REST Countries API v5";
        return merged;
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
        string? currencyCode = currencies.ValueKind == JsonValueKind.Object
            ? PickCurrency(code, currencies.EnumerateObject().Select(p => p.Name))
            : null;

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

    private static CountryOverview? FindLocalCountry(string country)
    {
        var query = Normalize(country.Trim());
        var selected = LocalIndex.FirstOrDefault(i => i.Names.Contains(query)).Country;
        if (selected == null && query.Length >= 3) // "a" ou "ni" não devem "adivinhar" um país
            selected = LocalIndex.FirstOrDefault(i => i.Names.Any(n => n.StartsWith(query))).Country
                ?? LocalIndex.FirstOrDefault(i => i.Names.Any(n => n.Contains(query))).Country;
        if (selected?.Name?.Common == null || string.IsNullOrWhiteSpace(selected.Cca2)) return null;
        var portuguese = selected.Translations?.GetValueOrDefault("por");
        return new CountryOverview
        {
            CapitalCoordinates = selected.CapitalInfo?.Latlng,
            Country = selected.Name.Common,
            CountryName = portuguese?.Common ?? selected.Name.Common,
            Capital = selected.Capital?.FirstOrDefault(),
            CountryCode = selected.Cca2,
            CurrencyCode = PickCurrency(selected.Cca2, selected.Currencies?.Keys ?? Enumerable.Empty<string>()),
            Population = selected.Population,
            FlagUrl = selected.Flags?.Svg ?? selected.Flags?.Png,
            Language = selected.Languages == null ? null : string.Join(", ", selected.Languages.Values),
            TimeZones = selected.Timezones?.ToList() ?? [],
            DataSource = "Base local REST Countries (fallback)"
        };
    }

    private static IEnumerable<string> LocalNames(CountryApiResponse country) =>
        new[] { country.Name?.Common, country.Name?.Official, country.Cca2 }
            .Concat(country.Translations?.Values.SelectMany(t => new[] { t.Common, t.Official }) ?? [])
            .Concat(country.AltSpellings ?? [])
            .OfType<string>();

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

    public IReadOnlyList<CountrySuggestion> GetCountries()
    {
        throw new NotImplementedException();
    }
}
