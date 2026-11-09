using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
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
        if (string.IsNullOrWhiteSpace(pais))
        {
            return BadRequest("O país é obrigatório.");
        }

        var resultado = await _newsService.GetNewsAsync(pais);

        if (!resultado.Sucesso)
        {
            return StatusCode(503, new { erro = resultado.MensagemErro });
        }

        return Ok(resultado.Dados);
    }
}