using System.Data;
using System.Data.SqlClient;

namespace Travel_Agency_Portal.Data;

public interface IPasswordResetRepository
{
    Task<bool> CreateAsync(string email, string tokenHash, DateTime expiresAt);
    Task<bool> ConsumeAsync(string email, string tokenHash, string passwordHash);
}

public sealed class PasswordResetRepository(IConfiguration configuration) : IPasswordResetRepository
{
    private SqlConnection CreateConnection() => new(configuration.GetConnectionString("CRUDCS"));

    public async Task<bool> CreateAsync(string email, string tokenHash, DateTime expiresAt)
    {
        using var connection = CreateConnection();
        await connection.OpenAsync();
        using var transaction = connection.BeginTransaction();
        // The user row serializes concurrent issue/reset attempts for the same account.
        using var command = new SqlCommand("""
            IF NOT EXISTS (SELECT 1 FROM dbo.Users WITH (UPDLOCK, HOLDLOCK) WHERE U_Email = @Email)
                SELECT 0;
            ELSE BEGIN
                UPDATE dbo.Password_Reset_Codes SET Is_Used = 1 WHERE U_Email = @Email AND Is_Used = 0;
                INSERT INTO dbo.Password_Reset_Codes (U_Email, Reset_Code_Hash, Expires_At, Is_Used, Created_At)
                VALUES (@Email, @Hash, @Expires, 0, SYSUTCDATETIME());
                SELECT 1;
            END
            """, connection, transaction);
        command.Parameters.Add("@Email", SqlDbType.VarChar, 255).Value = email;
        command.Parameters.Add("@Hash", SqlDbType.NVarChar, 64).Value = tokenHash;
        command.Parameters.Add("@Expires", SqlDbType.DateTime2).Value = expiresAt;
        var exists = Convert.ToInt32(await command.ExecuteScalarAsync()) == 1;
        transaction.Commit();
        return exists;
    }

    public async Task<bool> ConsumeAsync(string email, string tokenHash, string passwordHash)
    {
        using var connection = CreateConnection();
        await connection.OpenAsync();
        using var transaction = connection.BeginTransaction();
        using var command = new SqlCommand("""
            SELECT U_Id FROM dbo.Users WITH (UPDLOCK, HOLDLOCK) WHERE U_Email = @Email;
            """, connection, transaction);
        command.Parameters.Add("@Email", SqlDbType.VarChar, 255).Value = email;
        if (await command.ExecuteScalarAsync() is null)
        {
            transaction.Rollback();
            return false;
        }
        command.CommandText = """
            UPDATE dbo.Password_Reset_Codes
            SET Is_Used = 1
            WHERE U_Email = @Email AND Reset_Code_Hash = @Hash AND Is_Used = 0
              AND Expires_At >= SYSUTCDATETIME();
            """;
        command.Parameters.Add("@Hash", SqlDbType.NVarChar, 64).Value = tokenHash;
        if (await command.ExecuteNonQueryAsync() == 0)
        {
            transaction.Rollback();
            return false;
        }
        command.CommandText = """
            UPDATE dbo.Users SET U_Password = @Password, U_RepeatPassword = @Password WHERE U_Email = @Email;
            UPDATE dbo.Password_Reset_Codes SET Is_Used = 1 WHERE U_Email = @Email AND Is_Used = 0;
            """;
        command.Parameters.Add("@Password", SqlDbType.NVarChar, 500).Value = passwordHash;
        await command.ExecuteNonQueryAsync();
        transaction.Commit();
        return true;
    }
}
