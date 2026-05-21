using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using SmartInventory.Application.Asset.Common;
using SmartInventory.Application.Asset.DTOs;
using SmartInventory.Application.Asset.Filters;
using SmartInventory.Application.Asset.Interfaces;
using SmartInventory.Application.Mobile.Products.DTOs;
using SmartInventory.Application.Mobile.Products.Interfaces;
using SmartInventory.Application.Mobile.Products.Services;
using SmartInventory.Domain.Asset.Enums;
using Xunit;

namespace SmartInventory.Application.Tests.Mobile.Products.Services;

public class MobileProductServiceTests
{
    private readonly Mock<IAssetService> _assetServiceMock;
    private readonly Mock<IActivityLogService> _activityLogServiceMock;
    private readonly IMobileProductService _service;

    public MobileProductServiceTests()
    {
        _assetServiceMock = new Mock<IAssetService>();
        _activityLogServiceMock = new Mock<IActivityLogService>();

        _service = new MobileProductService(
            _assetServiceMock.Object,
            _activityLogServiceMock.Object);
    }

    #region GetProductsAsync

    [Fact]
    public async Task GetProductsAsync_ReturnsMappedPagedResult()
    {
        var assets = new List<AssetDto>
        {
            new()
            {
                Id = Guid.NewGuid(),
                AssetTag = "AST-001",
                Name = "Laptop Dell",
                Status = AssetStatus.Active,
                CurrentRoomCode = "Room-A1-01",
                Category = "Electronics",
                LastSeen = new DateTime(2026, 5, 14, 10, 0, 0, DateTimeKind.Utc),
                DetectedRoomCode = "Room-B1-01"
            },
            new()
            {
                Id = Guid.NewGuid(),
                AssetTag = "AST-002",
                Name = "Server HP",
                Status = AssetStatus.Maintenance,
                CurrentRoomCode = "Room-A1-02",
                Category = "Servers",
                LastSeen = null,
                DetectedRoomCode = null
            }
        };

        var pagedResult = new PagedResult<AssetDto>
        {
            Items = assets,
            TotalCount = 2,
            Page = 1,
            PageSize = 20
        };

        _assetServiceMock
            .Setup(s => s.GetAssetsAsync(It.IsAny<AssetFilter>()))
            .ReturnsAsync(pagedResult);

        var filter = new MobileProductFilterDto();
        var result = await _service.GetProductsAsync(filter, CancellationToken.None);

        result.Should().NotBeNull();
        result.Items.Should().HaveCount(2);
        result.TotalCount.Should().Be(2);
        result.Page.Should().Be(1);
        result.PageSize.Should().Be(20);
        result.HasNextPage.Should().BeFalse();

        var first = result.Items[0];
        first.Id.Should().Be(assets[0].Id);
        first.AssetTag.Should().Be("AST-001");
        first.Name.Should().Be("Laptop Dell");
        first.Status.Should().Be("Active");
        first.CurrentRoomCode.Should().Be("Room-A1-01");
        first.Category.Should().Be("Electronics");
        first.LastSeen.Should().Be(assets[0].LastSeen);
        first.HasDiscrepancy.Should().BeTrue();

        var second = result.Items[1];
        second.AssetTag.Should().Be("AST-002");
        second.Status.Should().Be("Maintenance");
        second.LastSeen.Should().BeNull();
        second.HasDiscrepancy.Should().BeFalse();
    }

    [Fact]
    public async Task GetProductsAsync_FilterParams_MapsToAssetFilter()
    {
        _assetServiceMock
            .Setup(s => s.GetAssetsAsync(It.IsAny<AssetFilter>()))
            .ReturnsAsync(new PagedResult<AssetDto>());

        var filter = new MobileProductFilterDto
        {
            Search = "laptop",
            Status = "active",
            RoomCode = "Room-A1",
            Department = "IT",
            Type = "Electronics",
            Page = 2,
            Limit = 10
        };

        await _service.GetProductsAsync(filter, CancellationToken.None);

        _assetServiceMock.Verify(s => s.GetAssetsAsync(It.Is<AssetFilter>(f =>
            f.Search == "laptop" &&
            f.RoomCode == "Room-A1" &&
            f.Group == "IT" &&
            f.Category == "Electronics" &&
            f.Status == AssetStatus.Active &&
            f.Page == 2 &&
            f.PageSize == 10)), Times.Once);
    }

