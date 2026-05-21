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
using SmartInventory.Domain.Location.Entities;
using Xunit;

namespace SmartInventory.Application.Tests.Mobile.Products.Services;

public class MobileProductWriteServiceFieldMappingTests
{
    private readonly Mock<IAssetService> _assetServiceMock;
    private readonly Mock<ILocationRepository> _locationRepoMock;
    private readonly IMobileProductWriteService _service;
    private readonly Guid _defaultUserId = Guid.NewGuid();

    public MobileProductWriteServiceFieldMappingTests()
    {
        _assetServiceMock = new Mock<IAssetService>();
        _locationRepoMock = new Mock<ILocationRepository>();
        _service = new MobileProductWriteService(_assetServiceMock.Object, _locationRepoMock.Object);
    }

    [Fact]
    public async Task CreateProductFromMobileAsync_ValidRequest_MapsAllFieldsCorrectly()
    {
        var dto = new MobileProductCreateDto
        {
            Name = "New Laptop",
            Sku = "SKU-001",
            Type = "Electronics",
            Description = "A test laptop",
            RoomId = "Room-A1-01",  // room code, not UUID
            PhotoPath = "/uploads/products/photo.jpg"
        };

        _assetServiceMock
            .Setup(s => s.CreateAssetAsync(It.IsAny<CreateAssetDto>(), _defaultUserId))
            .ReturnsAsync(new AssetDto
            {
                Id = Guid.NewGuid(),
                AssetTag = "SKU-001",
                Name = "New Laptop",
                Status = AssetStatus.Active,
                CurrentRoomCode = "Room-A1-01",
                PhotoPath = "/uploads/products/photo.jpg"
            });

        var result = await _service.CreateProductFromMobileAsync(dto, _defaultUserId, CancellationToken.None);

        result.Should().NotBeNull();
        result!.Name.Should().Be("New Laptop");

        _assetServiceMock.Verify(s => s.CreateAssetAsync(
            It.Is<CreateAssetDto>(a =>
                a.AssetTag == "SKU-001" &&
                a.Name == "New Laptop" &&
                a.Category == "Electronics" &&
                a.Description == "A test laptop" &&
                a.CurrentRoomCode == "Room-A1-01" &&
                a.PhotoPath == "/uploads/products/photo.jpg"),
            _defaultUserId), Times.Once);
    }

    [Fact]
    public async Task CreateProductFromMobileAsync_WithRoomUuid_ResolvesToRoomCode()
    {
        var roomUuid = Guid.NewGuid();
        var dto = new MobileProductCreateDto
        {
            Name = "Asset with UUID",
            Sku = "SKU-002",
            RoomId = roomUuid.ToString()
        };

        var room = new Room { Id = roomUuid, Code = "LI1", Name = "Room LI1" };
        _locationRepoMock
            .Setup(r => r.GetRoomByIdAsync(roomUuid))
            .ReturnsAsync(room);

        _assetServiceMock
            .Setup(s => s.CreateAssetAsync(It.IsAny<CreateAssetDto>(), _defaultUserId))
            .ReturnsAsync(new AssetDto
            {
                Id = Guid.NewGuid(),
                AssetTag = "SKU-002",
                Name = "Asset with UUID",
                Status = AssetStatus.Active,
                CurrentRoomCode = "LI1"
            });

        await _service.CreateProductFromMobileAsync(dto, _defaultUserId, CancellationToken.None);

        _assetServiceMock.Verify(s => s.CreateAssetAsync(
            It.Is<CreateAssetDto>(a => a.CurrentRoomCode == "LI1"),
            _defaultUserId), Times.Once);

        _locationRepoMock.Verify(r => r.GetRoomByIdAsync(roomUuid), Times.Once);
    }

    [Fact]
    public async Task CreateProductFromMobileAsync_NoRoomId_UsesEmptyString()
    {
        var dto = new MobileProductCreateDto
        {
            Name = "No Room Asset",
            Sku = "SKU-003"
        };

        _assetServiceMock
            .Setup(s => s.CreateAssetAsync(It.IsAny<CreateAssetDto>(), _defaultUserId))
            .ReturnsAsync(new AssetDto
            {
                Id = Guid.NewGuid(),
                AssetTag = "SKU-003",
                Name = "No Room Asset",
                Status = AssetStatus.Active,
                CurrentRoomCode = string.Empty
            });

        await _service.CreateProductFromMobileAsync(dto, _defaultUserId, CancellationToken.None);

        _assetServiceMock.Verify(s => s.CreateAssetAsync(
            It.Is<CreateAssetDto>(a => a.CurrentRoomCode == string.Empty),
            _defaultUserId), Times.Once);

        _locationRepoMock.Verify(r => r.GetRoomByIdAsync(It.IsAny<Guid>()), Times.Never);
    }

