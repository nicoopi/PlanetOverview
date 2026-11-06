using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using PlanetOverview.Data;
using PlanetOverview.Models;
using Microsoft.Extensions.Localization;
using System.Security.Cryptography;
using System.Text;

namespace PlanetOverview.Controllers;

[ApiController, Route("api/account")]
[AutoValidateAntiforgeryToken]
[ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
public class AccountApiController(UserManager<AppUser> users, SignInManager<AppUser> signIn,
    AccountDbContext database, IAntiforgery antiforgery, IStringLocalizer<SharedResource> text) : ControllerBase
{
    [HttpGet("csrf"), AllowAnonymous]
    public IActionResult Csrf() => Ok(new { token = antiforgery.GetAndStoreTokens(HttpContext).RequestToken });

    private const string RecoveryProvider = "PlanetOverview";
    private const string RecoveryToken = "PasswordRecovery";

    [HttpPost("recovery-code"), Authorize, EnableRateLimiting("account-entry")]
    public async Task<IActionResult> RecoveryCode(DeleteAccountRequest request)
    {
        var user = await users.GetUserAsync(User);
        if (user is null) return Unauthorized();
        if (!(await signIn.CheckPasswordSignInAsync(user, request.CurrentPassword, lockoutOnFailure: true)).Succeeded)
            return Problem(statusCode: 400, title: text["account_error_password"]);
        var code = Convert.ToHexString(RandomNumberGenerator.GetBytes(16));
        var result = await users.SetAuthenticationTokenAsync(user, RecoveryProvider, RecoveryToken, HashCode(code));
        if (!result.Succeeded) return IdentityError(result);
        return Ok(new { recoveryCode = string.Join("-", Enumerable.Range(0, 8).Select(i => code.Substring(i * 4, 4))) });
    }

    [HttpPost("forgot-password"), AllowAnonymous, EnableRateLimiting("account-entry")]
    public async Task<IActionResult> ForgotPassword(ForgotPasswordRequest request)
    {
        // Consume the recovery code and update the password atomically. Only its hash is stored.
        await using var transaction = await database.Database.BeginTransactionAsync();
        var user = await users.FindByEmailAsync(request.Email.Trim());
        var stored = user is null ? null : await users.GetAuthenticationTokenAsync(user, RecoveryProvider, RecoveryToken);
        var supplied = HashCode(request.RecoveryCode);
        if (stored is null || !CryptographicOperations.FixedTimeEquals(Encoding.UTF8.GetBytes(stored), Encoding.UTF8.GetBytes(supplied)))
            return Problem(statusCode: 400, title: text["account_error_recovery"]);
        var token = await users.GeneratePasswordResetTokenAsync(user!);
        var result = await users.ResetPasswordAsync(user!, token, request.NewPassword);
        if (!result.Succeeded) return IdentityError(result);
        result = await users.SetLockoutEndDateAsync(user!, null);
        if (!result.Succeeded) return IdentityError(result);
        result = await users.ResetAccessFailedCountAsync(user!);
        if (!result.Succeeded) return IdentityError(result);
        result = await users.RemoveAuthenticationTokenAsync(user!, RecoveryProvider, RecoveryToken);
        if (!result.Succeeded) return IdentityError(result);
        await transaction.CommitAsync();
        await signIn.SignOutAsync();
        return NoContent();
    }

    private static string HashCode(string code) => Convert.ToHexString(SHA256.HashData(
        Encoding.UTF8.GetBytes(code.Replace("-", "").Replace(" ", "").ToUpperInvariant())));

    [HttpPost("register"), AllowAnonymous, EnableRateLimiting("account-entry")]
    public async Task<IActionResult> Register(RegisterRequest request)
    {
        var name = request.Name.Trim();
        if (name.Length < 2) return Problem(statusCode: 400, title: text["account_error_name"]);
        var user = new AppUser { DisplayName = name, Email = request.Email.Trim(), UserName = request.Email.Trim() };
        IdentityResult result;
        try { result = await users.CreateAsync(user, request.Password); }
        catch (DbUpdateException exception) when (exception.InnerException is Microsoft.Data.Sqlite.SqliteException { SqliteExtendedErrorCode: 2067 })
        {
            return Problem(statusCode: 409, title: text["account_error_duplicate"]);
        }
        if (!result.Succeeded) return IdentityError(result);
        await signIn.SignInAsync(user, isPersistent: false);
        return CreatedAtAction(nameof(Me), AccountResponse.From(user));
    }

    [HttpPost("login"), AllowAnonymous, EnableRateLimiting("account-entry")]
    public async Task<IActionResult> Login(LoginRequest request)
    {
        var result = await signIn.PasswordSignInAsync(request.Email.Trim(), request.Password,
            isPersistent: false, lockoutOnFailure: true);
        if (!result.Succeeded)
            return Problem(statusCode: 401, title: text["account_error_login"]);
        var user = await users.FindByEmailAsync(request.Email.Trim());
        return Ok(AccountResponse.From(user!));
    }

    [HttpPost("logout"), Authorize]
    public async Task<IActionResult> Logout()
    {
        await signIn.SignOutAsync();
        return NoContent();
    }

    [HttpGet("me"), Authorize]
    public async Task<IActionResult> Me()
    {
        var user = await users.GetUserAsync(User);
        return user is null ? Unauthorized() : Ok(AccountResponse.From(user));
    }

    [HttpPut("me"), Authorize, EnableRateLimiting("account-entry")]
    public async Task<IActionResult> Update(UpdateAccountRequest request)
    {
        var user = await users.GetUserAsync(User);
        if (user is null) return Unauthorized();
        if (!(await signIn.CheckPasswordSignInAsync(user, request.CurrentPassword, lockoutOnFailure: true)).Succeeded)
            return Problem(statusCode: 400, title: text["account_error_password"]);
        if (request.Name.Trim().Length < 2)
            return Problem(statusCode: 400, title: text["account_error_name"]);

        // Keep email/profile/password changes atomic if any Identity validation fails.
        await using var transaction = await database.Database.BeginTransactionAsync();
        user.DisplayName = request.Name.Trim();
        user.Email = request.Email.Trim();
        user.UserName = user.Email;
        IdentityResult result;
        try { result = await users.UpdateAsync(user); }
        catch (DbUpdateException exception) when (exception.InnerException is Microsoft.Data.Sqlite.SqliteException { SqliteExtendedErrorCode: 2067 })
        {
            return Problem(statusCode: 409, title: text["account_error_duplicate"]);
        }
        if (!result.Succeeded) return IdentityError(result);
        if (request.NewPassword is not null)
        {
            result = await users.ChangePasswordAsync(user, request.CurrentPassword, request.NewPassword);
            if (!result.Succeeded) return IdentityError(result);
        }
        // Invalidate other sessions, then issue a fresh cookie for the current one.
        result = await users.UpdateSecurityStampAsync(user);
        if (!result.Succeeded) return IdentityError(result);
        await transaction.CommitAsync();
        await signIn.RefreshSignInAsync(user);
        return Ok(AccountResponse.From(user));
    }

    [HttpDelete("me"), Authorize, EnableRateLimiting("account-entry")]
    public async Task<IActionResult> Delete(DeleteAccountRequest request)
    {
        var user = await users.GetUserAsync(User);
        if (user is null) return Unauthorized();
        if (!(await signIn.CheckPasswordSignInAsync(user, request.CurrentPassword, lockoutOnFailure: true)).Succeeded)
            return Problem(statusCode: 400, title: text["account_error_password"]);
        var result = await users.DeleteAsync(user);
        if (!result.Succeeded) return IdentityError(result);
        await signIn.SignOutAsync();
        return NoContent();
    }

    private ObjectResult IdentityError(IdentityResult result)
    {
        var duplicate = result.Errors.Any(error => error.Code is "DuplicateEmail" or "DuplicateUserName");
        return Problem(statusCode: duplicate ? 409 : 400,
            title: duplicate ? text["account_error_duplicate"] : text["account_error_save"],
            detail: duplicate ? null : text["account_error_details"].Value);
    }
}
