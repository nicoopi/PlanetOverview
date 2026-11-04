using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using System;
using PlanetOverview.Models;

namespace PlanetOverview.Controllers;

public class OverviewController : Controller
{
    public IActionResult Index() => View();
    
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error() => View("~/Views/Shared/Error.cshtml", new ErrorViewModel { RequestId = HttpContext.TraceIdentifier });

    public IActionResult ComoFunciona()
    {
        return View();
    }

    // NOVA FUNÇÃO: Guarda o idioma escolhido no Cookie e recarrega a página
    [HttpPost]
    public IActionResult MudarIdioma(string culture, string returnUrl)
    {
        Response.Cookies.Append(
            CookieRequestCultureProvider.DefaultCookieName,
            CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(culture)),
            new CookieOptions { Expires = DateTimeOffset.UtcNow.AddYears(1) }
        );

        return LocalRedirect(returnUrl ?? "/");
    }
}