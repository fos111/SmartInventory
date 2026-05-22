namespace SmartInventory.Application.Caching;

public interface IBlobCacheService
{
    Task<byte[]?> GetAsync(string key, CancellationToken ct = default);
    Task SetAsync(string key, byte[] data, string contentType, CancellationToken ct = default);
    Task DeleteAsync(string key, CancellationToken ct = default);
}
