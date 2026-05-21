using FluentAssertions;
using Moq;
using SmartInventory.Application.Asset.DTOs;
using SmartInventory.Application.Asset.DTOs.Reports;
using SmartInventory.Application.Asset.Interfaces;
using SmartInventory.Application.Asset.Services;
using SmartInventory.Application.Location.DTOs;
using SmartInventory.Application.Location.Interfaces;
using SmartInventory.Application.Mobile.Lookup.DTOs;
using SmartInventory.Application.Mobile.Lookup.Interfaces;
using SmartInventory.Application.Mobile.Lookup.Services;
using SmartInventory.Domain.Asset.Enums;
using SmartInventory.Domain.Location.Entities;
using Xunit;

namespace SmartInventory.Application.Tests.Mobile.Lookup.Services;

public class MobileLookupServiceTests
{
    private readonly Mock<ILocationService> _locationServiceMock;
    private readonly Mock<ILocationRepository> _locationRepoMock;
    private readonly Mock<IReportingService> _reportingServiceMock;
    private readonly Mock<IAssetService> _assetServiceMock;
    private readonly CategoryService _categoryService;
    private readonly IMobileLookupService _service;

    public MobileLookupServiceTests()
    {
        _locationServiceMock = new Mock<ILocationService>();
        _locationRepoMock = new Mock<ILocationRepository>();
        _reportingServiceMock = new Mock<IReportingService>();
        _assetServiceMock = new Mock<IAssetService>();
        _categoryService = new CategoryService();

        _service = new MobileLookupService(
            _locationServiceMock.Object,
            _locationRepoMock.Object,
            _reportingServiceMock.Object,
            _assetServiceMock.Object,
            _categoryService);
    }

    #region GetCategoriesAsync

    [Fact]
    public async Task GetCategoriesAsync_ReturnsMappedCategoryList()
    {
        var result = await _service.GetCategoriesAsync();

        result.Should().NotBeEmpty();
        result.Should().AllSatisfy(c =>
        {
            c.Name.Should().NotBeNullOrEmpty();
            c.Group.Should().NotBeNullOrEmpty();
        });
    }

    #endregion

    #region GetDepartmentsAsync

    [Fact]
    public async Task GetDepartmentsAsync_FlattensHierarchyToZoneList()
    {
        var hierarchy = CreateTestHierarchy();

        _locationServiceMock
            .Setup(s => s.GetHierarchyAsync())
            .ReturnsAsync(hierarchy);

        var result = await _service.GetDepartmentsAsync();

        result.Should().HaveCount(2);

        var zoneA = result.Should().ContainSingle(d => d.Name == "Zone A").Subject;
        zoneA.Code.Should().Be("ZA");
        zoneA.RoomCount.Should().Be(2); // Building A1 Floor 1 has Room-A1-01, Room-A1-02

        var zoneB = result.Should().ContainSingle(d => d.Name == "Zone B").Subject;
        zoneB.Code.Should().Be("ZB");
        zoneB.RoomCount.Should().Be(1); // Building B1 Floor 1 has Room-B1-01
    }

    [Fact]
    public async Task GetDepartmentsAsync_EmptyHierarchy_ReturnsEmptyList()
    {
        var hierarchy = new HierarchyDto
        {
            Site = new SiteDto
            {
                Id = Guid.NewGuid(),
                Code = "SITE",
                Name = "Test Site",
                Zones = new List<ZoneDto>()
            }
        };

        _locationServiceMock
            .Setup(s => s.GetHierarchyAsync())
            .ReturnsAsync(hierarchy);

        var result = await _service.GetDepartmentsAsync();

        result.Should().BeEmpty();
    }

    #endregion

    #region GetRoomsByDepartmentAsync

    [Fact]
    public async Task GetRoomsByDepartmentAsync_ReturnsRoomsForZone()
    {
        var hierarchy = CreateTestHierarchy();
        var zoneId = hierarchy.Site.Zones[0].Id; // Zone A

        _locationServiceMock
            .Setup(s => s.GetHierarchyAsync())
            .ReturnsAsync(hierarchy);

        var result = await _service.GetRoomsByDepartmentAsync(zoneId);

        result.Should().HaveCount(2);
        result.Should().AllSatisfy(r => r.ZoneName.Should().Be("Zone A"));
        result.Should().AllSatisfy(r => r.BuildingName.Should().Be("Building A1"));
        result.Select(r => r.Code).Should().BeEquivalentTo(new[] { "Room-A1-01", "Room-A1-02" });
    }

    [Fact]
    public async Task GetRoomsByDepartmentAsync_UnknownZone_ReturnsEmptyList()
    {
        var hierarchy = CreateTestHierarchy();
        var unknownZoneId = Guid.NewGuid();

        _locationServiceMock
            .Setup(s => s.GetHierarchyAsync())
            .ReturnsAsync(hierarchy);

        var result = await _service.GetRoomsByDepartmentAsync(unknownZoneId);

        result.Should().BeEmpty();
    }

    #endregion

    #region GetRoomsByDepartmentCodeAsync

