using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using SmartInventory.Application.Asset.DTOs;
using SmartInventory.Application.Asset.Interfaces;
using SmartInventory.Application.Asset.Services;
using SmartInventory.Application.Auth.Interfaces;
using SmartInventory.Domain.Asset.Enums;
using SmartInventory.Domain.Auth.Entities;
using SmartInventory.Domain.Auth.Enums;
using Xunit;
using AssetEntity = SmartInventory.Domain.Asset.Entities.Asset;
using ActivityLogEntity = SmartInventory.Domain.Asset.Entities.ActivityLog;

namespace SmartInventory.Application.Tests;

public class ReportingServiceTests : ApplicationTestBase
{
    private readonly Mock<IAssetRepository> _assetRepositoryMock;
    private readonly Mock<IAssetHistoryService> _historyServiceMock;
    private readonly Mock<IActivityLogService> _activityLogServiceMock;
    private readonly Mock<IAuthRepository> _authRepositoryMock;
    private readonly ReportingService _service;

    public ReportingServiceTests()
    {
        _assetRepositoryMock = new Mock<IAssetRepository>();
        _historyServiceMock = new Mock<IAssetHistoryService>();
        _activityLogServiceMock = new Mock<IActivityLogService>();
        _authRepositoryMock = new Mock<IAuthRepository>();
        _service = new ReportingService(
            _assetRepositoryMock.Object,
            _historyServiceMock.Object,
            _activityLogServiceMock.Object,
            _authRepositoryMock.Object);
    }

    private List<AssetEntity> CreateTestAssets()
    {
        return new List<AssetEntity>
        {
            new() { Id = Guid.NewGuid(), AssetTag = "AST-001", Name = "Dell Laptop", Category = "Computer", Status = AssetStatus.Active, CurrentRoomCode = "LI1" },
            new() { Id = Guid.NewGuid(), AssetTag = "AST-002", Name = "HP Monitor", Category = "Display", Status = AssetStatus.Active, CurrentRoomCode = "LI2" },
            new() { Id = Guid.NewGuid(), AssetTag = "AST-003", Name = "Canon Printer", Category = "Printer/Scanner", Status = AssetStatus.Maintenance, CurrentRoomCode = "MEC1" },
            new() { Id = Guid.NewGuid(), AssetTag = "AST-004", Name = "Epson Projector", Category = "Projector", Status = AssetStatus.Retired, CurrentRoomCode = "GEST1" },
            new() { Id = Guid.NewGuid(), AssetTag = "AST-005", Name = "Dell Server", Category = "Server", Status = AssetStatus.Active, CurrentRoomCode = "LI1" },
            new() { Id = Guid.NewGuid(), AssetTag = "AST-006", Name = "Cisco Switch", Category = "Network Device", Status = AssetStatus.Active, CurrentRoomCode = "LI2" }
        };
    }

