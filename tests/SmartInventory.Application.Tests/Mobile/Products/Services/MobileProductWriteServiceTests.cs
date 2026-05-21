using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using SmartInventory.Application.Asset.DTOs;
using SmartInventory.Application.Asset.Interfaces;
using SmartInventory.Application.Location.Interfaces;
using SmartInventory.Application.Mobile.Products.DTOs;
using SmartInventory.Application.Mobile.Products.Interfaces;
using SmartInventory.Application.Mobile.Products.Services;
using SmartInventory.Domain.Asset.Enums;
using Xunit;

namespace SmartInventory.Application.Tests.Mobile.Products.Services;

public class MobileProductWriteServiceTests
{
    private readonly Mock<IAssetService> _assetServiceMock;
    private readonly Mock<ILocationRepository> _locationRepoMock;
    private readonly IMobileProductWriteService _service;
    private readonly Guid _defaultUserId = Guid.NewGuid();

    public MobileProductWriteServiceTests()
    {
        _assetServiceMock = new Mock<IAssetService>();
        _locationRepoMock = new Mock<ILocationRepository>();
        _service = new MobileProductWriteService(_assetServiceMock.Object, _locationRepoMock.Object);
    }

    #region CreateProductAsync

    [Fact]
    public async Task CreateProductAsync_ValidDto_ReturnsAssetScanDtoAndCallsService()
    {
        var dto = new CreateProductRequestDto
        {
            AssetTag = "AST-NEW",
            Name = "New Laptop",
            Description = "A new laptop",
            Category = "Electronics",
            Status = "active",
            Manufacturer = "Dell",
            Model = "Latitude 5420",
            SerialNumber = "SN12345",
            CurrentRoomCode = "Room-A1-01",
            InstallDate = new DateTime(2026, 5, 1, 0, 0, 0, DateTimeKind.Utc),
            LastServiceDate = new DateTime(2026, 6, 1, 0, 0, 0, DateTimeKind.Utc)
        };

        var createdAsset = new AssetDto
        {
            Id = Guid.NewGuid(),
            AssetTag = "AST-NEW",
            Name = "New Laptop",
            Status = AssetStatus.Active,
            CurrentRoomCode = "Room-A1-01"
        };

        _assetServiceMock
            .Setup(s => s.CreateAssetAsync(It.IsAny<CreateAssetDto>(), _defaultUserId))
            .ReturnsAsync(createdAsset);

        var result = await _service.CreateProductAsync(dto, _defaultUserId, CancellationToken.None);

        result.Should().NotBeNull();
        result!.Id.Should().Be(createdAsset.Id);
        result.AssetTag.Should().Be("AST-NEW");
        result.Name.Should().Be("New Laptop");
        result.Status.Should().Be("Active");
        result.CurrentRoomCode.Should().Be("Room-A1-01");

        _assetServiceMock.Verify(s => s.CreateAssetAsync(
            It.Is<CreateAssetDto>(a =>
                a.Name == "New Laptop" &&
                a.Category == "Electronics" &&
                a.CurrentRoomCode == "Room-A1-01" &&
                a.Manufacturer == "Dell" &&
                a.Model == "Latitude 5420" &&
                a.SerialNumber == "SN12345" &&
                a.AssetTag == "AST-NEW" &&
                a.Status == AssetStatus.Active),
            _defaultUserId), Times.Once);
    }

    [Fact]
    public async Task CreateProductAsync_MapsStatusFromStringToEnum()
    {
        var dto = new CreateProductRequestDto
        {
            Name = "Test Asset",
            Category = "General",
            CurrentRoomCode = "Room-01",
            Status = "maintenance"
        };

        _assetServiceMock
            .Setup(s => s.CreateAssetAsync(It.IsAny<CreateAssetDto>(), It.IsAny<Guid>()))
            .ReturnsAsync(new AssetDto { Id = Guid.NewGuid(), Name = "Test Asset" });

        await _service.CreateProductAsync(dto, _defaultUserId, CancellationToken.None);

        _assetServiceMock.Verify(s => s.CreateAssetAsync(
            It.Is<CreateAssetDto>(a => a.Status == AssetStatus.Maintenance),
            It.IsAny<Guid>()), Times.Once);
    }