    [Fact]
    public async Task CreateProductFromMobileAsync_NullStatus_UsesDefaultInStock()
    {
        var dto = new MobileProductCreateDto
        {
            Name = "Test",
            Sku = "SKU-004"
        };

        _assetServiceMock
            .Setup(s => s.CreateAssetAsync(It.IsAny<CreateAssetDto>(), _defaultUserId))
            .ReturnsAsync(new AssetDto { Id = Guid.NewGuid(), Name = "Test" });

        await _service.CreateProductFromMobileAsync(dto, _defaultUserId, CancellationToken.None);

        _assetServiceMock.Verify(s => s.CreateAssetAsync(
            It.Is<CreateAssetDto>(a => a.Status == AssetStatus.InStock),
            _defaultUserId), Times.Once);
    }

    [Fact]
    public async Task CreateProductFromMobileAsync_WithPhotoPath_PassesThrough()
    {
        var dto = new MobileProductCreateDto
        {
            Name = "Photo Asset",
            Sku = "SKU-005",
            PhotoPath = "/uploads/products/new-photo.jpg"
        };

        _assetServiceMock
            .Setup(s => s.CreateAssetAsync(It.IsAny<CreateAssetDto>(), _defaultUserId))
            .ReturnsAsync(new AssetDto
            {
                Id = Guid.NewGuid(),
                AssetTag = "SKU-005",
                Name = "Photo Asset",
                Status = AssetStatus.Active,
                PhotoPath = "/uploads/products/new-photo.jpg"
            });

        var result = await _service.CreateProductFromMobileAsync(dto, _defaultUserId, CancellationToken.None);

        _assetServiceMock.Verify(s => s.CreateAssetAsync(
            It.Is<CreateAssetDto>(a => a.PhotoPath == "/uploads/products/new-photo.jpg"),
            _defaultUserId), Times.Once);

        result.Should().NotBeNull();
        result!.PhotoUrl.Should().Be("/uploads/products/new-photo.jpg");
    }

    [Fact]
    public async Task CreateProductFromMobileAsync_UnmappableFields_AreAccepted()
    {
        var dto = new MobileProductCreateDto
        {
            Name = "Unmappable Fields Test",
            Sku = "SKU-006",
            Tags = "[\"tag1\",\"tag2\"]",
            Price = "299.99",
            Specifications = "{\"color\":\"black\"}"
        };

        _assetServiceMock
            .Setup(s => s.CreateAssetAsync(It.IsAny<CreateAssetDto>(), _defaultUserId))
            .ReturnsAsync(new AssetDto { Id = Guid.NewGuid(), AssetTag = "SKU-006", Name = "Unmappable Fields Test" });

        var result = await _service.CreateProductFromMobileAsync(dto, _defaultUserId, CancellationToken.None);

        result.Should().NotBeNull();
        // The unmappable fields are accepted but not stored — no error should occur
    }

    [Fact]
    public async Task UpdateProductAsync_ValidRequest_MapsFieldsCorrectly()
    {
        var assetId = Guid.NewGuid();
        var dto = new MobileProductCreateDto
        {
            Name = "Updated Laptop",
            Sku = "SKU-001",
            Type = "UpdatedCategory",
            Description = "Updated description",
            PhotoPath = "/uploads/products/updated.jpg"
        };

        _assetServiceMock
            .Setup(s => s.UpdateAssetAsync(assetId, It.IsAny<UpdateAssetDto>(), _defaultUserId))
            .ReturnsAsync(new AssetDto
            {
                Id = assetId,
                AssetTag = "SKU-001",
                Name = "Updated Laptop",
                Status = AssetStatus.Active,
                CurrentRoomCode = "Room-A1-01",
                PhotoPath = "/uploads/products/updated.jpg"
            });

        var result = await _service.UpdateProductAsync(assetId, dto, _defaultUserId, CancellationToken.None);

        result.Should().NotBeNull();
        result!.Name.Should().Be("Updated Laptop");

        _assetServiceMock.Verify(s => s.UpdateAssetAsync(assetId,
            It.Is<UpdateAssetDto>(a =>
                a.Name == "Updated Laptop" &&
                a.Category == "UpdatedCategory" &&
                a.Description == "Updated description" &&
                a.PhotoPath == "/uploads/products/updated.jpg"),
            _defaultUserId), Times.Once);
    }

    [Fact]
    public async Task UpdateProductAsync_NonExistentId_ReturnsNull()
    {
        var id = Guid.NewGuid();
        var dto = new MobileProductCreateDto { Name = "Ghost", Sku = "GHOST" };

        _assetServiceMock
            .Setup(s => s.UpdateAssetAsync(id, It.IsAny<UpdateAssetDto>(), _defaultUserId))
            .ThrowsAsync(new ArgumentException("Asset not found"));

        var result = await _service.UpdateProductAsync(id, dto, _defaultUserId, CancellationToken.None);

        result.Should().BeNull();
    }
}
