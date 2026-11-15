using System.Data;
using System.Data.SqlClient;
using Travel_Agency_Portal.Models;

namespace Travel_Agency_Portal.Data;

public class AuthRepository(IConfiguration configuration)
{
    private readonly IConfiguration _configuration = configuration;

    private DataTable ExecuteQuery(string query, params SqlParameter[] parameters)
    {
        var table = new DataTable();
        using var connection = new SqlConnection(_configuration.GetConnectionString("CRUDCS"));
        using var command = new SqlCommand(query, connection);
        command.Parameters.AddRange(parameters);

        connection.Open();
        using var adapter = new SqlDataAdapter(command);
        adapter.Fill(table);

        return table;
    }

    private int ExecuteScalar(string query, params SqlParameter[] parameters)
    {
        using var connection = new SqlConnection(_configuration.GetConnectionString("CRUDCS"));
        using var command = new SqlCommand(query, connection);
        command.Parameters.AddRange(parameters);

        connection.Open();
        var result = command.ExecuteScalar();

        return result is null || result == DBNull.Value ? 0 : Convert.ToInt32(result);
    }

    private int ExecuteNonQuery(string query, params SqlParameter[] parameters)
    {
        using var connection = new SqlConnection(_configuration.GetConnectionString("CRUDCS"));
        using var command = new SqlCommand(query, connection);
        command.Parameters.AddRange(parameters);

        connection.Open();
        return command.ExecuteNonQuery();
    }

    public Users? FindForMfa(MfaVerifyRequest request)
    {
        const string query = SqlQuery4;
        return MapUser(ExecuteQuery(query, new SqlParameter("@U_Email", request.Email)));
    }

    public Users? FindForLogin(LoginRequest request)
    {
        const string query = SqlQuery3;
        return MapUser(ExecuteQuery(query, new SqlParameter("@UserNameOrEmail", request.UserNameOrEmail)));
    }

    public int CountConflicts(RegisterRequest request)
    {
        const string existsQuery = SqlExistsQuery1;
        return ExecuteScalar(existsQuery,
            new SqlParameter("@U_Email", request.U_Email),
            new SqlParameter("@U_Username", request.U_Username));
    }

    public int Create(RegisterRequest request, string hashedPassword)
    {
        const string publicRegistrationRole = "user";
        const string insertQuery = SqlInsertQuery2;

        return ExecuteScalar(insertQuery,
            new SqlParameter("@U_Name", request.U_Name),
            new SqlParameter("@U_Surname", request.U_Surname),
            new SqlParameter("@U_Email", request.U_Email),
            new SqlParameter("@U_Username", request.U_Username),
            new SqlParameter("@U_Phone", (object?)request.U_Phone ?? DBNull.Value),
            new SqlParameter("@U_Password", hashedPassword),
            new SqlParameter("@U_RepeatPassword", hashedPassword),
            new SqlParameter("@U_Type", publicRegistrationRole));

    }

    private static Users? MapUser(DataTable table)
    {
        if (table.Rows.Count == 0) return null;
        var row = table.Rows[0];
        return new Users
        {
            U_Id = Convert.ToInt32(row["U_Id"]),
            U_Name = row["U_Name"].ToString() ?? "",
            U_Surname = row["U_Surname"].ToString() ?? "",
            U_Email = row["U_Email"].ToString() ?? "",
            U_Username = row["U_Username"].ToString() ?? "",
            U_Phone = row["U_Phone"] == DBNull.Value ? null : row["U_Phone"].ToString(),
            U_Type = row["U_Type"].ToString() ?? "user",
            U_Password = table.Columns.Contains("U_Password") ? row["U_Password"].ToString() ?? "" : ""
        };
    }

    private const string SqlExistsQuery1 = "SELECT COUNT(1) FROM dbo.Users WHERE U_Email = @U_Email OR U_Username = @U_Username";
    private const string SqlInsertQuery2 = @"
            INSERT INTO dbo.Users (U_Name, U_Surname, U_Email, U_Username, U_Phone, U_Password, U_RepeatPassword, U_Type)
            OUTPUT INSERTED.U_Id
            VALUES (@U_Name, @U_Surname, @U_Email, @U_Username, @U_Phone, @U_Password, @U_RepeatPassword, @U_Type)";
    private const string SqlQuery3 = @"
            SELECT TOP 1 U_Id, U_Name, U_Surname, U_Email, U_Username, U_Phone, U_Password, U_RepeatPassword, U_Type
            FROM dbo.Users
            WHERE U_Email = @UserNameOrEmail OR U_Username = @UserNameOrEmail";
    private const string SqlQuery4 = @"
            SELECT TOP 1 U_Id, U_Name, U_Surname, U_Email, U_Username, U_Phone, U_Type
            FROM dbo.Users
            WHERE U_Email = @U_Email";
}
