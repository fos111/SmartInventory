using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.Extensions.Logging;
using SmartInventory.Application.Caching;

namespace SmartInventory.Infrastructure.Caching;

public class BlobCacheService : IBlobCacheService
{
    private readonly BlobContainerClient _containerClient;
    private readonly ILogger<BlobCacheService> _logger;

    public BlobCacheService(
        Func<string, BlobContainerClient> containerClientFactory,
        ILogger<BlobCacheService> logger)
    {
        _logger = logger;
        _containerClient = containerClientFactory("cache");
    }

    public async Task<byte[]?> GetAsync(string key, CancellationToken ct = default)
    {
        try
        {
            var blobClient = _containerClient.GetBlobClient(key);
            var response = await blobClient.DownloadContentAsync(ct);
            return response.Value.Content.ToArray();
        }
        catch (RequestFailedException ex) when (ex.Status == 404)
        {
            return null;
        }
        catch (RequestFailedException ex)
        {
            _logger.LogWarning(ex, "Failed to read blob cache key {Key}", key);
            return null;
        }
    }

    public async Task SetAsync(string key, byte[] data, string contentType, CancellationToken ct = default)
    {
        try
        {
            await _containerClient.CreateIfNotExistsAsync(cancellationToken: ct);

            var blobClient = _containerClient.GetBlobClient(key);
            using var stream = new MemoryStream(data);
            await blobClient.UploadAsync(stream, new BlobUploadOptions
            {
                HttpHeaders = new BlobHttpHeaders { ContentType = contentType }
            }, ct);
        }
        catch (RequestFailedException ex)
        {
            _logger.LogWarning(ex, "Failed to write blob cache key {Key}", key);
        }
    }

    public async Task DeleteAsync(string key, CancellationToken ct = default)
    {
        try
        {
            var blobClient = _containerClient.GetBlobClient(key);
            await blobClient.DeleteIfExistsAsync(cancellationToken: ct);
        }
        catch (RequestFailedException ex)
        {
            _logger.LogWarning(ex, "Failed to delete blob cache key {Key}", key);
        }
    }
}
