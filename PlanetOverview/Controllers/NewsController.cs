using Microsoft.AspNetCore.Mvc;
using PlanetOverview.Services.Interfaces;

namespace PlanetOverview.Controllers;

[ApiController]
[Route("api/[controller]")]
public class NewsController : ControllerBase
{
    private readonly INewsService _newsService;

    public NewsController(INewsService newsService)
    {
        _newsService = newsService;
    }

    [HttpGet("{pais}")]
    public async Task<IActionResult> ObterNoticias(string pais)
    {
        var result = await _newsService.GetNewsAsync(pais);

        if (result == null)
        {
            return StatusCode(StatusCodes.Status503ServiceUnavailable, "Não foi possível consultar o serviço de notícias.");
        }

        return Ok(result);
    }
}
