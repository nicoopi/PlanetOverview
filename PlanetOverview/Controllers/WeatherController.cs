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

            var weather = await _weatherService.GetWeather(capital);
            
            if (weather == null)
            {
                return NotFound($"Não foi possível encontrar o clima para {capital}.");
            }

            return Ok(weather);
        }
    }
}