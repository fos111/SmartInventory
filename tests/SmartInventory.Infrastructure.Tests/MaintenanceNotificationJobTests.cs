using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using SmartInventory.Application.Asset.Interfaces;
using SmartInventory.Infrastructure.Asset.BackgroundJobs;
using SmartInventory.Application.Notification.DTOs;
using SmartInventory.Application.Notification.Interfaces;
using SmartInventory.Domain.Asset.Enums;
using SmartInventory.Domain.Notification.Enums;
using Xunit;
using AssetEntity = SmartInventory.Domain.Asset.Entities.Asset;

namespace SmartInventory.Infrastructure.Tests;

public class MaintenanceNotificationJobTests
{
    private readonly Mock<IAssetRepository> _repositoryMock;
    private readonly Mock<INotificationService> _notificationServiceMock;
    private readonly MaintenanceNotificationJob _job;

    public MaintenanceNotificationJobTests()
    {
        _repositoryMock = new Mock<IAssetRepository>();
        _notificationServiceMock = new Mock<INotificationService>();
        _job = new MaintenanceNotificationJob(
            _repositoryMock.Object,
            _notificationServiceMock.Object);
    }

    private AssetEntity CreateAsset(DateTime? maintenanceDueDate = null) => new()
    {
        Id = Guid.NewGuid(),
        AssetTag = "AST-001",
        Name = "Test Asset",
        Category = "Computer",
        Status = AssetStatus.Active,
        CurrentRoomCode = "LI1",
        MaintenanceDueDate = maintenanceDueDate
    };

