using System.Data;
using System.Data.SqlClient;
using Travel_Agency_Portal.Models;

namespace Travel_Agency_Portal.Data;

public class UsersRepository(IConfiguration configuration)
{
    private readonly IConfiguration _configuration = configuration;

    private List<UserProfile> ExecuteUsersQuery(string query, params SqlParameter[] parameters)
    {
        var users = new List<UserProfile>();
        using var connection = new SqlConnection(_configuration.GetConnectionString("CRUDCS"));
        using var command = new SqlCommand(query, connection);

        if (parameters.Length > 0)
        {
            command.Parameters.AddRange(parameters);
        }

        connection.Open();
        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            users.Add(new UserProfile
            {
                U_Id = reader.GetInt32(reader.GetOrdinal("U_Id")),
                U_Name = reader.GetString(reader.GetOrdinal("U_Name")),
                U_Surname = reader.GetString(reader.GetOrdinal("U_Surname")),
                U_Email = reader.GetString(reader.GetOrdinal("U_Email")),
                U_Username = reader.GetString(reader.GetOrdinal("U_Username")),
                U_Phone = reader.IsDBNull(reader.GetOrdinal("U_Phone")) ? null : reader.GetString(reader.GetOrdinal("U_Phone")),
                U_Type = reader.GetString(reader.GetOrdinal("U_Type"))
            });
        }

        return users;
    }

    private int ExecuteNonQuery(string query, params SqlParameter[] parameters)
    {
        using var connection = new SqlConnection(_configuration.GetConnectionString("CRUDCS"));
        using var command = new SqlCommand(query, connection);

        if (parameters.Length > 0)
        {
            command.Parameters.AddRange(parameters);
        }

        connection.Open();
        return command.ExecuteNonQuery();
    }

    private int ExecuteScalar(string query, params SqlParameter[] parameters)
    {
        using var connection = new SqlConnection(_configuration.GetConnectionString("CRUDCS"));
        using var command = new SqlCommand(query, connection);

        if (parameters.Length > 0)
        {
            command.Parameters.AddRange(parameters);
        }

        connection.Open();
        var result = command.ExecuteScalar();

        return result is null || result == DBNull.Value ? 0 : Convert.ToInt32(result);
    }

    private string ExecuteScalarString(string query, params SqlParameter[] parameters)
    {
        using var connection = new SqlConnection(_configuration.GetConnectionString("CRUDCS"));
        using var command = new SqlCommand(query, connection);

        if (parameters.Length > 0)
        {
            command.Parameters.AddRange(parameters);
        }

        connection.Open();
        var result = command.ExecuteScalar();

        return result is null || result == DBNull.Value ? string.Empty : result.ToString() ?? string.Empty;
    }

    public int UpdateRole(int id, UpdateUserRoleRequest request)
    {
        const string query = "UPDATE dbo.Users SET U_Type = @U_Type WHERE U_Id = @U_Id";
        return ExecuteNonQuery(query,
            new SqlParameter("@U_Id", id),
            new SqlParameter("@U_Type", request.U_Type));
    }

    public int CountDependencies(int id)
    {
        const string usageQuery = @"
            SELECT
                (SELECT COUNT(1) FROM dbo.Bookings WHERE U_Id = @U_Id) +
                (SELECT COUNT(1) FROM dbo.Contact_Form WHERE U_Id = @U_Id)";
        return ExecuteScalar(usageQuery, new SqlParameter("@U_Id", id));
    }

    public int Delete(int id)
    {
        const string query = "DELETE FROM dbo.Users WHERE U_Id = @U_Id";
        return ExecuteNonQuery(query, new SqlParameter("@U_Id", id));
    }

    public string GetPasswordHash(int id, ChangePasswordRequest request)
    {
        const string passwordQuery = "SELECT U_Password FROM dbo.Users WHERE U_Id = @U_Id";
        return ExecuteScalarString(passwordQuery, new SqlParameter("@U_Id", id));
    }

    public int SetPasswordHash(int id, ChangePasswordRequest request, string nextHash)
    {
        const string updateQuery = @"
            UPDATE dbo.Users
            SET U_Password = @U_Password,
                U_RepeatPassword = @U_Password
            WHERE U_Id = @U_Id";
        return ExecuteNonQuery(updateQuery,
            new SqlParameter("@U_Id", id),
            new SqlParameter("@U_Password", nextHash));
    }

    public int CountConflicts(int id, UpdateUserProfileRequest user)
    {
        const string existsQuery = @"
            SELECT COUNT(1)
            FROM dbo.Users
            WHERE U_Id <> @U_Id AND (U_Email = @U_Email OR U_Username = @U_Username)";
        return ExecuteScalar(existsQuery,
            new SqlParameter("@U_Id", id),
            new SqlParameter("@U_Email", user.U_Email),
            new SqlParameter("@U_Username", user.U_Username));
    }

    public int Update(int id, UpdateUserProfileRequest user)
    {
        const string query = @"
            UPDATE dbo.Users
            SET U_Name = @U_Name,
                U_Surname = @U_Surname,
                U_Email = @U_Email,
                U_Username = @U_Username,
                U_Phone = @U_Phone
            WHERE U_Id = @U_Id";
        return ExecuteNonQuery(query,
            new SqlParameter("@U_Id", id),
            new SqlParameter("@U_Name", user.U_Name),
            new SqlParameter("@U_Surname", user.U_Surname),
            new SqlParameter("@U_Email", user.U_Email),
            new SqlParameter("@U_Username", user.U_Username),
            new SqlParameter("@U_Phone", string.IsNullOrWhiteSpace(user.U_Phone) ? DBNull.Value : user.U_Phone));
    }

    public List<UserProfile> Find(int id)
    {
        const string query = @"SELECT U_Id, U_Name, U_Surname, U_Email, U_Username, U_Phone, U_Type FROM dbo.Users WHERE U_Id = @U_Id";
        return ExecuteUsersQuery(query, new SqlParameter("@U_Id", id));
    }

    public List<UserProfile> List()
    {
        const string query = @"SELECT U_Id, U_Name, U_Surname, U_Email, U_Username, U_Phone, U_Type FROM dbo.Users ORDER BY U_Id DESC";
        return ExecuteUsersQuery(query);
    }
}
