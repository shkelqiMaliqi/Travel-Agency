using Travel_Agency_Portal.Data;
using Travel_Agency_Portal.Models;

namespace Travel_Agency_Portal.Services;

public class AuthService : OperationService
{
    private readonly AuthRepository _repository;
    private readonly JwtTokenService _jwtTokenService;
    private readonly MfaService _mfaService;
    private readonly AuditLogService _auditLogService;
    private readonly MongoAnalyticsService _mongoAnalyticsService;

    public AuthService(AuthRepository repository,
        JwtTokenService jwtTokenService,
        MfaService mfaService,
        AuditLogService auditLogService,
        MongoAnalyticsService mongoAnalyticsService)
    {
        _repository = repository;
        _jwtTokenService = jwtTokenService;
        _mfaService = mfaService;
        _auditLogService = auditLogService;
        _mongoAnalyticsService = mongoAnalyticsService;
    }

    public async Task<OperationResult> Register(RegisterRequest request)
    {
        if (!string.Equals(request.Password, request.ConfirmPassword, StringComparison.Ordinal))
        {
            return BadRequest(new { message = "Passwords do not match." });
        }

        if (!PasswordPolicy.IsValid(request.Password))
        {
            return BadRequest(new { message = PasswordPolicy.Message });
        }

        var existingUsers = _repository.CountConflicts(request);

        if (existingUsers > 0)
        {
            return Conflict(new { message = "A user with this email or username already exists." });
        }

        var hashedPassword = PasswordHasher.Hash(request.Password);
        const string publicRegistrationRole = "user";
        var userId = _repository.Create(request, hashedPassword);

        var user = new Users
        {
            U_Id = userId,
            U_Name = request.U_Name,
            U_Surname = request.U_Surname,
            U_Email = request.U_Email,
            U_Username = request.U_Username,
            U_Phone = request.U_Phone,
            U_Type = publicRegistrationRole
        };

        await _mongoAnalyticsService.TrackAsync("user.registered", "auth", user.U_Email);

        return Ok(_jwtTokenService.CreateToken(user));
    }

    public async Task<OperationResult> Login(LoginRequest request)
    {
        var user = _repository.FindForLogin(request);
        if (user is null || !PasswordHasher.Verify(request.Password, user.U_Password))
            return Unauthorized(new { message = "Invalid credentials." });

        if (_mfaService.IsRequiredForRole(user.U_Type))
        {
            var (code, expiresAt) = await _mfaService.CreateCodeAsync(user);
            await _auditLogService.WriteSystemEventAsync("auth.mfa.challenge", $"MFA challenge created for {user.U_Email}.");
            return Ok(new AuthResponse
            {
                RequiresMfa = true,
                Role = user.U_Type,
                UserId = user.U_Id,
                Name = $"{user.U_Name} {user.U_Surname}".Trim(),
                Email = user.U_Email,
                MfaCode = null,
                MfaExpiresAtUtc = expiresAt
            });
        }

        await _mongoAnalyticsService.TrackAsync("user.logged_in", "auth", user.U_Email);
        return Ok(_jwtTokenService.CreateToken(user));
    }

    public async Task<OperationResult> VerifyMfa(MfaVerifyRequest request)
    {
        if (!await _mfaService.VerifyCodeAsync(request.Email, request.Code))
        {
            return Unauthorized(new { message = "MFA code is invalid or expired." });
        }

        var user = _repository.FindForMfa(request);
        if (user is null) return Unauthorized(new { message = "User not found for MFA verification." });

        await _mongoAnalyticsService.TrackAsync("user.logged_in_mfa", "auth", user.U_Email);
        return Ok(_jwtTokenService.CreateToken(user));
    }

}
