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