using System.Text.Json.Serialization;

namespace PlanetOverview.Models.ApiResponses;

public class CountryV5Response
{
    public CountryV5Data? Data { get; set; }
}

public class CountryV5Data
{
    public List<CountryV5>? Objects { get; set; }
}

public class CountryV5
{
    public CountryV5Names? Names { get; set; }
    public CountryV5Codes? Codes { get; set; }
    public List<CountryV5Capital>? Capitals { get; set; }
    public List<CountryV5Currency>? Currencies { get; set; }
    public List<CountryV5Language>? Languages { get; set; }
    public CountryV5Flag? Flag { get; set; }
    public long? Population { get; set; }
    public string[]? Timezones { get; set; }
}

public class CountryV5Names : CountryName
{
    public Dictionary<string, CountryName>? Translations { get; set; }
}

public class CountryV5Codes
{
    [JsonPropertyName("alpha_2")]
    public string? Alpha2 { get; set; }
}

public class CountryV5Capital
{
    public string? Name { get; set; }
    public CountryV5Coordinates? Coordinates { get; set; }
    public CountryV5CapitalAttributes? Attributes { get; set; }
}

public class CountryV5Coordinates
{
    public double? Lat { get; set; }
    public double? Lng { get; set; }
}

public class CountryV5CapitalAttributes
{
    public bool Primary { get; set; }
}

public class CountryV5Currency
{
    public string? Code { get; set; }
}

public class CountryV5Language
{
    public string? Name { get; set; }
}

public class CountryV5Flag
{
    [JsonPropertyName("url_svg")]
    public string? Svg { get; set; }
    [JsonPropertyName("url_png")]
    public string? Png { get; set; }
}