    [Fact]
    public async Task GetInventorySummary_ByCategory_ReturnsCorrectGroups()
    {
        var assets = CreateTestAssets();
        _assetRepositoryMock.Setup(r => r.GetAssetsAsync(It.IsAny<Asset.Filters.AssetFilter>(), 1, int.MaxValue))
            .ReturnsAsync((assets, assets.Count));

        var result = await _service.GetInventorySummaryAsync("category", null, UserRole.Supervisor);

        result.Should().NotBeEmpty();
        result.Count().Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task GetInventorySummary_ByStatus_ReturnsCorrectGroups()
    {
        var assets = CreateTestAssets();
        _assetRepositoryMock.Setup(r => r.GetAssetsAsync(It.IsAny<Asset.Filters.AssetFilter>(), 1, int.MaxValue))
            .ReturnsAsync((assets, assets.Count));

        var result = await _service.GetInventorySummaryAsync("status", null, UserRole.Supervisor);

        result.Should().NotBeEmpty();
        var activeCount = result.FirstOrDefault(r => r.GroupKey == "Active")?.Count ?? 0;
        activeCount.Should().Be(4);
    }

    [Fact]
    public async Task GetInventorySummary_ByLocation_ReturnsCorrectGroups()
    {
        var assets = CreateTestAssets();
        _assetRepositoryMock.Setup(r => r.GetAssetsAsync(It.IsAny<Asset.Filters.AssetFilter>(), 1, int.MaxValue))
            .ReturnsAsync((assets, assets.Count));

        var result = await _service.GetInventorySummaryAsync("location", null, UserRole.Supervisor);

        result.Should().NotBeEmpty();
    }

    [Fact]
    public async Task GetInventorySummary_InvalidGroupBy_ThrowsArgumentException()
    {
        var assets = CreateTestAssets();
        _assetRepositoryMock.Setup(r => r.GetAssetsAsync(It.IsAny<Asset.Filters.AssetFilter>(), 1, int.MaxValue))
            .ReturnsAsync((assets, assets.Count));

        var act = () => _service.GetInventorySummaryAsync("invalid", null, UserRole.Supervisor);

        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task GetAssetHistory_ReturnsHistoryForAsset()
    {
        var assetId = Guid.NewGuid();
        var asset = new AssetEntity { Id = assetId, AssetTag = "AST-001", Name = "Test Asset", Category = "Computer", Status = AssetStatus.Active, CurrentRoomCode = "LI1" };
        var history = new List<Domain.Asset.Entities.AssetHistory>
        {
            new() { Id = Guid.NewGuid(), AssetId = assetId, PropertyChanged = "Status", OldValue = "Active", NewValue = "Maintenance", ChangedAt = DateTime.UtcNow.AddDays(-1) },
            new() { Id = Guid.NewGuid(), AssetId = assetId, PropertyChanged = "CurrentRoomCode", OldValue = "LI1", NewValue = "LI2", ChangedAt = DateTime.UtcNow }
        };

        _assetRepositoryMock.Setup(r => r.GetByIdAsync(assetId)).ReturnsAsync(asset);
        _historyServiceMock.Setup(h => h.GetAssetHistoryAsync(assetId)).ReturnsAsync(history);

        var result = await _service.GetAssetHistoryAsync(assetId);

        result.Should().NotBeEmpty();
        result.Count().Should().Be(2);
    }

    [Fact]
    public async Task GetAssetHistory_InvalidAssetId_ThrowsArgumentException()
    {
        var assetId = Guid.NewGuid();
        _assetRepositoryMock.Setup(r => r.GetByIdAsync(assetId)).ReturnsAsync((AssetEntity?)null);

        var act = () => _service.GetAssetHistoryAsync(assetId);

        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task GetActivityLog_FiltersByDateRange()
    {
        var assets = CreateTestAssets();
        var assetId = assets[0].Id;
        var history = new List<Domain.Asset.Entities.AssetHistory>
        {
            new() { Id = Guid.NewGuid(), AssetId = assetId, PropertyChanged = "Status", OldValue = "Active", NewValue = "Maintenance", ChangedAt = DateTime.UtcNow.AddDays(-10) },
            new() { Id = Guid.NewGuid(), AssetId = assetId, PropertyChanged = "Status", OldValue = "Maintenance", NewValue = "Active", ChangedAt = DateTime.UtcNow.AddDays(-1) }
        };

        _assetRepositoryMock.Setup(r => r.GetAssetsAsync(It.IsAny<Asset.Filters.AssetFilter>(), 1, int.MaxValue))
            .ReturnsAsync((assets, assets.Count));
        _historyServiceMock.Setup(h => h.GetAllHistoryAsync(It.IsAny<DateTime?>(), It.IsAny<DateTime?>()))
            .ReturnsAsync(history);

        var result = await _service.GetActivityLogAsync(DateTime.UtcNow.AddDays(-5), DateTime.UtcNow, null);

        result.Should().NotBeEmpty();
    }

    [Fact]
    public async Task GetLocationReport_ReturnsAssetsPerLocation()
    {
        var assets = CreateTestAssets();
        _assetRepositoryMock.Setup(r => r.GetAssetsAsync(It.IsAny<Asset.Filters.AssetFilter>(), 1, int.MaxValue))
            .ReturnsAsync((assets, assets.Count));

        var result = await _service.GetLocationReportAsync(null);

        result.Should().NotBeEmpty();
    }

    [Fact]
    public async Task GetLocationReport_LI1_HasCorrectCounts()
    {
        var assets = CreateTestAssets();
        _assetRepositoryMock.Setup(r => r.GetAssetsAsync(It.IsAny<Asset.Filters.AssetFilter>(), 1, int.MaxValue))
            .ReturnsAsync((assets, assets.Count));

        var result = await _service.GetLocationReportAsync(null);
        var li1Result = result.FirstOrDefault(r => r.RoomCode == "LI1");

        li1Result.Should().NotBeNull();
        li1Result.TotalAssets.Should().Be(2);
        li1Result.ActiveAssets.Should().Be(2);
    }
}

public class ReportingActivityFeedTests : ApplicationTestBase
{
    private readonly Mock<IAssetRepository> _assetRepositoryMock;
    private readonly Mock<IAssetHistoryService> _historyServiceMock;
    private readonly Mock<IActivityLogService> _activityLogServiceMock;
    private readonly Mock<IAuthRepository> _authRepositoryMock;
    private readonly ReportingService _service;

    public ReportingActivityFeedTests()
    {
        _assetRepositoryMock = new Mock<IAssetRepository>();
        _historyServiceMock = new Mock<IAssetHistoryService>();
        _activityLogServiceMock = new Mock<IActivityLogService>();
        _authRepositoryMock = new Mock<IAuthRepository>();
        _service = new ReportingService(
            _assetRepositoryMock.Object,
            _historyServiceMock.Object,
            _activityLogServiceMock.Object,
            _authRepositoryMock.Object);
    }

    private List<AssetEntity> CreateTestAssets()
    {
        return new List<AssetEntity>
        {
            new() { Id = Guid.NewGuid(), AssetTag = "AST-001", Name = "Dell Laptop", Category = "Computer", Status = AssetStatus.Active, CurrentRoomCode = "LI1" },
            new() { Id = Guid.NewGuid(), AssetTag = "AST-002", Name = "HP Monitor", Category = "Display", Status = AssetStatus.Active, CurrentRoomCode = "LI2" }
        };
    }

    private List<Domain.Asset.Entities.AssetHistory> CreateAssetHistory(List<AssetEntity> assets, Guid userId)
    {
        return new List<Domain.Asset.Entities.AssetHistory>
        {
            new() { Id = Guid.NewGuid(), AssetId = assets[0].Id, PropertyChanged = "Status", OldValue = "Active", NewValue = "Maintenance", ChangedBy = userId, ChangedAt = DateTime.UtcNow.AddDays(-2) },
            new() { Id = Guid.NewGuid(), AssetId = assets[1].Id, PropertyChanged = "CurrentRoomCode", OldValue = "LI2", NewValue = "LI1", ChangedBy = userId, ChangedAt = DateTime.UtcNow.AddDays(-1) }
        };
    }

    private List<ActivityLogDto> CreateFacilityActivity(Guid userId)
    {
        return new List<ActivityLogDto>
        {
            new() { Id = Guid.NewGuid(), Action = "Created", EntityType = "Room", EntityId = "RM-NEW", AssetName = "New Room", ChangedBy = userId, ChangedAt = DateTime.UtcNow.AddDays(-3) },
            new() { Id = Guid.NewGuid(), Action = "Created", EntityType = "Building", EntityId = "BLD-NEW", AssetName = "New Building", ChangedBy = userId, ChangedAt = DateTime.UtcNow }
        };
    }

    [Fact]
    public async Task GetActivityLog_UnionsAssetHistoryAndActivityLog()
    {
        var assets = CreateTestAssets();
        var userId = Guid.NewGuid();
        var assetHistory = CreateAssetHistory(assets, userId);
        var facilityActivity = CreateFacilityActivity(userId);

        _assetRepositoryMock.Setup(r => r.GetAssetsAsync(It.IsAny<Asset.Filters.AssetFilter>(), 1, int.MaxValue))
            .ReturnsAsync((assets, assets.Count));
        _historyServiceMock.Setup(h => h.GetAllHistoryAsync(null, null)).ReturnsAsync(assetHistory);
        _activityLogServiceMock.Setup(a => a.GetAllActivityLogsAsync(null, null)).ReturnsAsync(facilityActivity);
        _authRepositoryMock.Setup(a => a.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new User { Id = userId, Username = "john.doe" });

        var result = (await _service.GetActivityLogAsync(null, null, null)).ToList();

        result.Should().HaveCount(4);
        result.Count(r => r.EntityType == null).Should().Be(2);
        result.Count(r => r.EntityType != null).Should().Be(2);
    }

    [Fact]
    public async Task GetActivityLog_PassesDateFiltersToBothSources()
    {
        var assets = CreateTestAssets();
        var from = DateTime.UtcNow.AddDays(-5);
        var to = DateTime.UtcNow;

        _assetRepositoryMock.Setup(r => r.GetAssetsAsync(It.IsAny<Asset.Filters.AssetFilter>(), 1, int.MaxValue))
            .ReturnsAsync((assets, assets.Count));
        _historyServiceMock.Setup(h => h.GetAllHistoryAsync(from, to)).ReturnsAsync(new List<Domain.Asset.Entities.AssetHistory>());
        _activityLogServiceMock.Setup(a => a.GetAllActivityLogsAsync(from, to)).ReturnsAsync(new List<ActivityLogDto>());

        await _service.GetActivityLogAsync(from, to, null);

        _historyServiceMock.Verify(h => h.GetAllHistoryAsync(from, to), Times.Once);
        _activityLogServiceMock.Verify(a => a.GetAllActivityLogsAsync(from, to), Times.Once);
    }

    [Fact]
    public async Task GetActivityLog_ResolvesUserNames()
    {
        var assets = CreateTestAssets();
        var user1Id = Guid.NewGuid();
        var user2Id = Guid.NewGuid();
        var assetHistory = new List<Domain.Asset.Entities.AssetHistory>
        {
            new() { Id = Guid.NewGuid(), AssetId = assets[0].Id, PropertyChanged = "Status", ChangedBy = user1Id, ChangedAt = DateTime.UtcNow.AddDays(-1) }
        };
        var facilityActivity = new List<ActivityLogDto>
        {
            new() { Id = Guid.NewGuid(), Action = "Created", EntityType = "Room", ChangedBy = user2Id, ChangedAt = DateTime.UtcNow }
        };

        _assetRepositoryMock.Setup(r => r.GetAssetsAsync(It.IsAny<Asset.Filters.AssetFilter>(), 1, int.MaxValue))
            .ReturnsAsync((assets, assets.Count));
        _historyServiceMock.Setup(h => h.GetAllHistoryAsync(null, null)).ReturnsAsync(assetHistory);
        _activityLogServiceMock.Setup(a => a.GetAllActivityLogsAsync(null, null)).ReturnsAsync(facilityActivity);
        _authRepositoryMock.Setup(a => a.GetByIdAsync(user1Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new User { Id = user1Id, Username = "alice" });
        _authRepositoryMock.Setup(a => a.GetByIdAsync(user2Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new User { Id = user2Id, Username = "bob" });

        var result = (await _service.GetActivityLogAsync(null, null, null)).ToList();

        result.Should().HaveCount(2);
        result.Single(r => r.ChangedBy == user1Id).ChangedByName.Should().Be("alice");
        result.Single(r => r.ChangedBy == user2Id).ChangedByName.Should().Be("bob");
    }

    [Fact]
    public async Task GetActivityLog_UnknownUser_ShowsUnknownUserName()
    {
        var assets = CreateTestAssets();
        var unknownUserId = Guid.NewGuid();
        var facilityActivity = new List<ActivityLogDto>
        {
            new() { Id = Guid.NewGuid(), Action = "Created", EntityType = "Room", ChangedBy = unknownUserId, ChangedAt = DateTime.UtcNow }
        };

        _assetRepositoryMock.Setup(r => r.GetAssetsAsync(It.IsAny<Asset.Filters.AssetFilter>(), 1, int.MaxValue))
            .ReturnsAsync((assets, assets.Count));
        _historyServiceMock.Setup(h => h.GetAllHistoryAsync(null, null)).ReturnsAsync(new List<Domain.Asset.Entities.AssetHistory>());
        _activityLogServiceMock.Setup(a => a.GetAllActivityLogsAsync(null, null)).ReturnsAsync(facilityActivity);
        _authRepositoryMock.Setup(a => a.GetByIdAsync(unknownUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        var result = (await _service.GetActivityLogAsync(null, null, null)).ToList();

        result.Should().HaveCount(1);
        result[0].ChangedByName.Should().Be("Unknown User");
    }

    [Fact]
    public async Task GetActivityLog_ResolvesUserOnceForDuplicateChangedBy()
    {
        var assets = CreateTestAssets();
        var userId = Guid.NewGuid();
        var assetHistory = CreateAssetHistory(assets, userId);
        var facilityActivity = new List<ActivityLogDto>
        {
            new() { Id = Guid.NewGuid(), Action = "Created", EntityType = "Room", ChangedBy = userId, ChangedAt = DateTime.UtcNow }
        };

        _assetRepositoryMock.Setup(r => r.GetAssetsAsync(It.IsAny<Asset.Filters.AssetFilter>(), 1, int.MaxValue))
            .ReturnsAsync((assets, assets.Count));
        _historyServiceMock.Setup(h => h.GetAllHistoryAsync(null, null)).ReturnsAsync(assetHistory);
        _activityLogServiceMock.Setup(a => a.GetAllActivityLogsAsync(null, null)).ReturnsAsync(facilityActivity);
        _authRepositoryMock.Setup(a => a.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new User { Id = userId, Username = "john.doe" });

        var result = (await _service.GetActivityLogAsync(null, null, null)).ToList();

        _authRepositoryMock.Verify(a => a.GetByIdAsync(userId, It.IsAny<CancellationToken>()), Times.Once);
        result.All(r => r.ChangedByName == "john.doe").Should().BeTrue();
    }

    [Fact]
    public async Task GetActivityLog_WithUserIdFilter_FiltersByUser()
    {
        var assets = CreateTestAssets();
        var targetUserId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();
        var assetHistory = new List<Domain.Asset.Entities.AssetHistory>
        {
            new() { Id = Guid.NewGuid(), AssetId = assets[0].Id, PropertyChanged = "Status", ChangedBy = targetUserId, ChangedAt = DateTime.UtcNow.AddDays(-1) },
            new() { Id = Guid.NewGuid(), AssetId = assets[1].Id, PropertyChanged = "Name", ChangedBy = otherUserId, ChangedAt = DateTime.UtcNow }
        };
        var facilityActivity = new List<ActivityLogDto>
        {
            new() { Id = Guid.NewGuid(), Action = "Created", EntityType = "Room", ChangedBy = otherUserId, ChangedAt = DateTime.UtcNow }
        };

        _assetRepositoryMock.Setup(r => r.GetAssetsAsync(It.IsAny<Asset.Filters.AssetFilter>(), 1, int.MaxValue))
            .ReturnsAsync((assets, assets.Count));
        _historyServiceMock.Setup(h => h.GetAllHistoryAsync(null, null)).ReturnsAsync(assetHistory);
        _activityLogServiceMock.Setup(a => a.GetAllActivityLogsAsync(null, null)).ReturnsAsync(facilityActivity);
        _authRepositoryMock.Setup(a => a.GetByIdAsync(targetUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new User { Id = targetUserId, Username = "target" });

        var result = (await _service.GetActivityLogAsync(null, null, targetUserId)).ToList();

        result.Should().HaveCount(1);
        result[0].ChangedBy.Should().Be(targetUserId);
    }

    [Fact]
    public async Task GetActivityLog_ReturnsMergedResultsChronologically()
    {
        var assets = CreateTestAssets();
        var userId = Guid.NewGuid();
        var oldest = new Domain.Asset.Entities.AssetHistory
        {
            Id = Guid.NewGuid(), AssetId = assets[0].Id, PropertyChanged = "Status",
            ChangedBy = userId, ChangedAt = DateTime.UtcNow.AddDays(-3)
        };
        var middle = new ActivityLogDto
        {
            Id = Guid.NewGuid(), Action = "Created", EntityType = "Room",
            ChangedBy = userId, ChangedAt = DateTime.UtcNow.AddDays(-1)
        };
        var newest = new Domain.Asset.Entities.AssetHistory
        {
            Id = Guid.NewGuid(), AssetId = assets[1].Id, PropertyChanged = "Name",
            ChangedBy = userId, ChangedAt = DateTime.UtcNow
        };

        _assetRepositoryMock.Setup(r => r.GetAssetsAsync(It.IsAny<Asset.Filters.AssetFilter>(), 1, int.MaxValue))
            .ReturnsAsync((assets, assets.Count));
        _historyServiceMock.Setup(h => h.GetAllHistoryAsync(null, null)).ReturnsAsync(new List<Domain.Asset.Entities.AssetHistory> { oldest, newest });
        _activityLogServiceMock.Setup(a => a.GetAllActivityLogsAsync(null, null)).ReturnsAsync(new List<ActivityLogDto> { middle });
        _authRepositoryMock.Setup(a => a.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new User { Id = userId, Username = "user" });

        var result = (await _service.GetActivityLogAsync(null, null, null)).ToList();

        result.Should().HaveCount(3);
        result[0].ChangedAt.Should().Be(newest.ChangedAt);
        result[1].ChangedAt.Should().Be(middle.ChangedAt);
        result[2].ChangedAt.Should().Be(oldest.ChangedAt);
    }

    [Fact]
    public async Task GetActivityLog_ReturnsEmpty_WhenNoActivity()
    {
        var assets = CreateTestAssets();

        _assetRepositoryMock.Setup(r => r.GetAssetsAsync(It.IsAny<Asset.Filters.AssetFilter>(), 1, int.MaxValue))
            .ReturnsAsync((assets, assets.Count));
        _historyServiceMock.Setup(h => h.GetAllHistoryAsync(null, null)).ReturnsAsync(new List<Domain.Asset.Entities.AssetHistory>());
        _activityLogServiceMock.Setup(a => a.GetAllActivityLogsAsync(null, null)).ReturnsAsync(new List<ActivityLogDto>());

        var result = await _service.GetActivityLogAsync(null, null, null);

        result.Should().BeEmpty();
    }
}

public class ExportServiceTests : ApplicationTestBase
{
    private readonly CsvExportService _csvService;
    private readonly PdfExportService _pdfService;

    public ExportServiceTests()
    {
        _csvService = new CsvExportService();
        _pdfService = new PdfExportService();
    }

    [Fact]
    public async Task ExportToCsv_GeneratesValidCsv()
    {
        var data = new List<InventorySummaryDto>
        {
            new() { GroupKey = "Computer", GroupLabel = "Computer", Count = 10 },
            new() { GroupKey = "Display", GroupLabel = "Display", Count = 5 }
        };

        var result = await _csvService.ExportToCsvAsync(data, "test");

        result.Should().NotBeEmpty();
        var content = System.Text.Encoding.UTF8.GetString(result);
        content.Should().Contain("GroupKey");
        content.Should().Contain("Computer");
    }

    [Fact]
    public async Task ExportToCsv_EmptyList_ReturnsEmpty()
    {
        var data = new List<InventorySummaryDto>();

        var result = await _csvService.ExportToCsvAsync(data, "test");

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task ExportToPdf_GeneratesValidPdf()
    {
        var data = new List<InventorySummaryDto>
        {
            new() { GroupKey = "Computer", GroupLabel = "Computer", Count = 10 },
            new() { GroupKey = "Display", GroupLabel = "Display", Count = 5 }
        };

        var result = await _pdfService.ExportToPdfAsync(data, "Test Report", "test");

        result.Should().NotBeEmpty();
        result.Length.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task ExportToCsv_IncludesHeaders()
    {
        var data = new List<AssetHistoryDto>
        {
            new() { Id = Guid.NewGuid(), AssetId = Guid.NewGuid(), AssetTag = "AST-001", PropertyChanged = "Status", OldValue = "Active", NewValue = "Maintenance" }
        };

        var result = await _csvService.ExportToCsvAsync(data, "history");

        var content = System.Text.Encoding.UTF8.GetString(result);
        content.Should().Contain("AssetTag");
        content.Should().Contain("PropertyChanged");
    }
}
