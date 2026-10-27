using Microsoft.AspNetCore.Mvc;
using PlanetOverview.Models;
namespace PlanetOverview.Controllers;
public class OverviewController : Controller
{
    public IActionResult Index() => View();
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error() => View("~/Views/Shared/Error.cshtml", new ErrorViewModel { RequestId = HttpContext.TraceIdentifier });
}
