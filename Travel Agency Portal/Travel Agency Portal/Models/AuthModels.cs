using System.ComponentModel.DataAnnotations;

namespace Travel_Agency_Portal.Models;

public class RegisterRequest
{
    [Required]
    public string U_Name { get; set; } = string.Empty;

    [Required]
    public string U_Surname { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    public string U_Email { get; set; } = string.Empty;

    [Required]
    public string U_Username { get; set; } = string.Empty;

    public string? U_Phone { get; set; }

    [Required]
    [MinLength(6)]
    public string Password { get; set; } = string.Empty;

    [Required]
    public string ConfirmPassword { get; set; } = string.Empty;

    public string U_Type { get; set; } = "user";
}

public static class PasswordPolicy
{
    public const string Message = "Password must be at least 8 characters and include uppercase, lowercase, number, and special character.";

    public static bool IsValid(string password)
    {
        return !string.IsNullOrWhiteSpace(password) &&
               password.Length >= 8 &&
               password.Any(char.IsUpper) &&
               password.Any(char.IsLower) &&
               password.Any(char.IsDigit) &&
               password.Any(ch => !char.IsLetterOrDigit(ch));
    }
}

public class LoginRequest
{
    [Required]
    public string UserNameOrEmail { get; set; } = string.Empty;

    [Required]
    public string Password { get; set; } = string.Empty;
}

public class ForgotPasswordRequest
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;
}

public class ResetPasswordRequest
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string ResetCode { get; set; } = string.Empty;

    [Required]
    [MinLength(8)]
    public string NewPassword { get; set; } = string.Empty;

    [Required]
    public string ConfirmPassword { get; set; } = string.Empty;
}

public class AuthResponse
{
    public string Token { get; set; } = string.Empty;
    public DateTime ExpiresAtUtc { get; set; }
    public string Role { get; set; } = string.Empty;
    public int UserId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}
