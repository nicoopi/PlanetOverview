using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
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

        var resultado = await _currencyService.GetExchangeRateAsync(targetCurrency);

        if (!resultado.Sucesso)
        {
            return StatusCode(503, new { erro = resultado.MensagemErro });
        }

        return Ok(resultado.Dados);
    }
}