using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using SmartInventory.Application.Asset.DTOs;
using SmartInventory.Application.Asset.Interfaces;
using SmartInventory.Application.Asset.Services;
using SmartInventory.Domain.Asset.Entities;
using Xunit;

namespace SmartInventory.Application.Tests;

public class ActivityLogServiceTests
{
    private readonly Mock<IActivityLogRepository> _repositoryMock;
    private readonly ActivityLogService _service;

    public ActivityLogServiceTests()
    {
        _repositoryMock = new Mock<IActivityLogRepository>();
        _service = new ActivityLogService(_repositoryMock.Object);
    }

    private static ActivityLog CreateLogEntity(
        string action = "Created",
        string entityType = "Zone",
        string entityId = "Z-001",
        string entityName = "Zone A",
        string? details = null)
    {
        return new ActivityLog
        {
            Id = Guid.NewGuid(),
            Action = action,
            EntityType = entityType,
            EntityId = entityId,
            EntityName = entityName,
            Details = details,
            ChangedBy = Guid.NewGuid(),
            ChangedAt = DateTime.UtcNow
        };
    }

    #region TrackFacilityChangeAsync

    [Fact]
    public async Task TrackFacilityChangeAsync_ShouldCreateLogWithCorrectValues()
    {
        var action = "Created";
        var entityType = "Zone";
        var entityId = "Z-001";
        var entityName = "Zone A";
        var details = "New zone for storage";
        var userId = Guid.NewGuid();

        ActivityLog? capturedLog = null;
        _repositoryMock
            .Setup(r => r.AddAsync(It.IsAny<ActivityLog>()))
            .Callback<ActivityLog>(log => capturedLog = log)
            .ReturnsAsync((ActivityLog log) => log);

        await _service.TrackFacilityChangeAsync(action, entityType, entityId, entityName, details, userId);

        _repositoryMock.Verify(r => r.AddAsync(It.IsAny<ActivityLog>()), Times.Once);
        capturedLog.Should().NotBeNull();
        capturedLog!.Action.Should().Be(action);
        capturedLog.EntityType.Should().Be(entityType);
        capturedLog.EntityId.Should().Be(entityId);
        capturedLog.EntityName.Should().Be(entityName);
        capturedLog.Details.Should().Be(details);
        capturedLog.ChangedBy.Should().Be(userId);
        capturedLog.ChangedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task TrackFacilityChangeAsync_ShouldWorkWithNullDetails()
    {
        var userId = Guid.NewGuid();

        ActivityLog? capturedLog = null;
        _repositoryMock
            .Setup(r => r.AddAsync(It.IsAny<ActivityLog>()))
            .Callback<ActivityLog>(log => capturedLog = log)
            .ReturnsAsync((ActivityLog log) => log);

        await _service.TrackFacilityChangeAsync("Updated", "Building", "B-01", "Main Building", null, userId);

        capturedLog.Should().NotBeNull();
        capturedLog!.Details.Should().BeNull();
    }

    [Fact]
    public async Task TrackFacilityChangeAsync_ShouldAssignNewGuid()
    {
        _repositoryMock
            .Setup(r => r.AddAsync(It.IsAny<ActivityLog>()))
            .ReturnsAsync((ActivityLog log) => log);

        await _service.TrackFacilityChangeAsync("Created", "Zone", "Z-001", "Zone A", null, Guid.NewGuid());

        _repositoryMock.Verify(r => r.AddAsync(It.Is<ActivityLog>(log => log.Id != Guid.Empty)), Times.Once);
    }

    [Fact]
    public async Task TrackFacilityChangeAsync_ShouldThrowOnEmptyAction()
    {
        var act = () => _service.TrackFacilityChangeAsync("", "Zone", "Z-001", "Zone A", null, Guid.NewGuid());

        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task TrackFacilityChangeAsync_ShouldThrowOnEmptyEntityType()
    {
        var act = () => _service.TrackFacilityChangeAsync("Created", "", "Z-001", "Zone A", null, Guid.NewGuid());

        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task TrackFacilityChangeAsync_ShouldThrowOnEmptyEntityName()
    {
        var act = () => _service.TrackFacilityChangeAsync("Created", "Zone", "Z-001", "", null, Guid.NewGuid());

        await act.Should().ThrowAsync<ArgumentException>();
    }

    #endregion

    #region GetAllActivityLogsAsync

    [Fact]
    public async Task GetAllActivityLogsAsync_ShouldReturnAllLogsMappedToDtos()
    {
        var logs = new List<ActivityLog>
        {
            CreateLogEntity(action: "Created", entityType: "Zone", entityId: "Z-001", entityName: "Zone A"),
            CreateLogEntity(action: "Updated", entityType: "Building", entityId: "B-01", entityName: "Building B", details: "Renamed"),
        };

        _repositoryMock.Setup(r => r.GetAllAsync(null, null)).ReturnsAsync(logs);

        var result = (await _service.GetAllActivityLogsAsync()).ToList();

        result.Should().HaveCount(2);
        result[0].Id.Should().Be(logs[0].Id);
        result[0].Action.Should().Be("Created");
        result[0].AssetName.Should().Be("Zone A");
        result[0].AssetTag.Should().Be("Z-001");
        result[0].EntityType.Should().Be("Zone");
        result[0].EntityId.Should().Be("Z-001");
        result[0].ChangedBy.Should().Be(logs[0].ChangedBy);

        result[1].Action.Should().Be("Updated");
        result[1].Details.Should().Be("Renamed");
        result[1].EntityType.Should().Be("Building");
    }

    [Fact]
    public async Task GetAllActivityLogsAsync_ShouldReturnEmptyList_WhenNoLogs()
    {
        _repositoryMock.Setup(r => r.GetAllAsync(null, null)).ReturnsAsync(new List<ActivityLog>());

        var result = await _service.GetAllActivityLogsAsync();

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetAllActivityLogsAsync_ShouldPassFromDateToRepository()
    {
        var from = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        _repositoryMock.Setup(r => r.GetAllAsync(from, null)).ReturnsAsync(new List<ActivityLog>());

        await _service.GetAllActivityLogsAsync(from);

        _repositoryMock.Verify(r => r.GetAllAsync(from, null), Times.Once);
    }

    [Fact]
    public async Task GetAllActivityLogsAsync_ShouldPassToDateToRepository()
    {
        var to = new DateTime(2025, 12, 31, 23, 59, 59, DateTimeKind.Utc);
        _repositoryMock.Setup(r => r.GetAllAsync(null, to)).ReturnsAsync(new List<ActivityLog>());

        await _service.GetAllActivityLogsAsync(null, to);

        _repositoryMock.Verify(r => r.GetAllAsync(null, to), Times.Once);
    }

    [Fact]
    public async Task GetAllActivityLogsAsync_ShouldPassBothDatesToRepository()
    {
        var from = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var to = new DateTime(2025, 12, 31, 23, 59, 59, DateTimeKind.Utc);
        _repositoryMock.Setup(r => r.GetAllAsync(from, to)).ReturnsAsync(new List<ActivityLog>());

        await _service.GetAllActivityLogsAsync(from, to);

        _repositoryMock.Verify(r => r.GetAllAsync(from, to), Times.Once);
    }

    [Fact]
    public async Task GetAllActivityLogsAsync_ShouldPreserveRepositoryOrdering()
    {
        var earlier = CreateLogEntity();
        var later = CreateLogEntity();
        earlier.ChangedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        later.ChangedAt = new DateTime(2025, 6, 15, 0, 0, 0, DateTimeKind.Utc);

        _repositoryMock.Setup(r => r.GetAllAsync(null, null)).ReturnsAsync(new List<ActivityLog> { later, earlier });

        var result = (await _service.GetAllActivityLogsAsync()).ToList();

        result.Should().HaveCount(2);
        result[0].ChangedAt.Should().Be(later.ChangedAt);
        result[1].ChangedAt.Should().Be(earlier.ChangedAt);
    }

    #endregion
}
