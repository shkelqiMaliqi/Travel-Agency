using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using Travel_Agency_Portal.Models;

namespace Travel_Agency_Portal.Services;

public class JwtTokenService
{
    private readonly IConfiguration _configuration;

    public JwtTokenService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public AuthResponse CreateToken(Users user)
    {
        var secret = _configuration["Jwt:Secret"] ?? throw new InvalidOperationException("Missing Jwt:Secret configuration.");
        var issuer = _configuration["Jwt:Issuer"] ?? "TravelAgency.Api";
        var audience = _configuration["Jwt:Audience"] ?? "TravelAgency.Client";
        var expiresAt = DateTime.UtcNow.AddHours(4);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.U_Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.U_Email),
            new Claim(ClaimTypes.Name, $"{user.U_Name} {user.U_Surname}".Trim()),
            new Claim(ClaimTypes.NameIdentifier, user.U_Id.ToString()),
            new Claim(ClaimTypes.Role, user.U_Type),
            new Claim("username", user.U_Username)
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: expiresAt,
            signingCredentials: credentials);

        return new AuthResponse
        {
            Token = new JwtSecurityTokenHandler().WriteToken(token),
            ExpiresAtUtc = expiresAt,
            Role = user.U_Type,
            UserId = user.U_Id,
            Name = $"{user.U_Name} {user.U_Surname}".Trim(),
            Email = user.U_Email
        };
    }
}
