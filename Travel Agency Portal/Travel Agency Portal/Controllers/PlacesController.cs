using System.Data.SqlClient;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Travel_Agency_Portal.Models;

namespace Travel_Agency_Portal.Controllers;

[Route("api/v1/[controller]")]
[ApiController]
public class PlacesController : ControllerBase
{
    private readonly IConfiguration _configuration;
    private readonly IMemoryCache _cache;
    private readonly IDistributedCache? _distributedCache;

    public PlacesController(IConfiguration configuration, IMemoryCache cache, IDistributedCache? distributedCache = null)
    {
        _configuration = configuration;
        _cache = cache;
        _distributedCache = distributedCache;
    }

    [AllowAnonymous]
    [HttpGet]
    [ResponseCache(Duration = 60, Location = ResponseCacheLocation.Any, VaryByQueryKeys = new[] { "search" })]
    public IActionResult GetPlaces([FromQuery] string? search = null)
    {
        var cacheKey = $"places:{search?.Trim().ToLowerInvariant()}";
        if (_cache.TryGetValue(cacheKey, out List<Place>? cachedPlaces))
        {
            return Ok(cachedPlaces);
        }

        if (_distributedCache is not null)
        {
            var distributedJson = _distributedCache.GetString(cacheKey);
            if (!string.IsNullOrWhiteSpace(distributedJson))
            {
                var distributedPlaces = JsonSerializer.Deserialize<List<Place>>(distributedJson);
                if (distributedPlaces is not null)
                {
                    _cache.Set(cacheKey, distributedPlaces, TimeSpan.FromMinutes(2));
                    return Ok(distributedPlaces);
                }
            }
        }

        var query = "SELECT Place_Id, Place_Name, Place_Description, Place_Url FROM dbo.Places";
        var parameters = new List<SqlParameter>();

        if (!string.IsNullOrWhiteSpace(search))
        {
            query += " WHERE Place_Name LIKE @Search";
            parameters.Add(new SqlParameter("@Search", $"{search.Trim()}%"));
        }

        query += " ORDER BY Place_Name ASC";
        var places = ExecutePlacesQuery(query, parameters.ToArray());
        _cache.Set(cacheKey, places, TimeSpan.FromMinutes(5));
        _distributedCache?.SetString(cacheKey, JsonSerializer.Serialize(places), new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10)
        });

        return Ok(places);
    }

    [AllowAnonymous]
    [HttpGet("{id:int}")]
    [ResponseCache(Duration = 60, Location = ResponseCacheLocation.Any)]
    public IActionResult GetPlace(int id)
    {
        var cacheKey = $"places:id:{id}";
        if (_cache.TryGetValue(cacheKey, out Place? cachedPlace))
        {
            return Ok(cachedPlace);
        }

        if (_distributedCache is not null)
        {
            var distributedJson = _distributedCache.GetString(cacheKey);
            if (!string.IsNullOrWhiteSpace(distributedJson))
            {
                var distributedPlace = JsonSerializer.Deserialize<Place>(distributedJson);
                if (distributedPlace is not null)
                {
                    _cache.Set(cacheKey, distributedPlace, TimeSpan.FromMinutes(2));
                    return Ok(distributedPlace);
                }
            }
        }

        const string query = "SELECT Place_Id, Place_Name, Place_Description, Place_Url FROM dbo.Places WHERE Place_Id = @Place_Id";
        var result = ExecutePlacesQuery(query, new SqlParameter("@Place_Id", id));

        if (result.Count == 0)
        {
            return NotFound();
        }

        _cache.Set(cacheKey, result[0], TimeSpan.FromMinutes(5));
        _distributedCache?.SetString(cacheKey, JsonSerializer.Serialize(result[0]), new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10)
        });
        return Ok(result[0]);
    }

    [Authorize(Roles = "admin")]
    [HttpPost]
    public IActionResult AddPlace([FromBody] Place place)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        const string query = @"
            INSERT INTO dbo.Places (Place_Name, Place_Description, Place_Url)
            VALUES (@Place_Name, @Place_Description, @Place_Url)";

        ExecuteNonQuery(query,
            new SqlParameter("@Place_Name", place.Place_Name),
            new SqlParameter("@Place_Description", place.Place_Description),
            new SqlParameter("@Place_Url", (object?)place.Place_Url ?? DBNull.Value));

        _cache.Remove("places:");
        _distributedCache?.Remove("places:");
        return Ok(new { message = "Place added successfully." });
    }

    [Authorize(Roles = "admin")]
    [HttpPut("{id:int}")]
    public IActionResult UpdatePlace(int id, [FromBody] Place place)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        const string query = @"
            UPDATE dbo.Places
            SET Place_Name = @Place_Name,
                Place_Description = @Place_Description,
                Place_Url = @Place_Url
            WHERE Place_Id = @Place_Id";

        var rows = ExecuteNonQuery(query,
            new SqlParameter("@Place_Id", id),
            new SqlParameter("@Place_Name", place.Place_Name),
            new SqlParameter("@Place_Description", place.Place_Description),
            new SqlParameter("@Place_Url", (object?)place.Place_Url ?? DBNull.Value));

        _cache.Remove("places:");
        _cache.Remove($"places:id:{id}");
        _distributedCache?.Remove("places:");
        _distributedCache?.Remove($"places:id:{id}");
        return rows > 0 ? Ok(new { message = "Place updated successfully." }) : NotFound();
    }

    [Authorize(Roles = "admin")]
    [HttpDelete("{id:int}")]
    public IActionResult DeletePlace(int id)
    {
        const string usageQuery = @"
            SELECT
                (SELECT COUNT(1) FROM dbo.Hotels WHERE Place_Id = @Place_Id) +
                (SELECT COUNT(1) FROM dbo.Travel_Packages WHERE Place_Id = @Place_Id)";

        if (ExecuteScalar(usageQuery, new SqlParameter("@Place_Id", id)) > 0)
        {
            return Conflict(new { message = "This destination has hotels or packages. Delete those first." });
        }

        const string query = "DELETE FROM dbo.Places WHERE Place_Id = @Place_Id";
        var rows = ExecuteNonQuery(query, new SqlParameter("@Place_Id", id));

        _cache.Remove("places:");
        _cache.Remove($"places:id:{id}");
        _distributedCache?.Remove("places:");
        _distributedCache?.Remove($"places:id:{id}");
        return rows > 0 ? Ok(new { message = "Place deleted successfully." }) : NotFound();
    }

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
}
