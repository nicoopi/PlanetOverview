using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace PlanetOverview.Controllers;

[Route("account")]
[ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
public class AccountController : Controller
{
    [HttpGet("login")]
    public IActionResult Login() => Entry("login");

    [HttpGet("register")]
    public IActionResult Register() => Entry("register");

    [HttpGet("forgot-password")]
    public IActionResult ForgotPassword() => View("Index", "forgot");

    private IActionResult Entry(string mode)
    {
        if (User.Identity?.IsAuthenticated == true) return Redirect("/");
        return View("Index", mode);
    }

    [Authorize, HttpGet("profile")]
    public IActionResult Profile() => View("Index", "profile");
}
