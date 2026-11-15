using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;

namespace Travel_Agency_Portal.Services;

// Versioned keys invalidate every filter/detail across all API instances.
public sealed class CatalogCache(IDistributedCache cache, ILogger<CatalogCache> logger)
{
    private const string VersionKey = "catalog:version";

    public T GetOrCreate<T>(string key, Func<T> load)
    {
        string? version = null;
        try
        {
            version = cache.GetString(VersionKey) ?? "0";
            var json = cache.GetString($"catalog:{version}:{key}");
            if (json is not null)
            {
                var value = JsonSerializer.Deserialize<T>(json);
                if (value is not null) return value;
            }
        }
        catch (Exception ex) when (ex is not OutOfMemoryException)
        {
            logger.LogWarning(ex, "Catalog cache unavailable; reading from the database.");
            version = null;
        }
        var result = load();
        if (version is not null)
        {
            try
            {
                cache.SetString($"catalog:{version}:{key}", JsonSerializer.Serialize(result),
                    new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(20) });
            }
            catch (Exception ex) when (ex is not OutOfMemoryException)
            {
                logger.LogWarning(ex, "Could not populate catalog cache.");
            }
        }
        return result;
    }

    public void Invalidate()
    {
        try { cache.SetString(VersionKey, Guid.NewGuid().ToString("N")); }
        catch (Exception ex) when (ex is not OutOfMemoryException)
        {
            logger.LogWarning(ex, "Catalog invalidation unavailable; entries expire after 20 seconds.");
        }
    }
}