    [Fact]
    public async Task GetRoomsByDepartmentCodeAsync_ReturnsRoomsForKnownCode()
    {
        var zone = new Zone
        {
            Id = Guid.NewGuid(),
            Code = "ADM",
            Name = "Administration",
            Buildings = new List<Building>
            {
                new()
                {
                    Id = Guid.NewGuid(),
                    Code = "ADMIN",
                    Name = "Bâtiment Administratif",
                    Floors = new List<Floor>
                    {
                        new()
                        {
                            Id = Guid.NewGuid(),
                            Level = 0,
                            Description = "Rez-de-chaussée",
                            Rooms = new List<Room>
                            {
                                new() { Id = Guid.NewGuid(), Code = "ADM1", Name = "Bureau Directeur" },
                                new() { Id = Guid.NewGuid(), Code = "ADM2", Name = "Salle Réunion" }
                            }
                        }
                    }
                }
            }
        };

        _locationRepoMock
            .Setup(r => r.GetZoneByCodeAsync("ADM"))
            .ReturnsAsync(zone);

        var result = await _service.GetRoomsByDepartmentCodeAsync("ADM");

        result.Should().HaveCount(2);
        result.Should().AllSatisfy(r => r.ZoneName.Should().Be("Administration"));
        result.Should().AllSatisfy(r => r.BuildingName.Should().Be("Bâtiment Administratif"));
        result.Should().AllSatisfy(r => r.FloorLevel.Should().Be(0));
        result.Select(r => r.Code).Should().BeEquivalentTo(new[] { "ADM1", "ADM2" });
    }

    [Fact]
    public async Task GetRoomsByDepartmentCodeAsync_UnknownCode_ReturnsEmptyList()
    {
        _locationRepoMock
            .Setup(r => r.GetZoneByCodeAsync(It.IsAny<string>()))
            .ReturnsAsync((Zone?)null);

        var result = await _service.GetRoomsByDepartmentCodeAsync("UNKNOWN");

        result.Should().BeEmpty();
    }

    #endregion

    #region GetStatsAsync

    [Fact]
    public async Task GetStatsAsync_MapsStatusSummaryToFlatDto()
    {
        var statusSummary = new List<StatusSummaryDto>
        {
            new() { Status = "Active", Count = 42, Percentage = 60.0 },
            new() { Status = "Maintenance", Count = 3, Percentage = 4.3 },
            new() { Status = "CriticalIssue", Count = 1, Percentage = 1.4 },
            new() { Status = "InStock", Count = 15, Percentage = 21.4 },
            new() { Status = "Retired", Count = 5, Percentage = 7.1 },
            new() { Status = "Lost", Count = 4, Percentage = 5.7 }
        };

        _reportingServiceMock
            .Setup(s => s.GetStatusSummaryAsync())
            .ReturnsAsync(statusSummary);

        var result = await _service.GetStatsAsync();

        result.InStock.Should().Be(42);  // Active → InStock
        result.Maintenance.Should().Be(3);
        result.Critical.Should().Be(1);  // CriticalIssue → Critical
        result.Lost.Should().Be(4);
        result.Retired.Should().Be(5);
    }

    [Fact]
    public async Task GetStatsAsync_MissingStatuses_ReturnsZeroForMissing()
    {
        var statusSummary = new List<StatusSummaryDto>
        {
            new() { Status = "Active", Count = 10, Percentage = 100.0 }
        };

        _reportingServiceMock
            .Setup(s => s.GetStatusSummaryAsync())
            .ReturnsAsync(statusSummary);

        var result = await _service.GetStatsAsync();

        result.InStock.Should().Be(10);
        result.Maintenance.Should().Be(0);
        result.Critical.Should().Be(0);
        result.Lost.Should().Be(0);
        result.Retired.Should().Be(0);
    }

    #endregion

    #region GetMoveLogAsync

    [Fact]
    public async Task GetMoveLogAsync_ReturnsMoveAndStatusChangeActions()
    {
        var activityLog = new List<ActivityLogDto>
        {
            new()
            {
                Id = Guid.NewGuid(),
                AssetTag = "TAG-001",
                AssetName = "Laptop 1",
                Action = "Moved",
                OldValue = "Room-A",
                NewValue = "Room-B",
                ChangedByName = "John",
                ChangedAt = new DateTime(2026, 5, 14, 10, 0, 0, DateTimeKind.Utc)
            },
            new()
            {
                Id = Guid.NewGuid(),
                AssetTag = "TAG-002",
                AssetName = "Server 1",
                Action = "StatusChanged",
                OldValue = "Active",
                NewValue = "Maintenance",
                ChangedByName = "Jane",
                ChangedAt = new DateTime(2026, 5, 14, 11, 0, 0, DateTimeKind.Utc)
            }
        };

        _reportingServiceMock
            .Setup(s => s.GetActivityLogAsync(It.IsAny<DateTime?>(), It.IsAny<DateTime?>(), It.IsAny<Guid?>()))
            .ReturnsAsync(activityLog);

        var result = await _service.GetMoveLogAsync();

        result.Should().HaveCount(2);
        result.Should().ContainSingle(e => e.AssetTag == "TAG-001" && e.OldValue == "Room-A" && e.NewValue == "Room-B");
        result.Should().ContainSingle(e => e.AssetTag == "TAG-002" && e.OldValue == "Active" && e.NewValue == "Maintenance");
    }

