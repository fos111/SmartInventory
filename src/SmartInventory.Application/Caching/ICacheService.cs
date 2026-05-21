namespace SmartInventory.Application.Caching;

/// <summary>
/// Cache-aside wrapper around IDistributedCache with typed serialization
/// and configurable TTL. All keys are prefixed with "SmartInventory:".
/// </summary>
public interface ICacheService
{
    Task<T?> GetAsync<T>(string key);
    Task SetAsync<T>(string key, T value, TimeSpan? expiration = null);
    Task RemoveAsync(string key);
    Task RemoveByPrefixAsync(string prefix);
}
