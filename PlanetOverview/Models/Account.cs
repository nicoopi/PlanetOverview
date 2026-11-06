using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace PlanetOverview.Models;

public class AppUser : IdentityUser
{
    public string DisplayName { get; set; } = "";
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}

public class RegisterRequest
{
    [Required, StringLength(100, MinimumLength = 2)]
    public string Name { get; set; } = "";
    [Required, EmailAddress, StringLength(254)]
    public string Email { get; set; } = "";
    [Required, StringLength(128, MinimumLength = 8)]
    public string Password { get; set; } = "";
}

public class LoginRequest
{
    [Required, EmailAddress, StringLength(254)]
    public string Email { get; set; } = "";
    [Required, StringLength(128)]
    public string Password { get; set; } = "";
}

public class UpdateAccountRequest
{
    [Required, StringLength(100, MinimumLength = 2)]
    public string Name { get; set; } = "";
    [Required, EmailAddress, StringLength(254)]
    public string Email { get; set; } = "";
    [Required, StringLength(128)]
    public string CurrentPassword { get; set; } = "";
    [StringLength(128, MinimumLength = 8)]
    public string? NewPassword { get; set; }
}

public class DeleteAccountRequest
{
    [Required, StringLength(128)]
    public string CurrentPassword { get; set; } = "";
}

public class ForgotPasswordRequest
{
    [Required, EmailAddress, StringLength(254)]
    public string Email { get; set; } = "";
    [Required, StringLength(64)]
    public string RecoveryCode { get; set; } = "";
    [Required, StringLength(128, MinimumLength = 8)]
    public string NewPassword { get; set; } = "";
}

public record AccountResponse(string Id, string Name, string Email, DateTimeOffset CreatedAt)
{
    public static AccountResponse From(AppUser user) => new(user.Id, user.DisplayName, user.Email!, user.CreatedAt);
}
