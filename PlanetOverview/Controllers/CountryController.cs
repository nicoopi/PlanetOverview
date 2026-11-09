using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using PlanetOverview.Services.Interfaces;

namespace PlanetOverview.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CountryController : ControllerBase
{
    private readonly ICountryService _countryService;

    public CountryController(ICountryService countryService)
    {
        _countryService = countryService;
    }

    [HttpGet]
    [ResponseCache(Duration = 3600, Location = ResponseCacheLocation.Any)]
    public async Task<IActionResult> GetCountries()
    {
        var resultado = await _countryService.GetCountries();
        
        if (!resultado.Sucesso)
        {
            return StatusCode(503, new { erro = resultado.MensagemErro });
        }
        
        return Ok(resultado.Dados);
    }

    [HttpGet("{country}")]
    public async Task<IActionResult> GetCountry(string country)
    {
        if (string.IsNullOrWhiteSpace(country))
        {
            return BadRequest("O país é obrigatório.");
        }

        var resultado = await _countryService.GetCountry(country);

        if (!resultado.Sucesso)
        {
            return StatusCode(404, new { erro = resultado.MensagemErro });
        }

        return Ok(resultado.Dados);
    }
}