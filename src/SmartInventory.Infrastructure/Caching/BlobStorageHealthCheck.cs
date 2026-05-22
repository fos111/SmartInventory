using Azure.Storage.Blobs;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace SmartInventory.Infrastructure.Caching;

public class BlobStorageHealthCheck : IHealthCheck
{
    private readonly BlobServiceClient? _blobServiceClient;

    public BlobStorageHealthCheck(BlobServiceClient? blobServiceClient = null)
    {
        _blobServiceClient = blobServiceClient;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        if (_blobServiceClient == null)
            return HealthCheckResult.Healthy("Blob storage not configured — using local storage");

        try
        {
            var properties = await _blobServiceClient.GetAccountInfoAsync(cancellationToken);
            return HealthCheckResult.Healthy("Azure Blob Storage is accessible");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("Azure Blob Storage is not accessible", ex);
        }
    }
}
