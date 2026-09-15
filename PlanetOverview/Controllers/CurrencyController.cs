using Microsoft.AspNetCore.Mvc;
using PlanetOverview.Services.Interfaces;

namespace PlanetOverview.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CurrencyController : ControllerBase
{
    private readonly ICurrencyService _currencyService;

    public CurrencyController(ICurrencyService currencyService)
    {
        _currencyService = currencyService;
    }

    [HttpGet("{targetCurrency}")]
    public async Task<IActionResult> GetExchangeRate(string targetCurrency)
    {
        if (string.IsNullOrWhiteSpace(targetCurrency))
        {
            return BadRequest("A moeda de destino é obrigatória (ex: BRL).");
        }

        var currency = await _currencyService.GetExchangeRateAsync(targetCurrency);

        if (currency == null)
        {
            return NotFound($"Não foi possível encontrar a cotação para a moeda {targetCurrency}.");
        }

        return Ok(currency);
    }
}