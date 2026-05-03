using System.Data;
using System.Data.SqlClient;

namespace Travel_Agency_Portal.Services;

public class DatabaseService
{
    private readonly string _connectionString;

    public DatabaseService(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("CRUDCS")
            ?? throw new InvalidOperationException("Missing CRUDCS connection string.");
    }

    public DataTable Query(string query, params SqlParameter[] parameters)
    {
        var table = new DataTable();
        using var connection = new SqlConnection(_connectionString);
        using var command = new SqlCommand(query, connection);
        command.Parameters.AddRange(parameters);

        connection.Open();
        using var adapter = new SqlDataAdapter(command);
        adapter.Fill(table);

        return table;
    }

    public int Execute(string query, params SqlParameter[] parameters)
    {
        using var connection = new SqlConnection(_connectionString);
        using var command = new SqlCommand(query, connection);
        command.Parameters.AddRange(parameters);

        connection.Open();
        return command.ExecuteNonQuery();
    }

    public T Scalar<T>(string query, params SqlParameter[] parameters)
    {
        using var connection = new SqlConnection(_connectionString);
        using var command = new SqlCommand(query, connection);
        command.Parameters.AddRange(parameters);

        connection.Open();
        var result = command.ExecuteScalar();

        if (result is null || result == DBNull.Value)
        {
            return default!;
        }

        return (T)Convert.ChangeType(result, typeof(T));
    }
}