    [Fact]
    public async Task CreateProductAsync_NullStatus_UsesDefaultInStock()
    {
        var dto = new CreateProductRequestDto
        {
            Name = "Test Asset",
            Category = "General",
            CurrentRoomCode = "Room-01",
            Status = null
        };

        _assetServiceMock
            .Setup(s => s.CreateAssetAsync(It.IsAny<CreateAssetDto>(), It.IsAny<Guid>()))
            .ReturnsAsync(new AssetDto { Id = Guid.NewGuid(), Name = "Test Asset" });

        await _service.CreateProductAsync(dto, _defaultUserId, CancellationToken.None);

        _assetServiceMock.Verify(s => s.CreateAssetAsync(
            It.Is<CreateAssetDto>(a => a.Status == AssetStatus.InStock),
            It.IsAny<Guid>()), Times.Once);
    }

    [Fact]
    public async Task CreateProductAsync_InvalidStatus_UsesDefaultInStock()
    {
        var dto = new CreateProductRequestDto
        {
            Name = "Test Asset",
            Category = "General",
            CurrentRoomCode = "Room-01",
            Status = "invalid_status_value"
        };

        _assetServiceMock
            .Setup(s => s.CreateAssetAsync(It.IsAny<CreateAssetDto>(), It.IsAny<Guid>()))
            .ReturnsAsync(new AssetDto { Id = Guid.NewGuid(), Name = "Test Asset" });

        await _service.CreateProductAsync(dto, _defaultUserId, CancellationToken.None);

        _assetServiceMock.Verify(s => s.CreateAssetAsync(
            It.Is<CreateAssetDto>(a => a.Status == AssetStatus.InStock),
            It.IsAny<Guid>()), Times.Once);
    }

    #endregion

    #region UpdateProductStatusAsync

    [Fact]
    public async Task UpdateProductStatusAsync_ValidStatus_ReturnsAssetScanDto()
    {
        var assetId = Guid.NewGuid();
        var asset = new AssetDto
        {
            Id = assetId,
            AssetTag = "AST-001",
            Name = "Laptop",
            Status = AssetStatus.Maintenance,
            CurrentRoomCode = "Room-A1-01"
        };

        _assetServiceMock
            .Setup(s => s.UpdateStatusAsync(assetId, AssetStatus.Maintenance, _defaultUserId, It.IsAny<Domain.Auth.Enums.UserRole>(), It.IsAny<string?>()))
            .ReturnsAsync(asset);

        var result = await _service.UpdateProductStatusAsync(assetId, "maintenance", _defaultUserId, CancellationToken.None);

        result.Should().NotBeNull();
        result!.Id.Should().Be(assetId);
        result.Status.Should().Be("Maintenance");
    }

    [Fact]
    public async Task UpdateProductStatusAsync_InvalidStatusString_ReturnsNull()
    {
        var result = await _service.UpdateProductStatusAsync(
            Guid.NewGuid(), "not_a_real_status", _defaultUserId, CancellationToken.None);

        result.Should().BeNull();
    }

    [Fact]
    public async Task UpdateProductStatusAsync_NonExistentId_ReturnsNull()
    {
        var id = Guid.NewGuid();

        _assetServiceMock
            .Setup(s => s.UpdateStatusAsync(id, It.IsAny<AssetStatus>(), _defaultUserId, It.IsAny<Domain.Auth.Enums.UserRole>(), It.IsAny<string?>()))
            .ThrowsAsync(new ArgumentException("Asset not found"));

        var result = await _service.UpdateProductStatusAsync(id, "active", _defaultUserId, CancellationToken.None);

        result.Should().BeNull();
    }

    #endregion

    #region MoveProductAsync