    [Fact]
    public async Task GetProductsAsync_InvalidStatus_FallsBackToNull()
    {
        _assetServiceMock
            .Setup(s => s.GetAssetsAsync(It.IsAny<AssetFilter>()))
            .ReturnsAsync(new PagedResult<AssetDto>());

        var filter = new MobileProductFilterDto
        {
            Status = "invalid_status_value"
        };

        await _service.GetProductsAsync(filter, CancellationToken.None);

        _assetServiceMock.Verify(s => s.GetAssetsAsync(It.Is<AssetFilter>(f =>
            f.Status == null)), Times.Once);
    }

    [Fact]
    public async Task GetProductsAsync_EmptyResults_ReturnsEmptyItemsList()
    {
        _assetServiceMock
            .Setup(s => s.GetAssetsAsync(It.IsAny<AssetFilter>()))
            .ReturnsAsync(new PagedResult<AssetDto>
            {
                Items = new List<AssetDto>(),
                TotalCount = 0,
                Page = 1,
                PageSize = 20
            });

        var filter = new MobileProductFilterDto();
        var result = await _service.GetProductsAsync(filter, CancellationToken.None);

        result.Items.Should().BeEmpty();
        result.TotalCount.Should().Be(0);
    }

    #endregion

    #region ScanByTagAsync

    [Fact]
    public async Task ScanByTagAsync_ValidTag_ReturnsAssetScanDtoAndLogsScan()
    {
        var assetId = Guid.NewGuid();
        var assetTag = "AST-001";
        var userId = Guid.NewGuid();
        var asset = new AssetDto
        {
            Id = assetId,
            AssetTag = assetTag,
            Name = "Laptop Dell",
            Status = AssetStatus.Active,
            CurrentRoomCode = "Room-A1-01"
        };

        _assetServiceMock
            .Setup(s => s.GetAssetByTagAsync(assetTag))
            .ReturnsAsync(asset);

        var result = await _service.ScanByTagAsync(assetTag, userId, CancellationToken.None);

        result.Should().NotBeNull();
        result!.Id.Should().Be(assetId);
        result.AssetTag.Should().Be("AST-001");
        result.Name.Should().Be("Laptop Dell");
        result.Status.Should().Be("Active");
        result.CurrentRoomCode.Should().Be("Room-A1-01");

        _activityLogServiceMock.Verify(s => s.TrackFacilityChangeAsync(
            "Scanned",
            "Asset",
            assetTag,
            "Laptop Dell",
            "Room: Room-A1-01",
            userId), Times.Once);
    }

    [Fact]
    public async Task ScanByTagAsync_NonExistentTag_ReturnsNull()
    {
        _assetServiceMock
            .Setup(s => s.GetAssetByTagAsync(It.IsAny<string>()))
            .ReturnsAsync((AssetDto?)null);

        var result = await _service.ScanByTagAsync("NONEXISTENT", Guid.NewGuid(), CancellationToken.None);

        result.Should().BeNull();
    }

    [Fact]
    public async Task ScanByTagAsync_EmptyTag_ReturnsNull()
    {
        _assetServiceMock
            .Setup(s => s.GetAssetByTagAsync(It.IsAny<string>()))
            .ReturnsAsync((AssetDto?)null);

        var result = await _service.ScanByTagAsync("", Guid.NewGuid(), CancellationToken.None);

        result.Should().BeNull();
    }

    #endregion

    #region GetProductByIdAsync

    [Fact]
    public async Task GetProductByIdAsync_ValidId_ReturnsAssetScanDto()
    {
        var assetId = Guid.NewGuid();
        var asset = new AssetDto
        {
            Id = assetId,
            AssetTag = "AST-001",
            Name = "Laptop Dell",
            Status = AssetStatus.Active,
            CurrentRoomCode = "Room-A1-01"
        };

        _assetServiceMock
            .Setup(s => s.GetAssetByIdAsync(assetId))
            .ReturnsAsync(asset);

        var result = await _service.GetProductByIdAsync(assetId, CancellationToken.None);

        result.Should().NotBeNull();
        result!.Id.Should().Be(assetId);
        result.AssetTag.Should().Be("AST-001");
        result.Name.Should().Be("Laptop Dell");
        result.Status.Should().Be("Active");
        result.CurrentRoomCode.Should().Be("Room-A1-01");
    }