    [Fact]
    public async Task RunAsync_NoAssetsDue_DoesNotCreateNotification()
    {
        _repositoryMock
            .Setup(r => r.GetAssetsWithMaintenanceAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .ReturnsAsync(new List<AssetEntity>());

        await _job.RunAsync(CancellationToken.None);

        _notificationServiceMock.Verify(
            n => n.CreateNotificationAsync(It.IsAny<CreateNotificationDto>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task RunAsync_AssetsDueSoon_CreatesMaintenanceDueSoonNotification()
    {
        var asset = CreateAsset(DateTime.UtcNow.AddDays(3));

        _repositoryMock
            .Setup(r => r.GetAssetsWithMaintenanceAsync(It.Is<DateTime>(d => d <= DateTime.UtcNow), It.IsAny<DateTime>()))
            .ReturnsAsync(new List<AssetEntity>());

        _repositoryMock
            .Setup(r => r.GetAssetsWithMaintenanceAsync(It.IsAny<DateTime>(), It.Is<DateTime>(d => d >= DateTime.UtcNow.AddDays(6))))
            .ReturnsAsync(new List<AssetEntity> { asset });

        CreateNotificationDto? capturedDto = null;
        _notificationServiceMock
            .Setup(n => n.CreateNotificationAsync(It.IsAny<CreateNotificationDto>(), It.IsAny<CancellationToken>()))
            .Callback<CreateNotificationDto, CancellationToken>((dto, _) => capturedDto = dto)
            .Returns(Task.CompletedTask);

        await _job.RunAsync(CancellationToken.None);

        capturedDto.Should().NotBeNull();
        capturedDto!.EventType.Should().Be(NotificationEventType.MaintenanceDueSoon);
        capturedDto.Type.Should().Be(NotificationType.Info);
        capturedDto.Title.Should().Be("Maintenance Due Soon");
        capturedDto.Message.Should().Contain("1 asset(s)");
        capturedDto.TargetRole.Should().Be(SmartInventory.Domain.Auth.Enums.UserRole.Technicien);
    }

    [Fact]
    public async Task RunAsync_AssetsOverdue_CreatesMaintenanceOverdueNotification()
    {
        var asset = CreateAsset(DateTime.UtcNow.AddDays(-1));

        _repositoryMock
            .Setup(r => r.GetAssetsWithMaintenanceAsync(It.IsAny<DateTime>(), It.Is<DateTime>(d => d <= DateTime.UtcNow)))
            .ReturnsAsync(new List<AssetEntity> { asset });

        _repositoryMock
            .Setup(r => r.GetAssetsWithMaintenanceAsync(It.Is<DateTime>(d => d <= DateTime.UtcNow), It.IsAny<DateTime>()))
            .ReturnsAsync(new List<AssetEntity> { asset });

        CreateNotificationDto? capturedDto = null;
        _notificationServiceMock
            .Setup(n => n.CreateNotificationAsync(It.IsAny<CreateNotificationDto>(), It.IsAny<CancellationToken>()))
            .Callback<CreateNotificationDto, CancellationToken>((dto, _) => capturedDto = dto)
            .Returns(Task.CompletedTask);

        await _job.RunAsync(CancellationToken.None);

        capturedDto.Should().NotBeNull();
        capturedDto!.EventType.Should().Be(NotificationEventType.MaintenanceOverdue);
        capturedDto.Type.Should().Be(NotificationType.Warning);
        capturedDto.Title.Should().Be("Maintenance Overdue");
        capturedDto.Message.Should().Contain("1 asset(s)");
        capturedDto.TargetRole.Should().Be(SmartInventory.Domain.Auth.Enums.UserRole.Supervisor);
    }

    [Fact]
    public async Task RunAsync_BothDueSoonAndOverdue_CreatesTwoNotifications()
    {
        var dueSoonAsset = CreateAsset(DateTime.UtcNow.AddDays(3));
        var overdueAsset = CreateAsset(DateTime.UtcNow.AddDays(-1));

        _repositoryMock
            .Setup(r => r.GetAssetsWithMaintenanceAsync(It.IsAny<DateTime>(), It.Is<DateTime>(d => d >= DateTime.UtcNow.AddDays(6))))
            .ReturnsAsync(new List<AssetEntity> { dueSoonAsset });

        _repositoryMock
            .Setup(r => r.GetAssetsWithMaintenanceAsync(It.Is<DateTime>(d => d <= DateTime.UtcNow.AddDays(-1)), It.IsAny<DateTime>()))
            .ReturnsAsync(new List<AssetEntity> { overdueAsset });

        _notificationServiceMock
            .Setup(n => n.CreateNotificationAsync(It.IsAny<CreateNotificationDto>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        await _job.RunAsync(CancellationToken.None);

        _notificationServiceMock.Verify(
            n => n.CreateNotificationAsync(It.IsAny<CreateNotificationDto>(), It.IsAny<CancellationToken>()),
            Times.Exactly(2));
    }

    [Fact]
    public async Task RunAsync_MultipleAssetsDueSoon_MessageReflectsCount()
    {
        var assets = new List<AssetEntity>
        {
            CreateAsset(DateTime.UtcNow.AddDays(1)),
            CreateAsset(DateTime.UtcNow.AddDays(3)),
            CreateAsset(DateTime.UtcNow.AddDays(5))
        };

        _repositoryMock
            .Setup(r => r.GetAssetsWithMaintenanceAsync(It.IsAny<DateTime>(), It.Is<DateTime>(d => d >= DateTime.UtcNow.AddDays(6))))
            .ReturnsAsync(assets);

        _repositoryMock
            .Setup(r => r.GetAssetsWithMaintenanceAsync(It.Is<DateTime>(d => d <= DateTime.UtcNow.AddDays(-1)), It.IsAny<DateTime>()))
            .ReturnsAsync(new List<AssetEntity>());

        CreateNotificationDto? capturedDto = null;
        _notificationServiceMock
            .Setup(n => n.CreateNotificationAsync(It.IsAny<CreateNotificationDto>(), It.IsAny<CancellationToken>()))
            .Callback<CreateNotificationDto, CancellationToken>((dto, _) => capturedDto = dto)
            .Returns(Task.CompletedTask);

        await _job.RunAsync(CancellationToken.None);

        capturedDto.Should().NotBeNull();
        capturedDto!.Message.Should().Contain("3 asset(s)");
    }

    [Fact]
    public async Task RunAsync_CancellationToken_Propagated()
    {
        var ct = new CancellationTokenSource().Token;
        _repositoryMock
            .Setup(r => r.GetAssetsWithMaintenanceAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .ReturnsAsync(new List<AssetEntity>());

        await _job.RunAsync(ct);

        _repositoryMock.Verify(
            r => r.GetAssetsWithMaintenanceAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>()),
            Times.AtLeastOnce);
    }
}