    [Fact]
    public async Task MoveProductAsync_ValidRoomId_ReturnsAssetScanDto()
    {
        var assetId = Guid.NewGuid();
        var roomId = "Room-B2-01";
        var asset = new AssetDto
        {
            Id = assetId,
            AssetTag = "AST-001",
            Name = "Laptop",
            Status = AssetStatus.Active,
            CurrentRoomCode = roomId
        };

        _assetServiceMock
            .Setup(s => s.MoveAssetAsync(assetId, roomId, _defaultUserId))
            .ReturnsAsync(asset);

        var (data, _) = await _service.MoveProductAsync(assetId, roomId, _defaultUserId, CancellationToken.None);

        data.Should().NotBeNull();
        data!.Id.Should().Be(assetId);
        data.CurrentRoomCode.Should().Be(roomId);
    }

    [Fact]
    public async Task MoveProductAsync_NonExistentId_ReturnsNull()
    {
        var id = Guid.NewGuid();

        _assetServiceMock
            .Setup(s => s.MoveAssetAsync(id, It.IsAny<string>(), _defaultUserId))
            .ThrowsAsync(new ArgumentException("Asset not found"));

        var (data, _) = await _service.MoveProductAsync(id, "Room-B2", _defaultUserId, CancellationToken.None);

        data.Should().BeNull();
    }

    [Fact]
    public async Task MoveProductAsync_InvalidRoomCode_ReturnsNull()
    {
        var id = Guid.NewGuid();

        _assetServiceMock
            .Setup(s => s.MoveAssetAsync(id, It.IsAny<string>(), _defaultUserId))
            .ThrowsAsync(new ArgumentException("Room not found"));

        var (data, message) = await _service.MoveProductAsync(id, "INVALID", _defaultUserId, CancellationToken.None);

        data.Should().BeNull();
        message.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task MoveProductAsync_PassesRoomIdDirectlyAsNewRoomCode()
    {
        var assetId = Guid.NewGuid();
        var roomId = "Room-X1-99";

        _assetServiceMock
            .Setup(s => s.MoveAssetAsync(assetId, roomId, _defaultUserId))
            .ReturnsAsync(new AssetDto { Id = assetId, CurrentRoomCode = roomId });

        await _service.MoveProductAsync(assetId, roomId, _defaultUserId, CancellationToken.None);

        _assetServiceMock.Verify(s => s.MoveAssetAsync(assetId, roomId, _defaultUserId), Times.Once);
    }

    #endregion

    #region DeleteProductAsync

    [Fact]
    public async Task DeleteProductAsync_ValidId_ReturnsTrue()
    {
        var id = Guid.NewGuid();

        _assetServiceMock
            .Setup(s => s.DeleteAssetAsync(id.ToString(), _defaultUserId))
            .Returns(Task.CompletedTask);

        var (success, _) = await _service.DeleteProductAsync(id, _defaultUserId, CancellationToken.None);

        success.Should().BeTrue();
    }

    [Fact]
    public async Task DeleteProductAsync_NonExistentId_ReturnsFalse()
    {
        var id = Guid.NewGuid();

        _assetServiceMock
            .Setup(s => s.DeleteAssetAsync(id.ToString(), _defaultUserId))
            .ThrowsAsync(new ArgumentException("Asset not found"));

        var (success, message) = await _service.DeleteProductAsync(id, _defaultUserId, CancellationToken.None);

        success.Should().BeFalse();
        message.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task DeleteProductAsync_PassesIdAsString()
    {
        var id = Guid.NewGuid();

        _assetServiceMock
            .Setup(s => s.DeleteAssetAsync(id.ToString(), _defaultUserId))
            .Returns(Task.CompletedTask);

        await _service.DeleteProductAsync(id, _defaultUserId, CancellationToken.None);

        _assetServiceMock.Verify(s => s.DeleteAssetAsync(id.ToString(), _defaultUserId), Times.Once);
    }

    #endregion
}
