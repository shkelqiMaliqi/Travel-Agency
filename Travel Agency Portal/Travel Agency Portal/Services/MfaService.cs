using Microsoft.EntityFrameworkCore;
using Travel_Agency_Portal.Data;
using Travel_Agency_Portal.Models;

namespace Travel_Agency_Portal.Services;

public class MfaService
{
    private readonly TravelAgencyDbContext _dbContext;
    private readonly IConfiguration _configuration;
    private readonly MfaDeliveryService _mfaDeliveryService;

    public MfaService(
        TravelAgencyDbContext dbContext,
        IConfiguration configuration,
        MfaDeliveryService mfaDeliveryService)
    {
        _dbContext = dbContext;
        _configuration = configuration;
        _mfaDeliveryService = mfaDeliveryService;
    }

    public bool IsRequiredForRole(string role)
    {
        var requiredRoles = (_configuration["Security:MfaRequiredRoles"] ?? "admin")
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        return requiredRoles.Contains(role, StringComparer.OrdinalIgnoreCase);
    }

    public async Task<(string code, DateTime expiresAtUtc)> CreateCodeAsync(Users user)
    {
        var code = Random.Shared.Next(100000, 999999).ToString();
        var expiresAt = DateTime.UtcNow.AddMinutes(10);

        var existing = _dbContext.UserMfaCodes.Where(x => x.U_Email == user.U_Email && !x.Is_Used);
        await existing.ExecuteUpdateAsync(setters => setters.SetProperty(x => x.Is_Used, true));

        _dbContext.UserMfaCodes.Add(new UserMfaCode
        {
            U_Id = user.U_Id,
            U_Email = user.U_Email,
            Code_Hash = PasswordHasher.HashToken(code),
            Expires_At = expiresAt,
            Created_At = DateTime.UtcNow,
            Is_Used = false
        });

        await _dbContext.SaveChangesAsync();
        await _mfaDeliveryService.DeliverCodeAsync(user, code, expiresAt);
        return (code, expiresAt);
    }

    public async Task<bool> VerifyCodeAsync(string email, string code)
    {
        var hash = PasswordHasher.HashToken(code.Trim());
        var match = await _dbContext.UserMfaCodes
            .Where(x => x.U_Email == email && x.Code_Hash == hash && !x.Is_Used && x.Expires_At >= DateTime.UtcNow)
            .OrderByDescending(x => x.Created_At)
            .FirstOrDefaultAsync();

        if (match is null)
        {
            return false;
        }

        match.Is_Used = true;
        await _dbContext.SaveChangesAsync();
        return true;
    }
}
