using System.Text;
using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using SmartInventory.Infrastructure.Storage;
using Xunit;

namespace SmartInventory.Infrastructure.Tests.Storage;

public class AzureBlobStorageServiceTests
{
    private readonly Mock<BlobContainerClient> _containerClientMock;
    private readonly Mock<BlobClient> _blobClientMock;
    private readonly Mock<ILogger<AzureBlobStorageService>> _loggerMock;
    private readonly AzureBlobStorageService _service;

    public AzureBlobStorageServiceTests()
    {
        _blobClientMock = new Mock<BlobClient>();
        _containerClientMock = new Mock<BlobContainerClient>();
        _loggerMock = new Mock<ILogger<AzureBlobStorageService>>();

        _containerClientMock
            .Setup(c => c.GetBlobClient(It.IsAny<string>()))
            .Returns(_blobClientMock.Object);

        _containerClientMock
            .Setup(c => c.CreateIfNotExistsAsync(It.IsAny<PublicAccessType>(), It.IsAny<Dictionary<string, string>>(), It.IsAny<BlobContainerEncryptionScopeOptions>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Response.FromValue<BlobContainerInfo>(It.IsAny<BlobContainerInfo>(), Mock.Of<Response>()));

        _blobClientMock
            .Setup(b => b.Uri)
            .Returns(new Uri("https://test.blob.core.windows.net/products/photo.png"));

        _service = new AzureBlobStorageService(
            _ => _containerClientMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task SaveFileAsync_UploadsStreamToBlobAndReturnsUrl()
    {
        var stream = new MemoryStream(Encoding.UTF8.GetBytes("test-image-data"));
        var contentType = "image/png";

        var result = await _service.SaveFileAsync("products", "photo.png", stream, contentType);

        result.Should().Be("https://test.blob.core.windows.net/products/photo.png");
        _containerClientMock.Verify(c => c.CreateIfNotExistsAsync(It.IsAny<PublicAccessType>(), It.IsAny<Dictionary<string, string>>(), It.IsAny<BlobContainerEncryptionScopeOptions>(), It.IsAny<CancellationToken>()), Times.Once);
        _containerClientMock.Verify(c => c.GetBlobClient("photo.png"), Times.Once);
        _blobClientMock.Verify(b => b.UploadAsync(stream, It.IsAny<BlobUploadOptions>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task SaveFileAsync_SetsContentTypeOnUpload()
    {
        var stream = new MemoryStream(Encoding.UTF8.GetBytes("test"));
        BlobUploadOptions? capturedOptions = null;

        _blobClientMock
            .Setup(b => b.UploadAsync(It.IsAny<Stream>(), It.IsAny<BlobUploadOptions>(), It.IsAny<CancellationToken>()))
            .Callback<Stream, BlobUploadOptions, CancellationToken>((_, opts, _) => capturedOptions = opts)
            .ReturnsAsync(It.IsAny<Response<BlobContentInfo>>());

        await _service.SaveFileAsync("products", "photo.png", stream, "image/png");

        capturedOptions.Should().NotBeNull();
        capturedOptions!.HttpHeaders.Should().NotBeNull();
        capturedOptions.HttpHeaders.ContentType.Should().Be("image/png");
    }

    [Fact]
    public async Task DeleteFileAsync_WhenUrlValid_DeletesBlobAndReturnsTrue()
    {
        _blobClientMock
            .Setup(b => b.DeleteIfExistsAsync(It.IsAny<DeleteSnapshotsOption>(), It.IsAny<BlobRequestConditions>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Response.FromValue(true, Mock.Of<Response>()));

        var result = await _service.DeleteFileAsync("https://test.blob.core.windows.net/products/photo.png");

        result.Should().BeTrue();
        _containerClientMock.Verify(c => c.GetBlobClient("photo.png"), Times.Once);
        _blobClientMock.Verify(b => b.DeleteIfExistsAsync(It.IsAny<DeleteSnapshotsOption>(), It.IsAny<BlobRequestConditions>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteFileAsync_WhenBlobNotFound_ReturnsFalse()
    {
        _blobClientMock
            .Setup(b => b.DeleteIfExistsAsync(It.IsAny<DeleteSnapshotsOption>(), It.IsAny<BlobRequestConditions>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Response.FromValue(false, Mock.Of<Response>()));

        var result = await _service.DeleteFileAsync("https://test.blob.core.windows.net/products/nonexistent.png");

        result.Should().BeFalse();
    }

    [Fact]
    public async Task DeleteFileAsync_WhenUrlEmpty_ReturnsFalse()
    {
        var result = await _service.DeleteFileAsync("");
        result.Should().BeFalse();
    }

    [Fact]
    public async Task DeleteFileAsync_WhenUrlInvalid_ReturnsFalse()
    {
        var result = await _service.DeleteFileAsync("not-a-url");
        result.Should().BeFalse();
    }

    [Fact]
    public async Task DeleteFileAsync_WhenStorageError_ReturnsFalse()
    {
        _blobClientMock
            .Setup(b => b.DeleteIfExistsAsync(It.IsAny<DeleteSnapshotsOption>(), It.IsAny<BlobRequestConditions>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new RequestFailedException("Storage unavailable"));

        var result = await _service.DeleteFileAsync("https://test.blob.core.windows.net/products/photo.png");

        result.Should().BeFalse();
    }

    [Fact]
    public async Task SaveFileAsync_UsesContainerFromArgument()
    {
        string? usedContainer = null;
        var localService = new AzureBlobStorageService(
            containerName =>
            {
                usedContainer = containerName;
                return _containerClientMock.Object;
            },
            _loggerMock.Object);

        var stream = new MemoryStream(Encoding.UTF8.GetBytes("test"));
        await localService.SaveFileAsync("avatars", "user.png", stream, "image/png");

        usedContainer.Should().Be("avatars");
    }
}