    [Fact]
    public async Task GetProductByIdAsync_NonExistentId_ReturnsNull()
    {
        _assetServiceMock
            .Setup(s => s.GetAssetByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync((AssetDto?)null);

        var result = await _service.GetProductByIdAsync(Guid.NewGuid(), CancellationToken.None);

        result.Should().BeNull();
    }

    #endregion

    #region GetScanHistoryAsync

    [Fact]
    public async Task GetScanHistoryAsync_ReturnsOnlyScannedEntries()
    {
        var userId = Guid.NewGuid();
        var activityLogs = new List<ActivityLogDto>
        {
            new()
            {
                Id = Guid.NewGuid(),
                Action = "Scanned",
                AssetTag = "AST-001",
                AssetName = "Laptop Dell",
                Details = "Room: Room-A1-01",
                ChangedBy = userId,
                ChangedByName = "John Doe",
                ChangedAt = new DateTime(2026, 5, 14, 10, 30, 0, DateTimeKind.Utc)
            },
            new()
            {
                Id = Guid.NewGuid(),
                Action = "Moved",
                AssetTag = "AST-002",
                AssetName = "Server HP"
            },
            new()
            {
                Id = Guid.NewGuid(),
                Action = "Scanned",
                AssetTag = "AST-003",
                AssetName = "Monitor LG",
                Details = "Room: Room-B2-01",
                ChangedBy = userId,
                ChangedByName = "Jane Doe",
                ChangedAt = new DateTime(2026, 5, 14, 11, 0, 0, DateTimeKind.Utc)
            }
        };

        _activityLogServiceMock
            .Setup(s => s.GetAllActivityLogsAsync(It.IsAny<DateTime?>(), It.IsAny<DateTime?>()))
            .ReturnsAsync(activityLogs);

        var result = await _service.GetScanHistoryAsync(null, null, userId, CancellationToken.None);

        result.Should().HaveCount(2);
        result.Should().AllSatisfy(e => e.AssetTag.Should().NotBeNullOrEmpty());

        var firstScan = result.First();
        firstScan.AssetTag.Should().Be("AST-003");
        firstScan.AssetName.Should().Be("Monitor LG");
        firstScan.Location.Should().Be("Room: Room-B2-01");
        firstScan.ScannedByName.Should().Be("Jane Doe");

        var secondScan = result.Last();
        secondScan.AssetTag.Should().Be("AST-001");
        secondScan.AssetName.Should().Be("Laptop Dell");
    }

    [Fact]
    public async Task GetScanHistoryAsync_MapsAllPropertiesCorrectly()
    {
        var userId = Guid.NewGuid();
        var scanId = Guid.NewGuid();
        var scannedAt = new DateTime(2026, 5, 14, 10, 0, 0, DateTimeKind.Utc);
        var activityLogs = new List<ActivityLogDto>
        {
            new()
            {
                Id = scanId,
                Action = "Scanned",
                AssetTag = "AST-001",
                AssetName = "Laptop Dell",
                Details = "Room: A1-01",
                ChangedBy = userId,
                ChangedByName = "John Doe",
                ChangedAt = scannedAt
            }
        };

        _activityLogServiceMock
            .Setup(s => s.GetAllActivityLogsAsync(It.IsAny<DateTime?>(), It.IsAny<DateTime?>()))
            .ReturnsAsync(activityLogs);

        var result = await _service.GetScanHistoryAsync(null, null, userId, CancellationToken.None);

        result.Should().HaveCount(1);
        var entry = result.Single();
        entry.Id.Should().Be(scanId);
        entry.AssetTag.Should().Be("AST-001");
        entry.AssetName.Should().Be("Laptop Dell");
        entry.Location.Should().Be("Room: A1-01");
        entry.ScannedAt.Should().Be(scannedAt);
        entry.ScannedByName.Should().Be("John Doe");
    }

    [Fact]
    public async Task GetScanHistoryAsync_WithDateRange_PassesThrough()
    {
        var userId = Guid.NewGuid();
        var from = new DateTime(2026, 5, 1, 0, 0, 0, DateTimeKind.Utc);
        var to = new DateTime(2026, 5, 14, 23, 59, 59, DateTimeKind.Utc);

        _activityLogServiceMock
            .Setup(s => s.GetAllActivityLogsAsync(from, to))
            .ReturnsAsync(new List<ActivityLogDto>());

        await _service.GetScanHistoryAsync(from, to, userId, CancellationToken.None);

        _activityLogServiceMock.Verify(
            s => s.GetAllActivityLogsAsync(from, to), Times.Once);
    }

    [Fact]
    public async Task GetScanHistoryAsync_NoScans_ReturnsEmptyList()
    {
        var userId = Guid.NewGuid();
        _activityLogServiceMock
            .Setup(s => s.GetAllActivityLogsAsync(It.IsAny<DateTime?>(), It.IsAny<DateTime?>()))
            .ReturnsAsync(new List<ActivityLogDto>());

        var result = await _service.GetScanHistoryAsync(null, null, userId, CancellationToken.None);

        result.Should().BeEmpty();
    }

    #endregion
}
