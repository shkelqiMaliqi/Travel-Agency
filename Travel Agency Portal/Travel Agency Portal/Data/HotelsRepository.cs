using System.Data;
using System.Data.SqlClient;
using Travel_Agency_Portal.Models;

namespace Travel_Agency_Portal.Data;

public class HotelsRepository(IConfiguration configuration)
{
    private readonly IConfiguration _configuration = configuration;

    private List<Hotel> ExecuteHotelsQuery(string query, params SqlParameter[] parameters)
    {
        var hotels = new List<Hotel>();
        using var connection = new SqlConnection(_configuration.GetConnectionString("CRUDCS"));
        using var command = new SqlCommand(query, connection);
        command.Parameters.AddRange(parameters);

        connection.Open();
        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            hotels.Add(new Hotel
            {
                Hotel_Id = reader.GetInt32(reader.GetOrdinal("Hotel_Id")),
                Place_Id = reader.GetInt32(reader.GetOrdinal("Place_Id")),
                Place_Name = reader.GetString(reader.GetOrdinal("Place_Name")),
                Hotel_Name = reader.GetString(reader.GetOrdinal("Hotel_Name")),
                Hotel_Description = reader.GetString(reader.GetOrdinal("Hotel_Description")),
                Hotel_Stars = reader.GetInt32(reader.GetOrdinal("Hotel_Stars")),
                Hotel_Url = reader.IsDBNull(reader.GetOrdinal("Hotel_Url")) ? null : reader.GetString(reader.GetOrdinal("Hotel_Url"))
            });
        }

        return hotels;
    }

    private int ExecuteNonQuery(string query, params SqlParameter[] parameters)
    {
        using var connection = new SqlConnection(_configuration.GetConnectionString("CRUDCS"));
        using var command = new SqlCommand(query, connection);
        command.Parameters.AddRange(parameters);

        connection.Open();
        return command.ExecuteNonQuery();
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

    public int CountPackages(int id)
    {
        const string usageQuery = SqlUsageQuery4;
        return ExecuteScalar(usageQuery, new SqlParameter("@Hotel_Id", id));
    }

    public int Delete(int id)
    {
        const string query = SqlQuery5;
        return ExecuteNonQuery(query, new SqlParameter("@Hotel_Id", id));
    }

    public int Update(int id, Hotel hotel)
    {
        const string query = SqlQuery3;
        return ExecuteNonQuery(query,
            new SqlParameter("@Hotel_Id", id),
            new SqlParameter("@Place_Id", hotel.Place_Id),
            new SqlParameter("@Hotel_Name", hotel.Hotel_Name),
            new SqlParameter("@Hotel_Description", hotel.Hotel_Description),
            new SqlParameter("@Hotel_Stars", hotel.Hotel_Stars),
            new SqlParameter("@Hotel_Url", (object?)hotel.Hotel_Url ?? DBNull.Value));
    }

    public int Create(Hotel hotel)
    {
        const string query = SqlQuery2;
        return ExecuteScalar(query,
            new SqlParameter("@Place_Id", hotel.Place_Id),
            new SqlParameter("@Hotel_Name", hotel.Hotel_Name),
            new SqlParameter("@Hotel_Description", hotel.Hotel_Description),
            new SqlParameter("@Hotel_Stars", hotel.Hotel_Stars),
            new SqlParameter("@Hotel_Url", (object?)hotel.Hotel_Url ?? DBNull.Value));
    }

    public List<Hotel> List(int? placeId)
    {
        var query = SqlQuery1;

        var parameters = new List<SqlParameter>();
        if (placeId.HasValue)
        {
            query += " WHERE h.Place_Id = @Place_Id";
            parameters.Add(new SqlParameter("@Place_Id", placeId.Value));
        }

        query += " ORDER BY h.Hotel_Id DESC";
        return ExecuteHotelsQuery(query, parameters.ToArray());
    }

    private const string SqlQuery1 = @"
            SELECT h.Hotel_Id, h.Place_Id, p.Place_Name, h.Hotel_Name, h.Hotel_Description, h.Hotel_Stars, h.Hotel_Url
            FROM dbo.Hotels h
            INNER JOIN dbo.Places p ON p.Place_Id = h.Place_Id";
    private const string SqlQuery2 = @"
            INSERT INTO dbo.Hotels (Place_Id, Hotel_Name, Hotel_Description, Hotel_Stars, Hotel_Url)
            OUTPUT INSERTED.Hotel_Id
            VALUES (@Place_Id, @Hotel_Name, @Hotel_Description, @Hotel_Stars, @Hotel_Url)";
    private const string SqlQuery3 = @"
            UPDATE dbo.Hotels
            SET Place_Id = @Place_Id,
                Hotel_Name = @Hotel_Name,
                Hotel_Description = @Hotel_Description,
                Hotel_Stars = @Hotel_Stars,
                Hotel_Url = @Hotel_Url
            WHERE Hotel_Id = @Hotel_Id";
    private const string SqlUsageQuery4 = "SELECT COUNT(1) FROM dbo.Travel_Packages WHERE Hotel_Id = @Hotel_Id";
    private const string SqlQuery5 = "DELETE FROM dbo.Hotels WHERE Hotel_Id = @Hotel_Id";
}