    [Fact]
    public async Task GetMoveLogAsync_WithDateRange_PassesThroughToService()
    {
        var from = new DateTime(2026, 5, 1, 0, 0, 0, DateTimeKind.Utc);
        var to = new DateTime(2026, 5, 14, 23, 59, 59, DateTimeKind.Utc);

        _reportingServiceMock
            .Setup(s => s.GetActivityLogAsync(from, to, It.IsAny<Guid?>()))
            .ReturnsAsync(new List<ActivityLogDto>());

        var result = await _service.GetMoveLogAsync(from, to);

        result.Should().BeEmpty();
        _reportingServiceMock.Verify(
            s => s.GetActivityLogAsync(from, to, It.IsAny<Guid?>()),
            Times.Once);
    }

    #endregion

    #region CheckBarcodeAsync

    [Fact]
    public async Task CheckBarcodeAsync_ExistingTag_ReturnsExistsTrueWithDetails()
    {
        var assetTag = "TAG-001";
        var assetDto = new AssetDto
        {
            Id = Guid.NewGuid(),
            AssetTag = assetTag,
            Name = "Laptop 1",
            Status = AssetStatus.Active
        };

        _assetServiceMock
            .Setup(s => s.GetAssetByTagAsync(assetTag))
            .ReturnsAsync(assetDto);

        var result = await _service.CheckBarcodeAsync(assetTag);

        result.Exists.Should().BeTrue();
        result.AssetTag.Should().Be(assetTag);
        result.Name.Should().Be("Laptop 1");
    }

    [Fact]
    public async Task CheckBarcodeAsync_NonExistentTag_ReturnsExistsFalse()
    {
        var assetTag = "NONEXISTENT";

        _assetServiceMock
            .Setup(s => s.GetAssetByTagAsync(assetTag))
            .ReturnsAsync((AssetDto?)null);

        var result = await _service.CheckBarcodeAsync(assetTag);

        result.Exists.Should().BeFalse();
        result.AssetTag.Should().BeNull();
        result.Name.Should().BeNull();
    }

    [Fact]
    public async Task CheckBarcodeAsync_EmptyBarcode_ReturnsExistsFalse()
    {
        _assetServiceMock
            .Setup(s => s.GetAssetByTagAsync(It.IsAny<string>()))
            .ReturnsAsync((AssetDto?)null);

        var result = await _service.CheckBarcodeAsync("");

        result.Exists.Should().BeFalse();
        result.AssetTag.Should().BeNull();
    }

    #endregion

    #region Helpers

    private static HierarchyDto CreateTestHierarchy()
    {
        var zoneAId = Guid.NewGuid();
        var zoneBId = Guid.NewGuid();

        return new HierarchyDto
        {
            Site = new SiteDto
            {
                Id = Guid.NewGuid(),
                Code = "MAIN",
                Name = "Main Site",
                Zones = new List<ZoneDto>
                {
                    new()
                    {
                        Id = zoneAId,
                        Code = "ZA",
                        Name = "Zone A",
                        Buildings = new List<BuildingDto>
                        {
                            new()
                            {
                                Id = Guid.NewGuid(),
                                Code = "B-A1",
                                Name = "Building A1",
                                Floors = new List<FloorDto>
                                {
                                    new()
                                    {
                                        Id = Guid.NewGuid(),
                                        BuildingId = Guid.NewGuid(),
                                        Level = 1,
                                        Description = "Ground Floor",
                                        Rooms = new List<RoomDto>
                                        {
                                            new()
                                            {
                                                Id = Guid.NewGuid(),
                                                Code = "Room-A1-01",
                                                Name = "Server Room A1",
                                                FloorLevel = 1
                                            },
                                            new()
                                            {
                                                Id = Guid.NewGuid(),
                                                Code = "Room-A1-02",
                                                Name = "Storage A1",
                                                FloorLevel = 1
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    },
                    new()
                    {
                        Id = zoneBId,
                        Code = "ZB",
                        Name = "Zone B",
                        Buildings = new List<BuildingDto>
                        {
                            new()
                            {
                                Id = Guid.NewGuid(),
                                Code = "B-B1",
                                Name = "Building B1",
                                Floors = new List<FloorDto>
                                {
                                    new()
                                    {
                                        Id = Guid.NewGuid(),
                                        BuildingId = Guid.NewGuid(),
                                        Level = 1,
                                        Description = "Ground Floor",
                                        Rooms = new List<RoomDto>
                                        {
                                            new()
                                            {
                                                Id = Guid.NewGuid(),
                                                Code = "Room-B1-01",
                                                Name = "Workshop B1",
                                                FloorLevel = 1
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        };
    }

    #endregion
}
