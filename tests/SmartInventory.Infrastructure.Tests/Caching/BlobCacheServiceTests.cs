using System.Text;
using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using SmartInventory.Infrastructure.Caching;
using Xunit;

namespace SmartInventory.Infrastructure.Tests.Caching;

public class BlobCacheServiceTests
{
    private readonly Mock<BlobContainerClient> _containerClientMock;
    private readonly Mock<BlobClient> _blobClientMock;
    private readonly BlobCacheService _service;

    public BlobCacheServiceTests()
    {
        _blobClientMock = new Mock<BlobClient>();
        _containerClientMock = new Mock<BlobContainerClient>();

        _containerClientMock
            .Setup(c => c.GetBlobClient(It.IsAny<string>()))
            .Returns(_blobClientMock.Object);

        _containerClientMock
            .Setup(c => c.CreateIfNotExistsAsync(It.IsAny<PublicAccessType>(), It.IsAny<Dictionary<string, string>>(), It.IsAny<BlobContainerEncryptionScopeOptions>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Response.FromValue<BlobContainerInfo>(Mock.Of<BlobContainerInfo>(), Mock.Of<Response>()));

        var loggerMock = new Mock<ILogger<BlobCacheService>>();

        _service = new BlobCacheService(
            _ => _containerClientMock.Object,
            loggerMock.Object);
    }

    [Fact]
    public async Task GetAsync_WhenKeyExists_ReturnsBytes()
    {
        var expectedData = Encoding.UTF8.GetBytes("test-data");
        var mockDownloadResult = BlobsModelFactory.BlobDownloadResult(BinaryData.FromBytes(expectedData));

        _blobClientMock
            .Setup(b => b.DownloadContentAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(Response.FromValue(mockDownloadResult, Mock.Of<Response>()));

        var result = await _service.GetAsync("qrcodes/asset-abc.png");

        result.Should().NotBeNull();
        Encoding.UTF8.GetString(result!).Should().Be("test-data");
    }

    [Fact]
    public async Task GetAsync_WhenKeyNotFound_ReturnsNull()
    {
        _blobClientMock
            .Setup(b => b.DownloadContentAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new RequestFailedException(404, "Not found", "BlobNotFound", null));

        var result = await _service.GetAsync("nonexistent");

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetAsync_WhenStorageError_ReturnsNull()
    {
        _blobClientMock
            .Setup(b => b.DownloadContentAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new RequestFailedException("Storage unavailable"));

        var result = await _service.GetAsync("key");

        result.Should().BeNull();
    }

    [Fact]
    public async Task SetAsync_UploadsDataToBlobWithContentType()
    {
        var data = Encoding.UTF8.GetBytes("qr-code-png-data");
        BlobUploadOptions? capturedOptions = null;

        _blobClientMock
            .Setup(b => b.UploadAsync(It.IsAny<Stream>(), It.IsAny<BlobUploadOptions>(), It.IsAny<CancellationToken>()))
            .Callback<Stream, BlobUploadOptions, CancellationToken>((_, opts, _) => capturedOptions = opts)
            .ReturnsAsync(Response.FromValue<BlobContentInfo>(Mock.Of<BlobContentInfo>(), Mock.Of<Response>()));

        await _service.SetAsync("qrcodes/asset-abc.png", data, "image/png");

        _containerClientMock.Verify(c => c.CreateIfNotExistsAsync(It.IsAny<PublicAccessType>(), It.IsAny<Dictionary<string, string>>(), It.IsAny<BlobContainerEncryptionScopeOptions>(), It.IsAny<CancellationToken>()), Times.Once);
        _containerClientMock.Verify(c => c.GetBlobClient("qrcodes/asset-abc.png"), Times.Once);
        _blobClientMock.Verify(b => b.UploadAsync(It.IsAny<Stream>(), It.IsAny<BlobUploadOptions>(), It.IsAny<CancellationToken>()), Times.Once);
        capturedOptions!.HttpHeaders.ContentType.Should().Be("image/png");
    }

    [Fact]
    public async Task SetAsync_WhenStorageError_DoesNotThrow()
    {
        var data = Encoding.UTF8.GetBytes("test");
        _blobClientMock
            .Setup(b => b.UploadAsync(It.IsAny<Stream>(), It.IsAny<BlobUploadOptions>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new RequestFailedException("Storage unavailable"));

        var act = async () => await _service.SetAsync("key", data, "text/plain");
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task DeleteAsync_DeletesBlobByKey()
    {
        await _service.DeleteAsync("qrcodes/asset-abc.png");

        _containerClientMock.Verify(c => c.GetBlobClient("qrcodes/asset-abc.png"), Times.Once);
        _blobClientMock.Verify(b => b.DeleteIfExistsAsync(It.IsAny<DeleteSnapshotsOption>(), It.IsAny<BlobRequestConditions>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_WhenStorageError_DoesNotThrow()
    {
        _blobClientMock
            .Setup(b => b.DeleteIfExistsAsync(It.IsAny<DeleteSnapshotsOption>(), It.IsAny<BlobRequestConditions>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new RequestFailedException("Storage unavailable"));

        var act = async () => await _service.DeleteAsync("key");
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public void Constructor_UsesCacheContainer()
    {
        string? usedContainer = null;
        var loggerMock = new Mock<ILogger<BlobCacheService>>();

        var localService = new BlobCacheService(
            containerName =>
            {
                usedContainer = containerName;
                return _containerClientMock.Object;
            },
            loggerMock.Object);

        usedContainer.Should().Be("cache");
    }
}
