namespace PlanetOverview.Models.ApiResponses;

public class WeatherApiResponse
{
    public required MainWeatherData Main { get; set; }

    public required WeatherCondition[] Weather { get; set;}

    public required string Name { get; set; }
}

public class MainWeatherData
{
    public required double Temp { get; set; }
}

public class WeatherCondition
{
    public required string Description { get; set; }
}