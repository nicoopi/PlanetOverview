namespace PlanetOverview.Models;

public class CountryOverview
{
    public string? Country { get; set; }
    public string? Capital { get; set; }
    public string? CountryCode { get; set; }
    public long Population { get; set; } 
    public WeatherOverview? Weather { get; set; }
    public string? CountryName { get; set; }
    public string? FlagUrl { get; set; }
    public string? Language { get; set; }
    public string? Temperature { get; set; }
    public string? LocalTime { get; set; }
    public string? CurrencyConversion { get; set; }
}

public class CountryOverviewViewModel : CountryOverview
{
    public new string? Population
    {
        get => base.Population.ToString();
        set => base.Population = long.TryParse(value, out var parsed) ? parsed : 0;
    }
}