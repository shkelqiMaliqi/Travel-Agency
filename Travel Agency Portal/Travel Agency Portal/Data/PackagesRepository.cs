using Travel_Agency_Portal.Dtos;
using System.Data;
using System.Data.SqlClient;
using Travel_Agency_Portal.Models;

namespace Travel_Agency_Portal.Data;

public class PackagesRepository(IConfiguration configuration)
{
    private readonly IConfiguration _configuration = configuration;

    private List<TravelPackage> ExecutePackagesQuery(string query, params SqlParameter[] parameters)
    {
        var packages = new List<TravelPackage>();
        using var connection = new SqlConnection(_configuration.GetConnectionString("CRUDCS"));
        using var command = new SqlCommand(query, connection);
        command.Parameters.AddRange(parameters);

        connection.Open();
        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            packages.Add(new TravelPackage
            {
                Package_Id = reader.GetInt32(reader.GetOrdinal("Package_Id")),
                Place_Id = reader.GetInt32(reader.GetOrdinal("Place_Id")),
                Hotel_Id = reader.GetInt32(reader.GetOrdinal("Hotel_Id")),
                Place_Name = reader.GetString(reader.GetOrdinal("Place_Name")),
                Place_Description = reader.GetString(reader.GetOrdinal("Place_Description")),
                Hotel_Name = reader.GetString(reader.GetOrdinal("Hotel_Name")),
                Hotel_Description = reader.GetString(reader.GetOrdinal("Hotel_Description")),
                Hotel_Stars = reader.GetInt32(reader.GetOrdinal("Hotel_Stars")),
                Package_Name = reader.GetString(reader.GetOrdinal("Package_Name")),
                Package_Description = reader.GetString(reader.GetOrdinal("Package_Description")),
                Price_Per_Person = reader.GetDecimal(reader.GetOrdinal("Price_Per_Person")),
                Start_Date = reader.GetDateTime(reader.GetOrdinal("Start_Date")),
                End_Date = reader.GetDateTime(reader.GetOrdinal("End_Date")),
                Available_Seats = reader.GetInt32(reader.GetOrdinal("Available_Seats")),
                Package_Url = reader.IsDBNull(reader.GetOrdinal("Package_Url")) ? null : reader.GetString(reader.GetOrdinal("Package_Url"))
            });
        }

