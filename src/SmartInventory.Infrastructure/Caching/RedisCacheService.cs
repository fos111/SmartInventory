using System.Text.Json;
using System.Text.Json.Serialization;
using SmartInventory.Application.Caching;
using StackExchange.Redis;

namespace SmartInventory.Infrastructure.Caching;

public class RedisCacheService : ICacheService
{
    private const string KeyPrefix = "SmartInventory:";

    private readonly IDatabase _db;
    private readonly IServer _server;
    private readonly JsonSerializerOptions _jsonOptions;

    public RedisCacheService(IConnectionMultiplexer connection)
    {
        _db = connection.GetDatabase();
        _server = connection.GetServer(connection.GetEndPoints().First());
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            ReferenceHandler = ReferenceHandler.IgnoreCycles
        };
    }

    public async Task<T?> GetAsync<T>(string key)
    {
        var value = await _db.StringGetAsync(KeyPrefix + key);
        return value.IsNullOrEmpty ? default : JsonSerializer.Deserialize<T>(value!, _jsonOptions);
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan? expiration = null)
    {
        var serialized = JsonSerializer.Serialize(value, _jsonOptions);
        await _db.StringSetAsync(KeyPrefix + key, serialized, expiration);
    }

    public async Task RemoveAsync(string key)
    {
        await _db.KeyDeleteAsync(KeyPrefix + key);
    }

    public async Task RemoveByPrefixAsync(string prefix)
    {
        var pattern = KeyPrefix + prefix + "*";
        var keys = _server.Keys(pattern: pattern).ToArray();
        if (keys.Length > 0)
            await _db.KeyDeleteAsync(keys);
    }
}
