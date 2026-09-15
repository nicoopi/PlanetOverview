namespace PlanetOverview.Models.ApiResponses;

public class CurrencyApiResponse
{
    public required string Base { get; set; }

    public required Dictionary<string, string> Rates { get; set; }
}