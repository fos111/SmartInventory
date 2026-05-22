using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.Extensions.Logging;
using SmartInventory.Application.Storage.Interfaces;

namespace SmartInventory.Infrastructure.Storage;

public class AzureBlobStorageService : IFileStorageService
{
    private readonly Func<string, BlobContainerClient> _containerClientFactory;
    private readonly ILogger<AzureBlobStorageService> _logger;

    public AzureBlobStorageService(
        Func<string, BlobContainerClient> containerClientFactory,
        ILogger<AzureBlobStorageService> logger)
    {
        _containerClientFactory = containerClientFactory;
        _logger = logger;
    }

    public async Task<string> SaveFileAsync(string container, string fileName, Stream fileStream, string contentType, CancellationToken ct = default)
    {
        var containerClient = _containerClientFactory(container);
        await containerClient.CreateIfNotExistsAsync(cancellationToken: ct);

        var blobClient = containerClient.GetBlobClient(fileName);
        await blobClient.UploadAsync(fileStream, new BlobUploadOptions
        {
            HttpHeaders = new BlobHttpHeaders { ContentType = contentType }
        }, ct);

        _logger.LogInformation("Uploaded blob {Container}/{FileName} ({ContentType})", container, fileName, contentType);

        return blobClient.Uri.ToString();
    }

    public async Task<bool> DeleteFileAsync(string fileUrl, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(fileUrl))
            return false;

        if (!Uri.TryCreate(fileUrl, UriKind.Absolute, out var blobUri))
            return false;

        var segments = blobUri.Segments;
        if (segments.Length < 3)
            return false;

        var container = segments[1].TrimEnd('/');
        var blobName = string.Concat(segments.Skip(2));

        var containerClient = _containerClientFactory(container);
        var blobClient = containerClient.GetBlobClient(blobName);

        try
        {
            var response = await blobClient.DeleteIfExistsAsync(cancellationToken: ct);
            if (response.Value)
                _logger.LogInformation("Deleted blob {BlobUrl}", fileUrl);
            return response.Value;
        }
        catch (RequestFailedException ex)
        {
            _logger.LogWarning(ex, "Failed to delete blob {BlobUrl}", fileUrl);
            return false;
        }
    }
}
