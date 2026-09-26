using System.Text.Json.Serialization;

namespace PlanetOverview.Models.ApiResponses;

public class CountryApiResponse
{
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