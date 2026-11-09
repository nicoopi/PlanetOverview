using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using PlanetOverview.Services.Interfaces; 
using PlanetOverview.Models;

namespace PlanetOverview.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class WeatherController : ControllerBase
    {
        private readonly IWeatherService _weatherService;

        public WeatherController(IWeatherService weatherService)
        {
            _weatherService = weatherService;
        }

        // Rota: GET api/weather/{capital}
        [HttpGet("{capital}")]
        public async Task<IActionResult> GetWeather(string capital)
        {
            if (string.IsNullOrWhiteSpace(capital))
            {
                return BadRequest("O nome da capital é obrigatório.");
            }

            // O retorno agora é uma tupla
            var resultado = await _weatherService.GetWeather(capital);
            
            if (!resultado.Sucesso)
            {
                // Se falhou por falta de internet ou cidade inválida, devolve a mensagem exata do serviço
                return StatusCode(503, new { erro = resultado.MensagemErro });
            }

            // Se deu sucesso, devolve os dados
            return Ok(resultado.Dados);
        }
    }
}