        return packages;
    }

    private int ExecuteNonQuery(string query, params SqlParameter[] parameters)
    {
        using var connection = new SqlConnection(_configuration.GetConnectionString("CRUDCS"));
        using var command = new SqlCommand(query, connection);
        command.Parameters.AddRange(parameters);

        connection.Open();
        return command.ExecuteNonQuery();
    }

    public bool HotelBelongsToPlace(int hotelId, int placeId)
    {
        const string query = "SELECT COUNT(1) FROM dbo.Hotels WHERE Hotel_Id = @Hotel_Id AND Place_Id = @Place_Id";
        return ExecuteScalar(query, new SqlParameter("@Hotel_Id", hotelId), new SqlParameter("@Place_Id", placeId)) > 0;
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

    public int CountBookings(int id)
    {
        const string usageQuery = SqlUsageQuery5;
        return ExecuteScalar(usageQuery, new SqlParameter("@Package_Id", id));
    }

    public int Delete(int id)
    {
        const string query = SqlQuery6;
        return ExecuteNonQuery(query, new SqlParameter("@Package_Id", id));
    }

    public int Update(int id, PackageRequestDto package)
    {
        const string query = SqlQuery4;
        return ExecuteNonQuery(query,
            new SqlParameter("@Package_Id", id),
            new SqlParameter("@Place_Id", package.Place_Id),
            new SqlParameter("@Hotel_Id", package.Hotel_Id),
            new SqlParameter("@Package_Name", package.Package_Name),
            new SqlParameter("@Package_Description", package.Package_Description),
            new SqlParameter("@Price_Per_Person", package.Price_Per_Person),
            new SqlParameter("@Start_Date", package.Start_Date.Date),
            new SqlParameter("@End_Date", package.End_Date.Date),
            new SqlParameter("@Available_Seats", package.Available_Seats),
            new SqlParameter("@Package_Url", (object?)package.Package_Url ?? DBNull.Value));
    }

    public int Create(PackageRequestDto package)
    {
        const string query = SqlQuery3;
        return ExecuteScalar(query,
            new SqlParameter("@Place_Id", package.Place_Id),
            new SqlParameter("@Hotel_Id", package.Hotel_Id),
            new SqlParameter("@Package_Name", package.Package_Name),
            new SqlParameter("@Package_Description", package.Package_Description),
            new SqlParameter("@Price_Per_Person", package.Price_Per_Person),
            new SqlParameter("@Start_Date", package.Start_Date.Date),
            new SqlParameter("@End_Date", package.End_Date.Date),
            new SqlParameter("@Available_Seats", package.Available_Seats),
            new SqlParameter("@Package_Url", (object?)package.Package_Url ?? DBNull.Value));
    }

    public List<TravelPackage> Find(int id)
    {
        const string query = SqlQuery2;
        return ExecutePackagesQuery(query, new SqlParameter("@Package_Id", id));
    }

    public List<TravelPackage> List(string? search, int? placeId, decimal? minPrice, decimal? maxPrice)
    {
        var query = SqlQuery1;

        var filters = new List<string>();
        var parameters = new List<SqlParameter>();

        if (!string.IsNullOrWhiteSpace(search))
        {
            filters.Add("(tp.Package_Name LIKE @Search OR tp.Package_Description LIKE @Search OR p.Place_Name LIKE @Search OR h.Hotel_Name LIKE @Search)");
            parameters.Add(new SqlParameter("@Search", $"%{search.Trim()}%"));
        }

        if (placeId.HasValue)
        {
            filters.Add("tp.Place_Id = @Place_Id");
            parameters.Add(new SqlParameter("@Place_Id", placeId.Value));
        }

        if (minPrice.HasValue)
        {
            filters.Add("tp.Price_Per_Person >= @MinPrice");
            parameters.Add(new SqlParameter("@MinPrice", minPrice.Value));
        }

        if (maxPrice.HasValue)
        {
            filters.Add("tp.Price_Per_Person <= @MaxPrice");
            parameters.Add(new SqlParameter("@MaxPrice", maxPrice.Value));
        }

        if (filters.Count > 0)
        {
            query += " WHERE " + string.Join(" AND ", filters);
        }

        query += " ORDER BY tp.Package_Name ASC, p.Place_Name ASC";
        return ExecutePackagesQuery(query, parameters.ToArray());
    }

    private const string SqlQuery1 = @"
            SELECT tp.Package_Id, tp.Place_Id, tp.Hotel_Id,
                   p.Place_Name, p.Place_Description,
                   h.Hotel_Name, h.Hotel_Description, h.Hotel_Stars,
                   tp.Package_Name, tp.Package_Description, tp.Price_Per_Person,
                   tp.Start_Date, tp.End_Date, tp.Available_Seats, tp.Package_Url
            FROM dbo.Travel_Packages tp
            INNER JOIN dbo.Places p ON p.Place_Id = tp.Place_Id
            INNER JOIN dbo.Hotels h ON h.Hotel_Id = tp.Hotel_Id";
    private const string SqlQuery2 = @"
            SELECT tp.Package_Id, tp.Place_Id, tp.Hotel_Id,
                   p.Place_Name, p.Place_Description,
                   h.Hotel_Name, h.Hotel_Description, h.Hotel_Stars,
                   tp.Package_Name, tp.Package_Description, tp.Price_Per_Person,
                   tp.Start_Date, tp.End_Date, tp.Available_Seats, tp.Package_Url
            FROM dbo.Travel_Packages tp
            INNER JOIN dbo.Places p ON p.Place_Id = tp.Place_Id
            INNER JOIN dbo.Hotels h ON h.Hotel_Id = tp.Hotel_Id
            WHERE tp.Package_Id = @Package_Id";
    private const string SqlQuery3 = @"
            INSERT INTO dbo.Travel_Packages
                (Place_Id, Hotel_Id, Package_Name, Package_Description, Price_Per_Person, Start_Date, End_Date, Available_Seats, Package_Url)
            OUTPUT INSERTED.Package_Id
            VALUES
                (@Place_Id, @Hotel_Id, @Package_Name, @Package_Description, @Price_Per_Person, @Start_Date, @End_Date, @Available_Seats, @Package_Url)";
    private const string SqlQuery4 = @"
            UPDATE dbo.Travel_Packages
            SET Place_Id = @Place_Id,
                Hotel_Id = @Hotel_Id,
                Package_Name = @Package_Name,
                Package_Description = @Package_Description,
                Price_Per_Person = @Price_Per_Person,
                Start_Date = @Start_Date,
                End_Date = @End_Date,
                Available_Seats = @Available_Seats,
                Package_Url = @Package_Url
            WHERE Package_Id = @Package_Id";
    private const string SqlUsageQuery5 = "SELECT COUNT(1) FROM dbo.Bookings WHERE Package_Id = @Package_Id";
    private const string SqlQuery6 = "DELETE FROM dbo.Travel_Packages WHERE Package_Id = @Package_Id";
}
