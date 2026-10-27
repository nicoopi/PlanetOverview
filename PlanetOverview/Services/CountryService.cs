using PlanetOverview.Models;
using PlanetOverview.Models.ApiResponses;
using PlanetOverview.Services.Interfaces;
namespace PlanetOverview.Services;
public class CountryService : ICountryService
{
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

    public Task<CountryOverview?> GetCountry(string country) => Task.FromResult(FindCountry(country));

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
