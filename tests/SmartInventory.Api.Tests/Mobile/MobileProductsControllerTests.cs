using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using SmartInventory.Api.Controllers.Mobile;
using SmartInventory.Api.Models;
using SmartInventory.Application.Asset.Interfaces;
using SmartInventory.Application.Mobile.Products.DTOs;
using SmartInventory.Application.Mobile.Products.Interfaces;
using SmartInventory.Application.Storage.Interfaces;
using Xunit;

namespace SmartInventory.Api.Tests.Mobile;

public class MobileProductsControllerTests
{
    private readonly Mock<IMobileProductService> _productServiceMock;
    private readonly Mock<IMobileProductWriteService> _writeServiceMock;
    private readonly Mock<IFileStorageService> _fileStorageMock;
    private readonly Mock<IAssetService> _assetServiceMock;
    private readonly MobileProductsController _controller;
    private readonly Guid _defaultUserId = Guid.NewGuid();

    public MobileProductsControllerTests()
    {
        _productServiceMock = new Mock<IMobileProductService>();
        _writeServiceMock = new Mock<IMobileProductWriteService>();
        _fileStorageMock = new Mock<IFileStorageService>();
        _assetServiceMock = new Mock<IAssetService>();
        _controller = new MobileProductsController(_productServiceMock.Object, _writeServiceMock.Object, _fileStorageMock.Object, _assetServiceMock.Object);

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, _defaultUserId.ToString())
        };
        var identity = new ClaimsIdentity(claims, "Test");
        var principal = new ClaimsPrincipal(identity);
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = principal }
        };
    }

    [Fact]
    public async Task GetProducts_ReturnsOkWithEnvelope()
    {
        var items = new List<AssetListItemDto>
        {
            new()
            {
                Id = Guid.NewGuid(),
                AssetTag = "AST-001",
                Name = "Laptop Dell",
                Status = "Active",
                CurrentRoomCode = "Room-A1-01",
                Category = "Electronics"
            }
        };

        var pagedResult = new MobilePagedResultDto<AssetListItemDto>
        {
            Items = items,
            TotalCount = 1,
            Page = 1,
            PageSize = 20,
            HasNextPage = false
        };

        _productServiceMock
            .Setup(s => s.GetProductsAsync(It.IsAny<MobileProductFilterDto>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(pagedResult);

        var filter = new MobileProductFilterDto();
        var result = await _controller.GetProducts(filter, CancellationToken.None);

        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var envelope = okResult.Value.Should().BeOfType<MobileEnvelope<MobilePagedResultDto<AssetListItemDto>>>().Subject;
        envelope.Success.Should().BeTrue();
        envelope.Data.Should().NotBeNull();
        envelope.Data!.Items.Should().HaveCount(1);
        envelope.Data.Items[0].AssetTag.Should().Be("AST-001");
        envelope.Data.TotalCount.Should().Be(1);
    }

    [Fact]
    public async Task ScanByTag_Found_ReturnsOkWithAssetScanDto()
    {
        var assetTag = "AST-001";
        var scanResult = new AssetScanDto
        {
            Id = Guid.NewGuid(),
            AssetTag = assetTag,
            Name = "Laptop Dell",
            Status = "Active",
            CurrentRoomCode = "Room-A1-01"
        };

        _productServiceMock
            .Setup(s => s.ScanByTagAsync(assetTag, It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(scanResult);

        var result = await _controller.ScanByTag(assetTag, CancellationToken.None);

        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var envelope = okResult.Value.Should().BeOfType<MobileEnvelope<AssetScanDto>>().Subject;
        envelope.Success.Should().BeTrue();
        envelope.Data.Should().NotBeNull();
        envelope.Data!.AssetTag.Should().Be(assetTag);
        envelope.Data.Name.Should().Be("Laptop Dell");
        envelope.Data.Status.Should().Be("Active");
        envelope.Data.CurrentRoomCode.Should().Be("Room-A1-01");
    }

    [Fact]
    public async Task ScanByTag_NotFound_ReturnsOkWithFailure()
    {
        _productServiceMock
            .Setup(s => s.ScanByTagAsync(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((AssetScanDto?)null);

        var result = await _controller.ScanByTag("NONEXISTENT", CancellationToken.None);

        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var envelope = okResult.Value.Should().BeOfType<MobileEnvelope<AssetScanDto>>().Subject;
        envelope.Success.Should().BeFalse();
        envelope.Message.Should().NotBeNullOrEmpty();
        envelope.Data.Should().BeNull();
    }

    [Fact]
    public async Task ScanByTag_CallsServiceWithUserId()
    {
        var userId = Guid.NewGuid();
        var assetTag = "AST-001";

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, userId.ToString())
        };
        var identity = new ClaimsIdentity(claims, "Test");
        var principal = new ClaimsPrincipal(identity);
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = principal }
        };

        _productServiceMock
            .Setup(s => s.ScanByTagAsync(assetTag, userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new AssetScanDto());

        await _controller.ScanByTag(assetTag, CancellationToken.None);

        _productServiceMock.Verify(
            s => s.ScanByTagAsync(assetTag, userId, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetProduct_Found_ReturnsOkWithAssetScanDto()
    {
        var assetId = Guid.NewGuid();
        var scanResult = new AssetScanDto
        {
            Id = assetId,
            AssetTag = "AST-001",
            Name = "Laptop Dell",
            Status = "Active",
            CurrentRoomCode = "Room-A1-01"
        };

        _productServiceMock
            .Setup(s => s.GetProductByIdAsync(assetId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(scanResult);

        var result = await _controller.GetProduct(assetId, CancellationToken.None);

        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var envelope = okResult.Value.Should().BeOfType<MobileEnvelope<AssetScanDto>>().Subject;
        envelope.Success.Should().BeTrue();
        envelope.Data!.Id.Should().Be(assetId);
        envelope.Data.AssetTag.Should().Be("AST-001");
    }

    [Fact]
    public async Task GetProduct_NotFound_ReturnsOkWithFailure()
    {
        _productServiceMock
            .Setup(s => s.GetProductByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((AssetScanDto?)null);

        var result = await _controller.GetProduct(Guid.NewGuid(), CancellationToken.None);

        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var envelope = okResult.Value.Should().BeOfType<MobileEnvelope<AssetScanDto>>().Subject;
        envelope.Success.Should().BeFalse();
        envelope.Message.Should().NotBeNullOrEmpty();
        envelope.Data.Should().BeNull();
    }

    [Fact]
    public async Task GetScanHistory_ReturnsOkWithEnvelope()
    {
        var history = new List<ScanHistoryEntryDto>
        {
            new()
            {
                Id = Guid.NewGuid(),
                AssetTag = "AST-001",
                AssetName = "Laptop Dell",
                Location = "Room: Room-A1-01",
                ScannedAt = new DateTime(2026, 5, 14, 10, 0, 0, DateTimeKind.Utc),
                ScannedByName = "John Doe"
            }
        };

        _productServiceMock
            .Setup(s => s.GetScanHistoryAsync(
                It.IsAny<DateTime?>(), It.IsAny<DateTime?>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(history);

        var result = await _controller.GetScanHistory(null, null, CancellationToken.None);

        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var envelope = okResult.Value.Should().BeOfType<MobileEnvelope<List<ScanHistoryEntryDto>>>().Subject;
        envelope.Success.Should().BeTrue();
        envelope.Data.Should().HaveCount(1);
        envelope.Data![0].AssetTag.Should().Be("AST-001");
    }

    [Fact]
    public async Task GetScanHistory_Empty_ReturnsOkWithEmptyList()
    {
        _productServiceMock
            .Setup(s => s.GetScanHistoryAsync(
                It.IsAny<DateTime?>(), It.IsAny<DateTime?>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ScanHistoryEntryDto>());

        var result = await _controller.GetScanHistory(null, null, CancellationToken.None);

        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var envelope = okResult.Value.Should().BeOfType<MobileEnvelope<List<ScanHistoryEntryDto>>>().Subject;
        envelope.Success.Should().BeTrue();
        envelope.Data.Should().BeEmpty();
    }

    [Fact]
    public async Task GetScanHistory_WithDateRange_PassesThrough()
    {
        var from = new DateTime(2026, 5, 1, 0, 0, 0, DateTimeKind.Utc);
        var to = new DateTime(2026, 5, 14, 23, 59, 59, DateTimeKind.Utc);

        _productServiceMock
            .Setup(s => s.GetScanHistoryAsync(from, to, It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ScanHistoryEntryDto>());

        await _controller.GetScanHistory(from, to, CancellationToken.None);

        _productServiceMock.Verify(
            s => s.GetScanHistoryAsync(from, to, It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    // ──────────────────────────────────────────────
    // Write endpoint tests (Group 4)
    // ──────────────────────────────────────────────

    [Fact]
    public async Task CreateProduct_ReturnsOkWithEnvelope()
    {
        var dto = new CreateProductRequestDto
        {
            Name = "New Laptop",
            Category = "Electronics",
            CurrentRoomCode = "Room-A1-01"
        };
        var scanDto = new AssetScanDto
        {
            Id = Guid.NewGuid(),
            AssetTag = "AST-099",
            Name = "New Laptop",
            Status = "Active",
            CurrentRoomCode = "Room-A1-01"
        };

        _writeServiceMock
            .Setup(s => s.CreateProductAsync(dto, _defaultUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(scanDto);

        var result = await _controller.CreateProductJson(dto, CancellationToken.None);

        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var envelope = okResult.Value.Should().BeOfType<MobileEnvelope<AssetScanDto>>().Subject;
        envelope.Success.Should().BeTrue();
        envelope.Data.Should().NotBeNull();
        envelope.Data!.AssetTag.Should().Be("AST-099");
        envelope.Data.Name.Should().Be("New Laptop");
    }

    [Fact]
    public async Task CreateProduct_ServiceReturnsNull_ReturnsFailure()
    {
        var dto = new CreateProductRequestDto
        {
            Name = "New Laptop",
            Category = "Electronics",
            CurrentRoomCode = "Room-A1-01"
        };

        _writeServiceMock
            .Setup(s => s.CreateProductAsync(dto, _defaultUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((AssetScanDto?)null);

        var result = await _controller.CreateProductJson(dto, CancellationToken.None);

        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var envelope = okResult.Value.Should().BeOfType<MobileEnvelope<AssetScanDto>>().Subject;
        envelope.Success.Should().BeFalse();
        envelope.Message.Should().NotBeNullOrEmpty();
        envelope.Data.Should().BeNull();
    }

    [Fact]
    public async Task UpdateProductStatus_ReturnsOkWithEnvelope()
    {
        var assetId = Guid.NewGuid();
        var dto = new UpdateStatusRequestDto { Status = "Maintenance" };
        var scanDto = new AssetScanDto
        {
            Id = assetId,
            AssetTag = "AST-001",
            Name = "Laptop Dell",
            Status = "Maintenance",
            CurrentRoomCode = "Room-A1-01"
        };

        _writeServiceMock
            .Setup(s => s.UpdateProductStatusAsync(assetId, dto.Status, _defaultUserId, It.IsAny<CancellationToken>(), It.IsAny<string?>()))
            .ReturnsAsync(scanDto);

        var result = await _controller.UpdateProductStatus(assetId, dto, CancellationToken.None);

        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var envelope = okResult.Value.Should().BeOfType<MobileEnvelope<AssetScanDto>>().Subject;
        envelope.Success.Should().BeTrue();
        envelope.Data.Should().NotBeNull();
        envelope.Data!.Status.Should().Be("Maintenance");
    }

    [Fact]
    public async Task UpdateProductStatus_NotFound_ReturnsFailure()
    {
        var assetId = Guid.NewGuid();
        var dto = new UpdateStatusRequestDto { Status = "Maintenance" };

        _writeServiceMock
            .Setup(s => s.UpdateProductStatusAsync(assetId, dto.Status, _defaultUserId, It.IsAny<CancellationToken>(), It.IsAny<string?>()))
            .ReturnsAsync((AssetScanDto?)null);

        var result = await _controller.UpdateProductStatus(assetId, dto, CancellationToken.None);

        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var envelope = okResult.Value.Should().BeOfType<MobileEnvelope<AssetScanDto>>().Subject;
        envelope.Success.Should().BeFalse();
        envelope.Message.Should().NotBeNullOrEmpty();
        envelope.Data.Should().BeNull();
    }

    [Fact]
    public async Task MoveProduct_ReturnsOkWithEnvelope()
    {
        var assetId = Guid.NewGuid();
        var dto = new MoveProductRequestDto { RoomId = "Room-B2-05" };
        var scanDto = new AssetScanDto
        {
            Id = assetId,
            AssetTag = "AST-001",
            Name = "Laptop Dell",
            Status = "Active",
            CurrentRoomCode = "Room-B2-05"
        };

        _writeServiceMock
            .Setup(s => s.MoveProductAsync(assetId, dto.RoomId, _defaultUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((scanDto, (string?)null));

        var result = await _controller.MoveProduct(assetId, dto, CancellationToken.None);

        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var envelope = okResult.Value.Should().BeOfType<MobileEnvelope<AssetScanDto>>().Subject;
        envelope.Success.Should().BeTrue();
        envelope.Data.Should().NotBeNull();
        envelope.Data!.CurrentRoomCode.Should().Be("Room-B2-05");
    }

    [Fact]
    public async Task MoveProduct_NotFound_ReturnsFailure()
    {
        var assetId = Guid.NewGuid();
        var dto = new MoveProductRequestDto { RoomId = "Room-B2-05" };

        _writeServiceMock
            .Setup(s => s.MoveProductAsync(assetId, dto.RoomId, _defaultUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(((AssetScanDto?)null, (string?)"Product not found"));

        var result = await _controller.MoveProduct(assetId, dto, CancellationToken.None);

        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var envelope = okResult.Value.Should().BeOfType<MobileEnvelope<AssetScanDto>>().Subject;
        envelope.Success.Should().BeFalse();
        envelope.Message.Should().NotBeNullOrEmpty();
        envelope.Data.Should().BeNull();
    }

    [Fact]
    public async Task DeleteProduct_ReturnsOkWithEnvelope()
    {
        var assetId = Guid.NewGuid();

        _writeServiceMock
            .Setup(s => s.DeleteProductAsync(assetId, _defaultUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((true, (string?)null));

        var result = await _controller.DeleteProduct(assetId, CancellationToken.None);

        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var envelope = okResult.Value.Should().BeOfType<MobileEnvelope>().Subject;
        envelope.Success.Should().BeTrue();
        envelope.Message.Should().Be("Product deleted");
    }

    [Fact]
    public async Task DeleteProduct_NotFound_ReturnsFailure()
    {
        var assetId = Guid.NewGuid();

        _writeServiceMock
            .Setup(s => s.DeleteProductAsync(assetId, _defaultUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((false, (string?)"Asset not retired"));

        var result = await _controller.DeleteProduct(assetId, CancellationToken.None);

        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var envelope = okResult.Value.Should().BeOfType<MobileEnvelope>().Subject;
        envelope.Success.Should().BeFalse();
        envelope.Message.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task CreateProduct_CallsServiceWithUserId()
    {
        var userId = Guid.NewGuid();
        var dto = new CreateProductRequestDto
        {
            Name = "New Laptop",
            Category = "Electronics",
            CurrentRoomCode = "Room-A1-01"
        };

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, userId.ToString())
        };
        var identity = new ClaimsIdentity(claims, "Test");
        var principal = new ClaimsPrincipal(identity);
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = principal }
        };

        _writeServiceMock
            .Setup(s => s.CreateProductAsync(dto, userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new AssetScanDto());

        await _controller.CreateProductJson(dto, CancellationToken.None);

        _writeServiceMock.Verify(
            s => s.CreateProductAsync(dto, userId, It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
