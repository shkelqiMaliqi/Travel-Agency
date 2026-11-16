using System.Security.Cryptography;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace Travel_Agency_Portal.Services;

public sealed class JwtSigningKey
{
    public SymmetricSecurityKey Key { get; }

    public JwtSigningKey(IConfiguration configuration, IHostEnvironment environment, ILogger<JwtSigningKey> logger)
    {
        var secret = configuration["Jwt:Secret"];
        if (string.IsNullOrWhiteSpace(secret) && environment.IsDevelopment())
        {
            Key = new SymmetricSecurityKey(RandomNumberGenerator.GetBytes(64));
            logger.LogWarning("Using an ephemeral development JWT key. Tokens expire on restart. Set Jwt__Secret to share a key across replicas.");
            return;
        }

        if (string.IsNullOrWhiteSpace(secret) || Encoding.UTF8.GetByteCount(secret) < 32 ||
            secret.Contains("ChangeMe", StringComparison.OrdinalIgnoreCase) ||
            secret.Contains("replace-with", StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("Configure Jwt__Secret with at least 32 bytes of randomly generated secret material. Do not use example secrets.");

        Key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
    }
}
