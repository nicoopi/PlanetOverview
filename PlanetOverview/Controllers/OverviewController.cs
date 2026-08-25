using Microsoft.AspNetCore.Mvc;
using PlanetOverview.Models;

namespace PlanetOverview.Controllers
{
    public class OverviewController : Controller
    {
        public IActionResult Index()
        {
            // Criando dados estáticos baseados no seu protótipo
            var mockData = new CountryOverviewViewModel
            {
                CountryName = "Brasil",
                FlagUrl = "https://flagcdn.com/w80/br.png",
                Capital = "Brasília",
                Language = "Português",
                Population = "203 milhões",
                Temperature = "24°C",
                LocalTime = "18:42",
                CurrencyConversion = "USD -> BRL"
            };

            // Envia os dados para a View
            return View(mockData); 
        }
    }
}