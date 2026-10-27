using System.Text.Json.Serialization;

namespace PlanetOverview.Models.ApiResponses;

public class CountryApiResponse
{
    public string[]? AltSpellings { get; set; }
    public CapitalCoordinates? CapitalInfo { get; set; }
    public Dictionary<string, CountryName>? Translations { get; set; }
    public Dictionary<string, CountryCurrency>? Currencies { get; set; }
    [JsonPropertyName("name")]
    public CountryName? Name { get; set; }

    [JsonPropertyName("capital")]
    public string[]? Capital { get; set; }

    [JsonPropertyName("region")]
    public string? Region { get; set; }

    [JsonPropertyName("population")]
    public long Population { get; set; }

    [JsonPropertyName("cca2")]
    public string? Cca2 { get; set; }

    [JsonPropertyName("cca3")]
    public string? Cca3 { get; set; }

    [JsonPropertyName("flags")]
    public CountryFlags? Flags { get; set; }

    [JsonPropertyName("languages")]
    public Dictionary<string, string>? Languages { get; set; }

    [JsonPropertyName("timezones")]
    public string[]? Timezones { get; set; }
}

public class CountryName
{
    [JsonPropertyName("common")]
    public string? Common { get; set; }

    [JsonPropertyName("official")]
    public string? Official { get; set; }
}

public class CountryFlags
{
    [JsonPropertyName("png")]
    public string? Png { get; set; }

    [JsonPropertyName("svg")]
    public string? Svg { get; set; }
}
public class CountryCurrency
{
    public string? Name { get; set; }
    public string? Symbol { get; set; }
}

public class CapitalCoordinates
{
    public double[]? Latlng { get; set; }
}
