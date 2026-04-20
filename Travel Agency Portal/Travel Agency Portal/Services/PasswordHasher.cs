using System.Security.Cryptography;
using System.Text;

namespace Travel_Agency_Portal.Services;

public static class PasswordHasher
{
    public static string Hash(string value)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(value));
        return Convert.ToHexString(bytes);
    }
}
