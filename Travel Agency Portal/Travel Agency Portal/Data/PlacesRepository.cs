using System.Data;
using System.Data.SqlClient;
using Travel_Agency_Portal.Models;

namespace Travel_Agency_Portal.Data;

public class PlacesRepository(IConfiguration configuration)
{
    private readonly IConfiguration _configuration = configuration;

    private List<Place> ExecutePlacesQuery(string query, params SqlParameter[] parameters)
    {
        var places = new List<Place>();
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
            places.Add(new Place
            {
                Place_Id = reader.GetInt32(reader.GetOrdinal("Place_Id")),
                Place_Name = reader.GetString(reader.GetOrdinal("Place_Name")),
                Place_Description = reader.GetString(reader.GetOrdinal("Place_Description")),
                Place_Url = reader.IsDBNull(reader.GetOrdinal("Place_Url"))
                    ? null
                    : reader.GetString(reader.GetOrdinal("Place_Url"))
            });
        }

        return places;
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

    public int CountDependencies(int id)
    {
        const string usageQuery = SqlUsageQuery5;
        return ExecuteScalar(usageQuery, new SqlParameter("@Place_Id", id));
    }

    public int Delete(int id)
    {
        const string query = SqlQuery6;
        return ExecuteNonQuery(query, new SqlParameter("@Place_Id", id));
    }

    public int Update(int id, Place place)
    {
        const string query = SqlQuery4;
        return ExecuteNonQuery(query,
            new SqlParameter("@Place_Id", id),
            new SqlParameter("@Place_Name", place.Place_Name),
            new SqlParameter("@Place_Description", place.Place_Description),
            new SqlParameter("@Place_Url", (object?)place.Place_Url ?? DBNull.Value));
    }

    public int Create(Place place)
    {
        const string query = SqlQuery3;
        return ExecuteScalar(query,
            new SqlParameter("@Place_Name", place.Place_Name),
            new SqlParameter("@Place_Description", place.Place_Description),
            new SqlParameter("@Place_Url", (object?)place.Place_Url ?? DBNull.Value));
    }

    public List<Place> Find(int id)
    {
        const string query = SqlQuery2;
        return ExecutePlacesQuery(query, new SqlParameter("@Place_Id", id));
    }

    public List<Place> List(string? search)
    {
        var query = SqlQuery1;
        var parameters = new List<SqlParameter>();

        if (!string.IsNullOrWhiteSpace(search))
        {
            query += " WHERE Place_Name LIKE @Search";
            parameters.Add(new SqlParameter("@Search", $"{search.Trim()}%"));
        }

        query += " ORDER BY Place_Name ASC";
        return ExecutePlacesQuery(query, parameters.ToArray());
    }

    private const string SqlQuery1 = "SELECT Place_Id, Place_Name, Place_Description, Place_Url FROM dbo.Places";
    private const string SqlQuery2 = "SELECT Place_Id, Place_Name, Place_Description, Place_Url FROM dbo.Places WHERE Place_Id = @Place_Id";
    private const string SqlQuery3 = @"
            INSERT INTO dbo.Places (Place_Name, Place_Description, Place_Url)
            OUTPUT INSERTED.Place_Id
            VALUES (@Place_Name, @Place_Description, @Place_Url)";
    private const string SqlQuery4 = @"
            UPDATE dbo.Places
            SET Place_Name = @Place_Name,
                Place_Description = @Place_Description,
                Place_Url = @Place_Url
            WHERE Place_Id = @Place_Id";
    private const string SqlUsageQuery5 = @"
            SELECT
                (SELECT COUNT(1) FROM dbo.Hotels WHERE Place_Id = @Place_Id) +
                (SELECT COUNT(1) FROM dbo.Travel_Packages WHERE Place_Id = @Place_Id)";
    private const string SqlQuery6 = "DELETE FROM dbo.Places WHERE Place_Id = @Place_Id";
}
