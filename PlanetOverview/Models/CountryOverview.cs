namespace PlanetOverview.Models;

public class CountryOverview
{
    public required string Country { get; set; }

    public required string Capital { get; set; }

    public required string CountryCode { get; set; }

    public long Population { get; set; }

    public WeatherOverview? Weather { get; set;}
}