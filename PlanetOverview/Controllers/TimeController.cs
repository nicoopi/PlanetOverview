using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using PlanetOverview.Services.Interfaces;

namespace PlanetOverview.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TimeController : ControllerBase
{
    private readonly ITimeService _timeService;

    public TimeController(ITimeService timeService)
    {
        _timeService = timeService;
    }

    [HttpGet("coordinates")]
    public async Task<IActionResult> GetByCoordinates([FromQuery] double latitude, [FromQuery] double longitude)
    {
        if (!double.IsFinite(latitude) || !double.IsFinite(longitude) || latitude < -90 || latitude > 90 || longitude < -180 || longitude > 180)
            return BadRequest("Coordenadas inválidas.");
            
        var resultado = await _timeService.GetByCoordinatesAsync(latitude, longitude);
        
        if (!resultado.Sucesso)
        {
            // Oculta a complexidade do erro, enviando apenas a mensagem formatada para o JS
            return StatusCode(503, new { erro = resultado.MensagemErro });
        }
        
        return Ok(resultado.Dados);
    }

    [HttpGet]
    public async Task<IActionResult> GetCurrentTime([FromQuery] string timeZone)
    {
        if (string.IsNullOrWhiteSpace(timeZone))
        {
            return BadRequest("O fuso horário é obrigatório. Exemplo: America/Sao_Paulo.");
        }

        var resultado = await _timeService.GetCurrentTimeAsync(timeZone);

        if (!resultado.Sucesso)
        {
            return StatusCode(503, new { erro = resultado.MensagemErro });
        }

        return Ok(resultado.Dados);
    }
}