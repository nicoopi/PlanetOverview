using Microsoft.AspNetCore.Mvc;
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

    [HttpGet("{country}")]
    public async Task<IActionResult> GetCountry(string country)
    {
        if (string.IsNullOrWhiteSpace(country))
        {
            return BadRequest("O país é obrigatório.");
        }

        var result = await _countryService.GetCountry(country);

        if (result == null)
        {
            return NotFound(
                $"Não foi possível encontrar o país '{country}'."
            );
        }

        return Ok(result);
    }
}