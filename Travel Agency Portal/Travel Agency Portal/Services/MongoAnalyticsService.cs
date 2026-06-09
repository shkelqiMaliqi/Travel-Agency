using MongoDB.Bson;
using MongoDB.Driver;
using Travel_Agency_Portal.Models;

namespace Travel_Agency_Portal.Services;

public class MongoAnalyticsService
{
    private readonly IConfiguration _configuration;

    public MongoAnalyticsService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    private IMongoCollection<BsonDocument>? GetCollection()
    {
        var connectionString = _configuration["Mongo:ConnectionString"];
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            return null;
        }

        var databaseName = _configuration["Mongo:Database"] ?? "travel_agency";
        var collectionName = _configuration["Mongo:AnalyticsCollection"] ?? "analytics_events";
        var client = new MongoClient(connectionString);
        return client.GetDatabase(databaseName).GetCollection<BsonDocument>(collectionName);
    }

    public async Task<bool> TrackAsync(string eventName, string? category, string? metadata)
    {
        var collection = GetCollection();
        if (collection is null)
        {
            return false;
        }

        await collection.InsertOneAsync(new BsonDocument
        {
            { "eventName", eventName },
            { "category", category ?? string.Empty },
            { "metadata", metadata ?? string.Empty },
            { "createdAt", DateTime.UtcNow }
        });

        return true;
    }

    public async Task<List<BsonDocument>> GetRecentAsync()
    {
        var collection = GetCollection();
        if (collection is null)
        {
            return new List<BsonDocument>();
        }

        return await collection.Find(FilterDefinition<BsonDocument>.Empty)
            .SortByDescending(x => x["createdAt"])
            .Limit(20)
            .ToListAsync();
    }
}
