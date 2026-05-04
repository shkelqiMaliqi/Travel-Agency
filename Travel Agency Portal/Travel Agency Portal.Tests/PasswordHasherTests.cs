using Travel_Agency_Portal.Services;

namespace Travel_Agency_Portal.Tests;

public class PasswordHasherTests
{
    [Fact]
    public void Hash_CreatesSaltedHashThatVerifies()
    {
        var hash = PasswordHasher.Hash("Admin123!");

        Assert.StartsWith("PBKDF2-SHA256$", hash);
        Assert.True(PasswordHasher.Verify("Admin123!", hash));
        Assert.False(PasswordHasher.Verify("Wrong123!", hash));
    }

    [Fact]
    public void Verify_AcceptsLegacySha256Hashes()
    {
        const string legacyHash = "3EB3FE66B31E3B4D10FA70B5CAD49C7112294AF6AE4E476A1C405155D45AA121";

        Assert.True(PasswordHasher.Verify("Admin123!", legacyHash));
        Assert.False(PasswordHasher.Verify("Wrong123!", legacyHash));
    }
}
