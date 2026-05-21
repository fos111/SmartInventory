using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using SmartInventory.Application.Asset.DTOs;
using SmartInventory.Application.Asset.DTOs.Reports;
using SmartInventory.Application.Asset.Interfaces;
using SmartInventory.Application.Mobile.Auth.DTOs;
using SmartInventory.Application.Mobile.Auth.Interfaces;
using SmartInventory.Application.Mobile.Home.DTOs;
using SmartInventory.Application.Mobile.Home.Interfaces;
using SmartInventory.Application.Mobile.Home.Services;
using SmartInventory.Application.Notification.Interfaces;
using Xunit;

namespace SmartInventory.Application.Tests.Mobile.Home.Services;

public class MobileHomeServiceTests
{
    private readonly Mock<IMobileAuthService> _authServiceMock;
    private readonly Mock<IReportingService> _reportingServiceMock;
    private readonly Mock<IActivityLogService> _activityLogServiceMock;
    private readonly Mock<INotificationService> _notificationServiceMock;
    private readonly IMobileHomeService _service;
    private readonly Guid _userId;

    public MobileHomeServiceTests()
    {
        _authServiceMock = new Mock<IMobileAuthService>();
        _reportingServiceMock = new Mock<IReportingService>();
        _activityLogServiceMock = new Mock<IActivityLogService>();
        _notificationServiceMock = new Mock<INotificationService>();
        _userId = Guid.NewGuid();

        _service = new MobileHomeService(
            _authServiceMock.Object,
            _reportingServiceMock.Object,
            _activityLogServiceMock.Object,
            _notificationServiceMock.Object);
    }

    #region GetHomeAsync

