namespace PlanetOverview.Models;

public class CurrencyOverview
{
    public required string BaseCurrency { get; set; }

    public required string TargetCurrency { get; set; }
    
    public required decimal ExchangeRate { get; set; }
}