using System.Security.Cryptography;
using Travel_Agency_Portal.Data;
using Travel_Agency_Portal.Models;

namespace Travel_Agency_Portal.Services;

public sealed class PasswordResetService(
    IPasswordResetRepository repository,
    IPasswordResetDelivery delivery,
    ILogger<PasswordResetService> logger) : OperationService
{
    public const string RequestMessage = "If an account exists with that email, a password reset code will be sent. Check your inbox.";

    public async Task<OperationResult> RequestAsync(ForgotPasswordRequest request)
    {
        var email = request.Email.Trim();
        var token = Convert.ToHexString(RandomNumberGenerator.GetBytes(32));
        var expires = DateTime.UtcNow.AddMinutes(15);
        if (await repository.CreateAsync(email, PasswordHasher.HashToken(token), expires))
        {
            try
            {
                await delivery.SendAsync(email, token, expires);
            }
            catch (Exception ex)
            {
                // Never expose SMTP errors, account existence, or tokens to the caller.
                logger.LogError(ex, "Password reset delivery failed.");
            }
        }
        return Ok(new { message = RequestMessage });
    }

    public async Task<OperationResult> ResetAsync(ResetPasswordRequest request)
    {
        if (request.NewPassword != request.ConfirmPassword)
            return BadRequest(new { message = "Passwords do not match." });
        if (!PasswordPolicy.IsValid(request.NewPassword))
            return BadRequest(new { message = PasswordPolicy.Message });
        var token = request.ResetCode.Trim();
        if (token.Length != 64 || !token.All(Uri.IsHexDigit))
            return BadRequest(new { message = "Reset code is invalid or expired." });
        var changed = await repository.ConsumeAsync(request.Email.Trim(),
            PasswordHasher.HashToken(token), PasswordHasher.Hash(request.NewPassword));
        return changed
            ? Ok(new { message = "Password reset successfully. You can now sign in." })
            : BadRequest(new { message = "Reset code is invalid or expired." });
    }
}
