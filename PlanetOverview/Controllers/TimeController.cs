using Microsoft.AspNetCore.Mvc;
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
        var result = await _timeService.GetByCoordinatesAsync(latitude, longitude);
        return result == null ? NotFound("Horário indisponível.") : Ok(result);
    }

    [HttpGet]
    public async Task<IActionResult> GetCurrentTime(
        [FromQuery] string timeZone)
    {
        if (string.IsNullOrWhiteSpace(timeZone))
        {
            return BadRequest(
                "O fuso horário é obrigatório. Exemplo: America/Sao_Paulo."
            );
        }

        var result = await _timeService
            .GetCurrentTimeAsync(timeZone);

        if (result == null)
        {
            return NotFound(
                $"Não foi possível encontrar o fuso horário '{timeZone}'."
            );
        }

        return Ok(result);
    }
}