    [Fact]
    public async Task GetHomeAsync_CallsAllServicesInParallel()
    {
        var user = new MobileUserDto { Id = _userId, Name = "John Doe", Email = "john@test.com", Role = "Admin", Avatar = null };
        var stats = new List<StatusSummaryDto>
        {
            new() { Status = "InStock", Count = 5 },
            new() { Status = "Maintenance", Count = 2 },
            new() { Status = "Critical", Count = 1 },
            new() { Status = "Retired", Count = 0 }
        };
        var activities = new List<ActivityLogDto>
        {
            new() { Id = Guid.NewGuid(), Action = "Created", AssetName = "Laptop", ChangedAt = DateTime.UtcNow, ChangedByName = "John" }
        };

        _authServiceMock
            .Setup(s => s.GetProfileAsync(_userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _reportingServiceMock
            .Setup(s => s.GetStatusSummaryAsync())
            .ReturnsAsync(stats);
        _activityLogServiceMock
            .Setup(s => s.GetAllActivityLogsAsync(It.IsAny<DateTime?>(), null))
            .ReturnsAsync(activities);

        await _service.GetHomeAsync(_userId, CancellationToken.None);

        _authServiceMock.Verify(s => s.GetProfileAsync(_userId, It.IsAny<CancellationToken>()), Times.Once);
        _reportingServiceMock.Verify(s => s.GetStatusSummaryAsync(), Times.Once);
        _activityLogServiceMock.Verify(s => s.GetAllActivityLogsAsync(It.IsAny<DateTime?>(), null), Times.Once);
        _notificationServiceMock.Verify(s => s.GetUnreadCountAsync(_userId), Times.Once);
    }

    [Fact]
    public async Task GetHomeAsync_ReturnsCompleteHomeSyncDto()
    {
        var userId = Guid.NewGuid();
        var user = new MobileUserDto { Id = userId, Name = "Jane Doe", Email = "jane@test.com", Role = "Manager", Avatar = "avatar.png" };
        var stats = new List<StatusSummaryDto>
        {
            new() { Status = "InStock", Count = 10 },
            new() { Status = "Maintenance", Count = 3 },
            new() { Status = "Critical", Count = 1 },
            new() { Status = "Retired", Count = 2 }
        };
        var activities = new List<ActivityLogDto>
        {
            new() { Id = Guid.NewGuid(), Action = "Updated", AssetName = "Server", ChangedAt = DateTime.UtcNow, ChangedByName = "Jane" }
        };

        _authServiceMock
            .Setup(s => s.GetProfileAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _reportingServiceMock
            .Setup(s => s.GetStatusSummaryAsync())
            .ReturnsAsync(stats);
        _activityLogServiceMock
            .Setup(s => s.GetAllActivityLogsAsync(It.IsAny<DateTime?>(), null))
            .ReturnsAsync(activities);
        _notificationServiceMock
            .Setup(s => s.GetUnreadCountAsync(userId))
            .ReturnsAsync(5);

        var result = await _service.GetHomeAsync(userId, CancellationToken.None);

        result.Should().NotBeNull();
        result.User.Should().NotBeNull();
        result.User!.Id.Should().Be(userId);
        result.User.Name.Should().Be("Jane Doe");
        result.User.Email.Should().Be("jane@test.com");
        result.User.Role.Should().Be("Manager");
        result.User.Avatar.Should().Be("avatar.png");
        result.Stats.Should().NotBeNull();
        result.Stats!.InStock.Should().Be(10);
        result.Stats.UnderMaintenance.Should().Be(3);
        result.Stats.Critical.Should().Be(1);
        result.Stats.Retired.Should().Be(2);
        result.RecentActivity.Should().NotBeNull();
        result.UnreadNotifications.Should().Be(5);
    }

    [Fact]
    public async Task GetHomeAsync_ReturnsLast10RecentActivities()
    {
        var activities = Enumerable.Range(0, 15)
            .Select(i => new ActivityLogDto
            {
                Id = Guid.NewGuid(),
                Action = "Event",
                AssetName = $"Asset-{i}",
                ChangedAt = new DateTime(2026, 5, 15, 10, 0, 0, DateTimeKind.Utc).AddMinutes(i),
                ChangedByName = "User"
            })
            .ToList();

        _authServiceMock
            .Setup(s => s.GetProfileAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new MobileUserDto { Id = Guid.NewGuid(), Name = "Test", Email = "test@test.com", Role = "User", Avatar = null });
        _reportingServiceMock
            .Setup(s => s.GetStatusSummaryAsync())
            .ReturnsAsync(new List<StatusSummaryDto>());
        _activityLogServiceMock
            .Setup(s => s.GetAllActivityLogsAsync(It.IsAny<DateTime?>(), null))
            .ReturnsAsync(activities);
        _notificationServiceMock
            .Setup(s => s.GetUnreadCountAsync(It.IsAny<Guid>()))
            .ReturnsAsync(0);

        var result = await _service.GetHomeAsync(Guid.NewGuid(), CancellationToken.None);

        result.RecentActivity.Should().HaveCount(10);
        result.RecentActivity.Should().BeInDescendingOrder(a => a.ChangedAt);
    }

    [Fact]
    public async Task GetHomeAsync_ServerTimestampIsRecent()
    {
        _authServiceMock
            .Setup(s => s.GetProfileAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new MobileUserDto { Id = Guid.NewGuid(), Name = "Test", Email = "test@test.com", Role = "User", Avatar = null });
        _reportingServiceMock
            .Setup(s => s.GetStatusSummaryAsync())
            .ReturnsAsync(new List<StatusSummaryDto>());
        _activityLogServiceMock
            .Setup(s => s.GetAllActivityLogsAsync(It.IsAny<DateTime?>(), null))
            .ReturnsAsync(new List<ActivityLogDto>());
        _notificationServiceMock
            .Setup(s => s.GetUnreadCountAsync(It.IsAny<Guid>()))
            .ReturnsAsync(0);

        var before = DateTime.UtcNow;
        var result = await _service.GetHomeAsync(Guid.NewGuid(), CancellationToken.None);
        var after = DateTime.UtcNow;

        result.ServerTimestamp.Should().BeOnOrAfter(before);
        result.ServerTimestamp.Should().BeOnOrBefore(after);
    }

    [Fact]
    public async Task GetHomeAsync_WhenUserIsNull_ReturnsNullUser()
    {
        _authServiceMock
            .Setup(s => s.GetProfileAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((MobileUserDto?)null);
        _reportingServiceMock
            .Setup(s => s.GetStatusSummaryAsync())
            .ReturnsAsync(new List<StatusSummaryDto>());
        _activityLogServiceMock
            .Setup(s => s.GetAllActivityLogsAsync(It.IsAny<DateTime?>(), null))
            .ReturnsAsync(new List<ActivityLogDto>());
        _notificationServiceMock
            .Setup(s => s.GetUnreadCountAsync(It.IsAny<Guid>()))
            .ReturnsAsync(0);

        var result = await _service.GetHomeAsync(Guid.NewGuid(), CancellationToken.None);

        result.User.Should().BeNull();
    }

    [Fact]
    public async Task GetHomeAsync_MapsStatsCorrectly()
    {
        var stats = new List<StatusSummaryDto>
        {
            new() { Status = "InStock", Count = 10 },
            new() { Status = "Maintenance", Count = 3 },
            new() { Status = "Critical", Count = 1 },
            new() { Status = "Retired", Count = 2 }
        };

        _authServiceMock
            .Setup(s => s.GetProfileAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new MobileUserDto { Id = Guid.NewGuid(), Name = "Test", Email = "test@test.com", Role = "User", Avatar = null });
        _reportingServiceMock
            .Setup(s => s.GetStatusSummaryAsync())
            .ReturnsAsync(stats);
        _activityLogServiceMock
            .Setup(s => s.GetAllActivityLogsAsync(It.IsAny<DateTime?>(), null))
            .ReturnsAsync(new List<ActivityLogDto>());
        _notificationServiceMock
            .Setup(s => s.GetUnreadCountAsync(It.IsAny<Guid>()))
            .ReturnsAsync(0);

        var result = await _service.GetHomeAsync(Guid.NewGuid(), CancellationToken.None);

        result.Stats.Should().NotBeNull();
        result.Stats!.InStock.Should().Be(10);
        result.Stats.UnderMaintenance.Should().Be(3);
        result.Stats.Critical.Should().Be(1);
        result.Stats.Retired.Should().Be(2);
    }

    #endregion
}
