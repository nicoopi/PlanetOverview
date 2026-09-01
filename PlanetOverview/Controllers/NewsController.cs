using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace PlanetOverview.Controllers
{
    [ApiController]
    [Route("api/[controller]")] 
    public class NewsController : ControllerBase
    {
                private readonly IConfiguration _config;
        public NewsController(IConfiguration config)
        {
            _config = config;
        }

        [HttpGet("{pais}")]
        public async Task<IActionResult> ObterNoticias(string pais)
        {
            string? chaveApi = _config["ApiConfigs:NewsApiKey"];

            string urlRequisicao = $"https://newsapi.org/v2/everything?q={pais}&apiKey={chaveApi}";

            using (HttpClient clienteHttp = new HttpClient())
            {
                clienteHttp.DefaultRequestHeaders.Add("User-Agent", "PlanetOverview-App");

                HttpResponseMessage resposta = await clienteHttp.GetAsync(urlRequisicao);

                if (resposta.IsSuccessStatusCode)
                {
                    string corpoJson = await resposta.Content.ReadAsStringAsync();

                    return Content(corpoJson, "application/json");
                }
                else
                {
                    return StatusCode((int)resposta.StatusCode, "Falha na comunicação com o serviço de notícias.");
                }
            }
        }
    }
}