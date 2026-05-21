using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using SmartInventory.Application.Asset.DTOs.Reports;
using SmartInventory.Application.Asset.Interfaces;
using SmartInventory.Application.Asset.Services;
using SmartInventory.Application.Auth.Interfaces;
using SmartInventory.Application.Location.Interfaces;
using SmartInventory.Domain.Asset.Enums;
using SmartInventory.Domain.Auth.Enums;
using SmartInventory.Domain.Location.Entities;
using Xunit;
using AssetEntity = SmartInventory.Domain.Asset.Entities.Asset;

namespace SmartInventory.Application.Tests;

public class ReportingServiceNewReportsTests
{
    private readonly Mock<IAssetRepository> _assetRepositoryMock;
    private readonly Mock<IAssetHistoryService> _historyServiceMock;
    private readonly Mock<IActivityLogService> _activityLogServiceMock;
    private readonly Mock<IAuthRepository> _authRepositoryMock;
    private readonly Mock<ILocationRepository> _locationRepositoryMock;
    private readonly ReportingService _service;

    public ReportingServiceNewReportsTests()
    {
        _assetRepositoryMock = new Mock<IAssetRepository>();
        _historyServiceMock = new Mock<IAssetHistoryService>();
        _activityLogServiceMock = new Mock<IActivityLogService>();
        _authRepositoryMock = new Mock<IAuthRepository>();
        _locationRepositoryMock = new Mock<ILocationRepository>();

        _locationRepositoryMock.Setup(r => r.GetFullHierarchyAsync())
            .ReturnsAsync(new List<Site>());

        // Set up all new aggregation methods to return empty lists by default
        // to prevent ArgumentNullException on unmocked methods.
        _assetRepositoryMock.Setup(r => r.GetStatusCountsAsync())
            .ReturnsAsync(new List<StatusCountDto>());
        _assetRepositoryMock.Setup(r => r.GetCategoryCountsAsync())
            .ReturnsAsync(new List<CategoryCountDto>());
        _assetRepositoryMock.Setup(r => r.GetLocationCountsAsync())
            .ReturnsAsync(new List<LocationCountDto>());
        _assetRepositoryMock.Setup(r => r.GetRoomAssetCountsAsync())
            .ReturnsAsync(new List<RoomAssetCountDto>());
        _assetRepositoryMock.Setup(r => r.GetRoomCategoriesAsync())
            .ReturnsAsync(new List<RoomCategoryDto>());
        _assetRepositoryMock.Setup(r => r.GetFilteredListAsync(It.IsAny<Asset.Filters.AssetFilter>()))
            .ReturnsAsync(new List<AssetEntity>());
        _assetRepositoryMock.Setup(r => r.GetAssetsWithMaintenanceAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .ReturnsAsync(new List<AssetEntity>());
        _assetRepositoryMock.Setup(r => r.GetAssetsByStatusAsync(It.IsAny<ICollection<AssetStatus>>()))
            .ReturnsAsync(new List<AssetEntity>());
        _assetRepositoryMock.Setup(r => r.GetCategoryStatusBreakdownAsync())
            .ReturnsAsync(new List<CategoryStatusBreakdownDto>());

        _service = new ReportingService(
            _assetRepositoryMock.Object,
            _historyServiceMock.Object,
            _activityLogServiceMock.Object,
            _authRepositoryMock.Object,
            _locationRepositoryMock.Object);
    }

    private void SetupLocationHierarchy()
    {
        var site = new Site
        {
            Id = Guid.NewGuid(),
            Code = "SITE-01",
            Name = "Main Site",
            Zones = new List<Zone>
            {
                new()
                {
                    Id = Guid.NewGuid(),
                    Code = "ZONE-INF",
                    Name = "Informatique",
                    Buildings = new List<Building>
                    {
                        new()
                        {
                            Id = Guid.NewGuid(),
                            Code = "BLD-INF",
                            Name = "Informatique Building",
                            Floors = new List<Floor>
                            {
                                new()
                                {
                                    Id = Guid.NewGuid(),
                                    Level = 0,
                                    Rooms = new List<Room>
                                    {
                                        new() { Id = Guid.NewGuid(), Code = "LI1", Name = "Lab Info 1" },
                                        new() { Id = Guid.NewGuid(), Code = "LI2", Name = "Lab Info 2" },
                                        new() { Id = Guid.NewGuid(), Code = "STOCK1", Name = "Stock Room" },
                                    }
                                }
                            }
                        }
                    }
                },
                new()
                {
                    Id = Guid.NewGuid(),
                    Code = "ZONE-GEST",
                    Name = "Gestion",
                    Buildings = new List<Building>
                    {
                        new()
                        {
                            Id = Guid.NewGuid(),
                            Code = "BLD-GEST",
                            Name = "Gestion Building",
                            Floors = new List<Floor>
                            {
                                new()
                                {
                                    Id = Guid.NewGuid(),
                                    Level = 0,
                                    Rooms = new List<Room>
                                    {
                                        new() { Id = Guid.NewGuid(), Code = "GEST1", Name = "Office 1" },
                                    }
                                }
                            }
                        }
                    }
                }
            }
        };

        _locationRepositoryMock.Setup(r => r.GetFullHierarchyAsync())
            .ReturnsAsync(new List<Site> { site });
    }

    private List<AssetEntity> CreateTestAssets()
    {
        var now = DateTime.UtcNow;
        return new List<AssetEntity>
        {
            new() { Id = Guid.NewGuid(), AssetTag = "AST-001", Name = "Dell Laptop", Category = "Computer",
                Status = AssetStatus.Active, CurrentRoomCode = "LI1", MaintenanceDueDate = now.AddDays(15) },
            new() { Id = Guid.NewGuid(), AssetTag = "AST-002", Name = "HP Monitor", Category = "Display",
                Status = AssetStatus.Active, CurrentRoomCode = "LI2", MaintenanceDueDate = now.AddDays(45) },
            new() { Id = Guid.NewGuid(), AssetTag = "AST-003", Name = "Canon Printer", Category = "Printer/Scanner",
                Status = AssetStatus.Maintenance, CurrentRoomCode = "MEC1", MaintenanceDueDate = now.AddDays(-5) },
            new() { Id = Guid.NewGuid(), AssetTag = "AST-004", Name = "Broken Scanner", Category = "Peripheral",
                Status = AssetStatus.CriticalIssue, CurrentRoomCode = "LI1", LastSeen = now.AddDays(-1) },
            new() { Id = Guid.NewGuid(), AssetTag = "AST-005", Name = "Lost Router", Category = "Network Device",
                Status = AssetStatus.Lost, CurrentRoomCode = "GEST1", LastSeen = now.AddDays(-30) },
            new() { Id = Guid.NewGuid(), AssetTag = "AST-006", Name = "Spare Monitor", Category = "Display",
                Status = AssetStatus.InStock, CurrentRoomCode = "STOCK1", MaintenanceDueDate = now.AddDays(90) },
            new() { Id = Guid.NewGuid(), AssetTag = "AST-007", Name = "Old Server", Category = "Server",
                Status = AssetStatus.Retired, CurrentRoomCode = "STORAGE1" },
            new() { Id = Guid.NewGuid(), AssetTag = "AST-008", Name = "Dell Server", Category = "Server",
                Status = AssetStatus.Active, CurrentRoomCode = "LI2", MaintenanceDueDate = now.AddDays(-60) },
        };
    }

    [Fact]
    public async Task GetMaintenanceForecast_ReturnsAssetsDueWithinDays()
    {
        var assets = CreateTestAssets();
        // Return only AST-001 (due ~15 days out) — the only asset in a 30-day window.
        var forecastAssets = assets.Where(a => a.AssetTag == "AST-001").ToList();
        _assetRepositoryMock.Setup(r => r.GetAssetsWithMaintenanceAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .ReturnsAsync(forecastAssets);

        var result = await _service.GetMaintenanceForecastAsync(30);

        result.Should().NotBeEmpty();
        result.Should().AllSatisfy(a => a.DaysUntilDue.Should().BeLessThanOrEqualTo(30));
        result.Should().AllSatisfy(a => a.DaysUntilDue.Should().BeGreaterThanOrEqualTo(0));
    }

    [Fact]
    public async Task GetMaintenanceForecast_AssetsOutsideWindow_AreExcluded()
    {
        var assets = CreateTestAssets();
        // Return only AST-001 (due ~15 days) — AST-006 (90 days out) should not appear.
        var forecastAssets = assets.Where(a => a.AssetTag == "AST-001").ToList();
        _assetRepositoryMock.Setup(r => r.GetAssetsWithMaintenanceAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .ReturnsAsync(forecastAssets);

        var result = await _service.GetMaintenanceForecastAsync(30);

        result.Any(a => a.AssetTag == "AST-006").Should().BeFalse();
    }

    [Fact]
    public async Task GetOverdueMaintenance_ReturnsPastDueAssets()
    {
        var assets = CreateTestAssets();
        // AST-003 (due 5 days ago) and AST-008 (due 60 days ago) are overdue.
        var overdueAssets = assets.Where(a => a.AssetTag is "AST-003" or "AST-008").ToList();
        _assetRepositoryMock.Setup(r => r.GetAssetsWithMaintenanceAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .ReturnsAsync(overdueAssets);

        var result = await _service.GetOverdueMaintenanceAsync();

        result.Should().NotBeEmpty();
        result.Should().AllSatisfy(a => a.DaysOverdue.Should().BeGreaterThan(0));
    }

    [Fact]
    public async Task GetCriticalIssues_ReturnsCriticalAndLostAssets()
    {
        var assets = CreateTestAssets();
        var expected = assets.Where(a => a.Status == AssetStatus.CriticalIssue || a.Status == AssetStatus.Lost).ToList();

        _assetRepositoryMock.Setup(r => r.GetAssetsByStatusAsync(It.Is<ICollection<AssetStatus>>(
            s => s.Contains(AssetStatus.CriticalIssue) && s.Contains(AssetStatus.Lost))))
            .ReturnsAsync(expected);

        var result = await _service.GetCriticalIssuesAsync();

        result.Should().HaveCount(2);
        result.Any(a => a.AssetTag == "AST-004").Should().BeTrue();
        result.Any(a => a.AssetTag == "AST-005").Should().BeTrue();
    }

    [Fact]
    public async Task GetStatusSummary_ReturnsCountsForAllStatuses()
    {
        var assets = CreateTestAssets();
        var statusCounts = assets.GroupBy(a => a.Status)
            .Select(g => new StatusCountDto { Status = g.Key, Count = g.Count() })
            .ToList();

        _assetRepositoryMock.Setup(r => r.GetStatusCountsAsync())
            .ReturnsAsync(statusCounts);

        var result = await _service.GetStatusSummaryAsync();

        result.Should().HaveCount(6);
        result.First(s => s.Status == "Active").Count.Should().Be(3);
        result.First(s => s.Status == "InStock").Count.Should().Be(1);
        result.Sum(s => s.Count).Should().Be(assets.Count);
    }

    [Fact]
    public async Task GetZoneInventory_ReturnsAllRoomsWithCounts()
    {
        var assets = CreateTestAssets();
        var roomCounts = assets.Where(a => !string.IsNullOrEmpty(a.CurrentRoomCode))
            .GroupBy(a => new { a.CurrentRoomCode, a.Status })
            .Select(g => new RoomAssetCountDto { RoomCode = g.Key.CurrentRoomCode, Status = g.Key.Status, Count = g.Count() })
            .ToList();

        _assetRepositoryMock.Setup(r => r.GetRoomAssetCountsAsync())
            .ReturnsAsync(roomCounts);
        SetupLocationHierarchy();

        var result = await _service.GetZoneInventoryAsync();

        result.Should().NotBeEmpty();
        result.Should().AllSatisfy(z => z.RoomCode.Should().NotBeNullOrEmpty());
    }

    [Fact]
    public async Task GetBuildingStocktake_ReturnsAllBuildings()
    {
        var assets = CreateTestAssets();
        var roomCounts = assets.Where(a => !string.IsNullOrEmpty(a.CurrentRoomCode))
            .GroupBy(a => new { a.CurrentRoomCode, a.Status })
            .Select(g => new RoomAssetCountDto { RoomCode = g.Key.CurrentRoomCode, Status = g.Key.Status, Count = g.Count() })
            .ToList();
        var roomCategories = assets.Where(a => !string.IsNullOrEmpty(a.CurrentRoomCode))
            .Select(a => new { a.CurrentRoomCode, a.Category })
            .Distinct()
            .Select(x => new RoomCategoryDto { RoomCode = x.CurrentRoomCode, Category = x.Category })
            .ToList();

        _assetRepositoryMock.Setup(r => r.GetRoomAssetCountsAsync())
            .ReturnsAsync(roomCounts);
        _assetRepositoryMock.Setup(r => r.GetRoomCategoriesAsync())
            .ReturnsAsync(roomCategories);
        SetupLocationHierarchy();

        var result = await _service.GetBuildingStocktakeAsync();

        result.Should().NotBeEmpty();
        result.Should().AllSatisfy(b => b.FloorCount.Should().BeGreaterThan(0));
    }

    [Fact]
    public async Task GetEmptyRooms_ReturnsRoomsBelowThreshold()
    {
        var assets = CreateTestAssets();
        var roomCounts = assets.Where(a => !string.IsNullOrEmpty(a.CurrentRoomCode))
            .GroupBy(a => new { a.CurrentRoomCode, a.Status })
            .Select(g => new RoomAssetCountDto { RoomCode = g.Key.CurrentRoomCode, Status = g.Key.Status, Count = g.Count() })
            .ToList();

        _assetRepositoryMock.Setup(r => r.GetRoomAssetCountsAsync())
            .ReturnsAsync(roomCounts);
        SetupLocationHierarchy();

        var result = await _service.GetEmptyRoomsAsync(1);

        result.Should().NotBeEmpty();
        result.Should().AllSatisfy(r => r.AssetCount.Should().BeLessThanOrEqualTo(1));
    }

    [Fact]
    public async Task GetLocationDiscrepancies_ReturnsMismatchedAssets()
    {
        var assets = CreateTestAssets();
        assets[0].DetectedRoomCode = "LI2";
        _assetRepositoryMock.Setup(r => r.GetDiscrepantAssetsAsync())
            .ReturnsAsync(new List<AssetEntity> { assets[0] });

        var result = await _service.GetLocationDiscrepanciesAsync();

        result.Should().ContainSingle();
        result[0].AssetTag.Should().Be("AST-001");
    }

    [Fact]
    public async Task GetCategoryStocktake_ReturnsGroupedCounts()
    {
        var assets = CreateTestAssets();
        var breakdown = assets
            .GroupBy(a => new { a.Category, a.Status })
            .Select(g => new CategoryStatusBreakdownDto
            {
                Category = g.Key.Category,
                Status = g.Key.Status,
                Count = g.Count()
            })
            .ToList();

        _assetRepositoryMock.Setup(r => r.GetCategoryStatusBreakdownAsync())
            .ReturnsAsync(breakdown);

        var result = await _service.GetCategoryStocktakeAsync();

        result.Should().NotBeEmpty();
        result.Should().AllSatisfy(c => c.Count.Should().BeGreaterThan(0));
        result.Should().AllSatisfy(c => c.StatusBreakdown.Should().NotBeEmpty());
        result.Sum(c => c.Count).Should().Be(assets.Count);
    }

    [Fact]
    public async Task GetRoomAudit_ReturnsNullForUnknownRoom()
    {
        var assets = CreateTestAssets();

        _assetRepositoryMock.Setup(r => r.GetFilteredListAsync(It.Is<Asset.Filters.AssetFilter>(
            f => f.RoomCode == "NONEXISTENT")))
            .ReturnsAsync(new List<AssetEntity>());

        var result = await _service.GetRoomAuditAsync("NONEXISTENT");

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetRoomAudit_ReturnsAssetsInRoom()
    {
        var assets = CreateTestAssets();
        var li1Assets = assets.Where(a => a.CurrentRoomCode == "LI1").ToList();

        _assetRepositoryMock.Setup(r => r.GetFilteredListAsync(It.Is<Asset.Filters.AssetFilter>(
            f => f.RoomCode == "LI1")))
            .ReturnsAsync(li1Assets);
        SetupLocationHierarchy();

        var result = await _service.GetRoomAuditAsync("LI1");

        result.Should().NotBeNull();
        result!.Assets.Should().HaveCount(2);
        result.Assets.Should().AllSatisfy(a => a.RfidTagId.Should().BeNull());
    }

    [Fact]
    public async Task GetDepartmentReport_ReturnsZoneKPIs()
    {
        var assets = CreateTestAssets();
        var roomCounts = assets.Where(a => !string.IsNullOrEmpty(a.CurrentRoomCode))
            .GroupBy(a => new { a.CurrentRoomCode, a.Status })
            .Select(g => new RoomAssetCountDto { RoomCode = g.Key.CurrentRoomCode, Status = g.Key.Status, Count = g.Count() })
            .ToList();

        _assetRepositoryMock.Setup(r => r.GetRoomAssetCountsAsync())
            .ReturnsAsync(roomCounts);
        SetupLocationHierarchy();

        var result = await _service.GetDepartmentReportsAsync();

        result.Should().NotBeEmpty();
        result.Should().AllSatisfy(d => d.TotalAssets.Should().BeGreaterThan(0));
        result.Should().AllSatisfy(d => d.AvailabilityRate.Should().BeInRange(0, 100));
    }
}
