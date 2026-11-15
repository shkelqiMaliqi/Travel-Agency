using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Travel_Agency_Portal.Models;
using Travel_Agency_Portal.Services;

namespace Travel_Agency_Portal.Controllers;

[Route("api/v1/[controller]")]
[ApiController]
[EnableRateLimiting("AuthPolicy")]
public class AuthController(AuthService service, PasswordResetService passwordReset) : ApiControllerBase
{

    [AllowAnonymous]
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        => ToActionResult(await service.Register(request));

    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
        => ToActionResult(await service.Login(request));

    [AllowAnonymous]
    [HttpPost("verify-mfa")]
    public async Task<IActionResult> VerifyMfa([FromBody] MfaVerifyRequest request)
        => ToActionResult(await service.VerifyMfa(request));

    [AllowAnonymous]
    [HttpPost("forgot-password")]
    [EnableRateLimiting("PasswordResetPolicy")]
    [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request)
        => ToActionResult(await passwordReset.RequestAsync(request));

    [AllowAnonymous]
    [HttpPost("reset-password")]
    [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
        => ToActionResult(await passwordReset.ResetAsync(request));

}
