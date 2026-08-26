namespace PlanetOverview.Models.ApiResponses;

public class CountryApiResponse
{
    public required CountryName Name { get; set; }

    public string[]? Capital { get; set; }

    public required string Region { get; set; }

    public long Population { get; set; }

    public required string Cca2 { get; set; }

    public required string Cca3 { get; set; }
}

public class CountryName
{
    public required string Common { get; set; }

    public required string Official { get; set; }
}