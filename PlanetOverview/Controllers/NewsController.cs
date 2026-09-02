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

        if (result == null || result.Articles == null || result.Articles.Count == 0)
        {
            return NotFound("Nenhuma notícia encontrada ou falha na comunicação com o serviço.");
        }

        return Ok(result);
    }
}