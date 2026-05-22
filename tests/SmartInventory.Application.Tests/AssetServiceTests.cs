using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Hangfire;
using Moq;
using SmartInventory.Application.Asset.DTOs;
using SmartInventory.Application.Asset.Interfaces;
using SmartInventory.Application.Asset.Services;
using SmartInventory.Domain.Asset.Entities;
using SmartInventory.Domain.Asset.Enums;
using SmartInventory.Domain.Auth.Enums;
using SmartInventory.Domain.Notification.Enums;
using SmartInventory.Application.Notification.DTOs;
using SmartInventory.Application.Caching;
using Xunit;
using AssetEntity = SmartInventory.Domain.Asset.Entities.Asset;

namespace SmartInventory.Application.Tests;

public class AssetServiceAuthorizationTests : ApplicationTestBase
{
    private readonly Mock<IAssetRepository> _repositoryMock;
    private readonly Mock<IAssetHistoryService> _historyServiceMock;
    private readonly Mock<IBackgroundJobClient> _queueMock;
    private readonly Mock<SmartInventory.Application.Notification.Interfaces.INotificationService> _notificationServiceMock;
    private readonly Mock<SmartInventory.Application.Location.Interfaces.ILocationRepository> _locationRepositoryMock;
    private readonly Mock<IActivityLogService> _activityLogServiceMock;
    private readonly AutoMapper.MapperConfiguration _config;
    private readonly AssetService _service;

    public AssetServiceAuthorizationTests()
    {
        _repositoryMock = new Mock<IAssetRepository>();
        _historyServiceMock = new Mock<IAssetHistoryService>();
        _queueMock = new Mock<IBackgroundJobClient>();
        _notificationServiceMock = new Mock<SmartInventory.Application.Notification.Interfaces.INotificationService>();
        _locationRepositoryMock = new Mock<SmartInventory.Application.Location.Interfaces.ILocationRepository>();
        _activityLogServiceMock = new Mock<IActivityLogService>();
        
        _config = new AutoMapper.MapperConfiguration(cfg => 
        {
            cfg.CreateMap<AssetDto, AssetEntity>();
            cfg.CreateMap<AssetEntity, AssetDto>();
        });

        _service = new AssetService(
            _repositoryMock.Object,
            _queueMock.Object,
            _historyServiceMock.Object,
            _notificationServiceMock.Object,
            _locationRepositoryMock.Object,
            _activityLogServiceMock.Object,
            _config.CreateMapper());
    }

    private AssetEntity CreateTestAsset(AssetStatus status = AssetStatus.Active)
    {
        return new AssetEntity
        {
            Id = Guid.NewGuid(),
            AssetTag = "AST-001",
            Name = "Dell Laptop",
            Category = "Computer",
            Status = status,
            CurrentRoomCode = "LI1"
        };
    }

    [Fact]
    public async Task UpdateStatus_TechnicianToRetired_ThrowsUnauthorizedAccessException()
    {
        var asset = CreateTestAsset(AssetStatus.Active);
        _repositoryMock.Setup(r => r.GetByIdAsync(asset.Id)).ReturnsAsync(asset);

        var act = () => _service.UpdateStatusAsync(asset.Id, AssetStatus.Retired, Guid.NewGuid(), UserRole.Technicien);

        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("*Only Supervisors can set asset status to Retired*");
    }

    [Fact]
    public async Task UpdateStatus_SupervisorToRetired_Succeeds()
    {
        var asset = CreateTestAsset(AssetStatus.Active);
        var updatedAsset = CreateTestAsset(AssetStatus.Retired);
        
        _repositoryMock.Setup(r => r.GetByIdAsync(asset.Id)).ReturnsAsync(asset);
        _repositoryMock.Setup(r => r.UpdateAsync(It.IsAny<AssetEntity>())).ReturnsAsync(updatedAsset);
        _historyServiceMock.Setup(h => h.TrackChangeAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Guid>()))
            .Returns(Task.CompletedTask);

        var result = await _service.UpdateStatusAsync(asset.Id, AssetStatus.Retired, Guid.NewGuid(), UserRole.Supervisor);

        result.Status.Should().Be(AssetStatus.Retired);
    }

    [Fact]
    public async Task UpdateStatus_TechnicianToActive_Succeeds()
    {
        var asset = CreateTestAsset(AssetStatus.Maintenance);
        var updatedAsset = CreateTestAsset(AssetStatus.Active);
        
        _repositoryMock.Setup(r => r.GetByIdAsync(asset.Id)).ReturnsAsync(asset);
        _repositoryMock.Setup(r => r.UpdateAsync(It.IsAny<AssetEntity>())).ReturnsAsync(updatedAsset);
        _historyServiceMock.Setup(h => h.TrackChangeAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Guid>()))
            .Returns(Task.CompletedTask);

        var result = await _service.UpdateStatusAsync(asset.Id, AssetStatus.Active, Guid.NewGuid(), UserRole.Technicien);

        result.Status.Should().Be(AssetStatus.Active);
    }

    [Fact]
    public async Task UpdateStatus_TechnicianToMaintenance_Succeeds()
    {
        var asset = CreateTestAsset(AssetStatus.Active);
        var updatedAsset = CreateTestAsset(AssetStatus.Maintenance);
        
        _repositoryMock.Setup(r => r.GetByIdAsync(asset.Id)).ReturnsAsync(asset);
        _repositoryMock.Setup(r => r.UpdateAsync(It.IsAny<AssetEntity>())).ReturnsAsync(updatedAsset);
        _historyServiceMock.Setup(h => h.TrackChangeAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Guid>()))
            .Returns(Task.CompletedTask);

        var result = await _service.UpdateStatusAsync(asset.Id, AssetStatus.Maintenance, Guid.NewGuid(), UserRole.Technicien);

        result.Status.Should().Be(AssetStatus.Maintenance);
    }

    [Fact]
    public async Task UpdateStatus_TechnicianToLost_Succeeds()
    {
        var asset = CreateTestAsset(AssetStatus.Active);
        var updatedAsset = CreateTestAsset(AssetStatus.Lost);
        
        _repositoryMock.Setup(r => r.GetByIdAsync(asset.Id)).ReturnsAsync(asset);
        _repositoryMock.Setup(r => r.UpdateAsync(It.IsAny<AssetEntity>())).ReturnsAsync(updatedAsset);
        _historyServiceMock.Setup(h => h.TrackChangeAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Guid>()))
            .Returns(Task.CompletedTask);

        var result = await _service.UpdateStatusAsync(asset.Id, AssetStatus.Lost, Guid.NewGuid(), UserRole.Technicien);

        result.Status.Should().Be(AssetStatus.Lost);
    }

    [Fact]
    public async Task UpdateStatus_TechnicianToCriticalIssue_Succeeds()
    {
        var asset = CreateTestAsset(AssetStatus.Active);
        var updatedAsset = CreateTestAsset(AssetStatus.CriticalIssue);
        
        _repositoryMock.Setup(r => r.GetByIdAsync(asset.Id)).ReturnsAsync(asset);
        _repositoryMock.Setup(r => r.UpdateAsync(It.IsAny<AssetEntity>())).ReturnsAsync(updatedAsset);
        _historyServiceMock.Setup(h => h.TrackChangeAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Guid>()))
            .Returns(Task.CompletedTask);

        var result = await _service.UpdateStatusAsync(asset.Id, AssetStatus.CriticalIssue, Guid.NewGuid(), UserRole.Technicien);

        result.Status.Should().Be(AssetStatus.CriticalIssue);
    }

    [Fact]
    public async Task UpdateStatus_SameStatusNoNote_ReturnsWithoutSideEffects()
    {
        var asset = CreateTestAsset(AssetStatus.Maintenance);
        _repositoryMock.Setup(r => r.GetByIdAsync(asset.Id)).ReturnsAsync(asset);

        var result = await _service.UpdateStatusAsync(asset.Id, AssetStatus.Maintenance, Guid.NewGuid(), UserRole.Technicien, note: null);

        result.Status.Should().Be(AssetStatus.Maintenance);
        _historyServiceMock.Verify(h => h.TrackChangeAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<Guid>()), Times.Never);
        _notificationServiceMock.Verify(n => n.CreateNotificationAsync(It.IsAny<CreateNotificationDto>(), It.IsAny<CancellationToken>()), Times.Never);
        _repositoryMock.Verify(r => r.UpdateAsync(It.IsAny<AssetEntity>()), Times.Never);
    }

    [Fact]
    public async Task UpdateStatus_SameStatusWithNote_UpdatesNoteOnly()
    {
        var asset = CreateTestAsset(AssetStatus.Maintenance);
        const string note = "Extended maintenance — waiting for replacement part.";
        _repositoryMock.Setup(r => r.GetByIdAsync(asset.Id)).ReturnsAsync(asset);
        _repositoryMock.Setup(r => r.UpdateAsync(It.IsAny<AssetEntity>())).ReturnsAsync(asset);
        _historyServiceMock.Setup(h => h.TrackChangeAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<Guid>()))
            .Returns(Task.CompletedTask);

        var result = await _service.UpdateStatusAsync(asset.Id, AssetStatus.Maintenance, Guid.NewGuid(), UserRole.Technicien, note);

        result.Status.Should().Be(AssetStatus.Maintenance);
        asset.StatusEntryNote.Should().Be(note);
        _historyServiceMock.Verify(
            h => h.TrackChangeAsync(asset.Id, "StatusEntryNote", null, note, It.IsAny<Guid>()), Times.Once);
        _historyServiceMock.Verify(
            h => h.TrackChangeAsync(It.IsAny<Guid>(), "Status", It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<Guid>()), Times.Never);
        _notificationServiceMock.Verify(n => n.CreateNotificationAsync(It.IsAny<CreateNotificationDto>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task UpdateStatus_SameStatusNoNote_AnyStatus_ReturnsWithoutSideEffects()
    {
        var asset = CreateTestAsset(AssetStatus.Active);
        _repositoryMock.Setup(r => r.GetByIdAsync(asset.Id)).ReturnsAsync(asset);

        var result = await _service.UpdateStatusAsync(asset.Id, AssetStatus.Active, Guid.NewGuid(), UserRole.Technicien, note: null);

        result.Status.Should().Be(AssetStatus.Active);
        _historyServiceMock.Verify(h => h.TrackChangeAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<Guid>()), Times.Never);
        _notificationServiceMock.Verify(n => n.CreateNotificationAsync(It.IsAny<CreateNotificationDto>(), It.IsAny<CancellationToken>()), Times.Never);
        _repositoryMock.Verify(r => r.UpdateAsync(It.IsAny<AssetEntity>()), Times.Never);
    }

    [Fact]
    public async Task UpdateStatus_SameStatusWithNote_CriticalIssue_UpdatesNoteOnly()
    {
        var asset = CreateTestAsset(AssetStatus.CriticalIssue);
        const string note = "Updated diagnosis — main board needs replacement.";
        _repositoryMock.Setup(r => r.GetByIdAsync(asset.Id)).ReturnsAsync(asset);
        _repositoryMock.Setup(r => r.UpdateAsync(It.IsAny<AssetEntity>())).ReturnsAsync(asset);
        _historyServiceMock.Setup(h => h.TrackChangeAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<Guid>()))
            .Returns(Task.CompletedTask);

        var result = await _service.UpdateStatusAsync(asset.Id, AssetStatus.CriticalIssue, Guid.NewGuid(), UserRole.Technicien, note);

        result.Status.Should().Be(AssetStatus.CriticalIssue);
        asset.StatusEntryNote.Should().Be(note);
        _historyServiceMock.Verify(
            h => h.TrackChangeAsync(asset.Id, "StatusEntryNote", null, note, It.IsAny<Guid>()), Times.Once);
        _historyServiceMock.Verify(
            h => h.TrackChangeAsync(It.IsAny<Guid>(), "Status", It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<Guid>()), Times.Never);
        _notificationServiceMock.Verify(n => n.CreateNotificationAsync(It.IsAny<CreateNotificationDto>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}

public class AssetMaintenanceDueDateTests : ApplicationTestBase
{
    private readonly Mock<IAssetRepository> _repositoryMock;
    private readonly Mock<IAssetHistoryService> _historyServiceMock;
    private readonly Mock<IBackgroundJobClient> _queueMock;
    private readonly Mock<SmartInventory.Application.Notification.Interfaces.INotificationService> _notificationServiceMock;
    private readonly Mock<SmartInventory.Application.Location.Interfaces.ILocationRepository> _locationRepositoryMock;
    private readonly Mock<IActivityLogService> _activityLogServiceMock;
    private readonly AssetService _service;

    public AssetMaintenanceDueDateTests()
    {
        _repositoryMock = new Mock<IAssetRepository>();
        _historyServiceMock = new Mock<IAssetHistoryService>();
        _queueMock = new Mock<IBackgroundJobClient>();
        _notificationServiceMock = new Mock<SmartInventory.Application.Notification.Interfaces.INotificationService>();
        _locationRepositoryMock = new Mock<SmartInventory.Application.Location.Interfaces.ILocationRepository>();
        _activityLogServiceMock = new Mock<IActivityLogService>();
        
        var config = new AutoMapper.MapperConfiguration(cfg => 
        {
            cfg.CreateMap<AssetDto, AssetEntity>();
            cfg.CreateMap<AssetEntity, AssetDto>();
        });

        _service = new AssetService(
            _repositoryMock.Object,
            _queueMock.Object,
            _historyServiceMock.Object,
            _notificationServiceMock.Object,
            _locationRepositoryMock.Object,
            _activityLogServiceMock.Object,
            config.CreateMapper());
    }

    private AssetEntity CreateTestAsset() => new AssetEntity
    {
        Id = Guid.NewGuid(),
        AssetTag = "AST-001",
        Name = "Dell Laptop",
        Category = "Computer",
        Status = AssetStatus.Active,
        CurrentRoomCode = "LI1"
    };

    [Fact]
    public async Task SetMaintenanceDueDate_Today_Succeeds()
    {
        var asset = CreateTestAsset();
        _repositoryMock.Setup(r => r.GetByIdAsync(asset.Id)).ReturnsAsync(asset);
        _repositoryMock.Setup(r => r.UpdateAsync(It.IsAny<AssetEntity>())).ReturnsAsync(asset);
        _historyServiceMock.Setup(h => h.TrackChangeAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Guid>()))
            .Returns(Task.CompletedTask);

        var result = await _service.SetMaintenanceDueDateAsync(asset.Id, DateTime.UtcNow.Date, Guid.NewGuid());

        result.MaintenanceDueDate.Should().NotBeNull();
    }

    [Fact]
    public async Task SetMaintenanceDueDate_Future_Succeeds()
    {
        var asset = CreateTestAsset();
        _repositoryMock.Setup(r => r.GetByIdAsync(asset.Id)).ReturnsAsync(asset);
        _repositoryMock.Setup(r => r.UpdateAsync(It.IsAny<AssetEntity>())).ReturnsAsync(asset);
        _historyServiceMock.Setup(h => h.TrackChangeAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Guid>()))
            .Returns(Task.CompletedTask);

        var result = await _service.SetMaintenanceDueDateAsync(asset.Id, DateTime.UtcNow.Date.AddDays(30), Guid.NewGuid());

        result.MaintenanceDueDate.Should().NotBeNull();
    }

    [Fact]
    public async Task SetMaintenanceDueDate_Past_ThrowsArgumentException()
    {
        var asset = CreateTestAsset();
        _repositoryMock.Setup(r => r.GetByIdAsync(asset.Id)).ReturnsAsync(asset);

        var act = () => _service.SetMaintenanceDueDateAsync(asset.Id, DateTime.UtcNow.Date.AddDays(-1), Guid.NewGuid());

        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*today or future*");
    }

    [Fact]
    public async Task SetMaintenanceDueDate_Null_ClearsDate()
    {
        var asset = CreateTestAsset();
        asset.MaintenanceDueDate = DateTime.UtcNow.Date.AddDays(7);
        
        _repositoryMock.Setup(r => r.GetByIdAsync(asset.Id)).ReturnsAsync(asset);
        _repositoryMock.Setup(r => r.UpdateAsync(It.IsAny<AssetEntity>())).ReturnsAsync(asset);
        _historyServiceMock.Setup(h => h.TrackChangeAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Guid>()))
            .Returns(Task.CompletedTask);

        var result = await _service.SetMaintenanceDueDateAsync(asset.Id, null, Guid.NewGuid());

        result.Should().NotBeNull();
    }
}

public class AssetCreationTrackingTests : ApplicationTestBase
{
    private readonly Mock<IAssetRepository> _repositoryMock;
    private readonly Mock<IAssetHistoryService> _historyServiceMock;
    private readonly Mock<IBackgroundJobClient> _queueMock;
    private readonly Mock<SmartInventory.Application.Notification.Interfaces.INotificationService> _notificationServiceMock;
    private readonly Mock<SmartInventory.Application.Location.Interfaces.ILocationRepository> _locationRepositoryMock;
    private readonly Mock<IActivityLogService> _activityLogServiceMock;
    private readonly Mock<IAssetService> _assetServiceMock;
    private readonly AssetService _service;

    public AssetCreationTrackingTests()
    {
        _repositoryMock = new Mock<IAssetRepository>();
        _historyServiceMock = new Mock<IAssetHistoryService>();
        _queueMock = new Mock<IBackgroundJobClient>();
        _notificationServiceMock = new Mock<SmartInventory.Application.Notification.Interfaces.INotificationService>();
        _locationRepositoryMock = new Mock<SmartInventory.Application.Location.Interfaces.ILocationRepository>();
        _activityLogServiceMock = new Mock<IActivityLogService>();
        _assetServiceMock = new Mock<IAssetService>();

        var config = new AutoMapper.MapperConfiguration(cfg =>
        {
            cfg.CreateMap<CreateAssetDto, SmartInventory.Domain.Asset.Entities.Asset>();
            cfg.CreateMap<SmartInventory.Domain.Asset.Entities.Asset, AssetDto>();
        });

        _service = new AssetService(
            _repositoryMock.Object,
            _queueMock.Object,
            _historyServiceMock.Object,
            _notificationServiceMock.Object,
            _locationRepositoryMock.Object,
            _activityLogServiceMock.Object,
            config.CreateMapper());
    }

    [Fact]
    public async Task CreateAssetAsync_WithValidDto_ShouldLogCreationActivity()
    {
        var dto = new CreateAssetDto
        {
            AssetTag = "AST-NEW-001",
            Name = "New Asset",
            Category = "Computer",
            CurrentRoomCode = "LI1"
        };
        var userId = Guid.NewGuid();
        var createdAsset = new SmartInventory.Domain.Asset.Entities.Asset
        {
            Id = Guid.NewGuid(),
            AssetTag = "AST-NEW-001",
            Name = "New Asset",
            Category = "Computer",
            CurrentRoomCode = "LI1"
        };

        _repositoryMock.Setup(r => r.IsAssetTagUniqueAsync(dto.AssetTag, null)).ReturnsAsync(true);
        _repositoryMock.Setup(r => r.IsRoomCodeValidAsync(dto.CurrentRoomCode)).ReturnsAsync(true);
        _repositoryMock.Setup(r => r.AddAsync(It.IsAny<SmartInventory.Domain.Asset.Entities.Asset>()))
            .ReturnsAsync(createdAsset);

        var result = await _service.CreateAssetAsync(dto, userId);

        result.Should().NotBeNull();
        result.AssetTag.Should().Be("AST-NEW-001");
        _activityLogServiceMock.Verify(a => a.TrackFacilityChangeAsync(
            "Created",
            "Asset",
            createdAsset.AssetTag,
            createdAsset.Name,
            null,
            userId), Times.Once);
    }

    [Fact]
    public async Task CreateAssetAsync_ShouldGenerateAssetTag_WhenNotProvided()
    {
        var dto = new CreateAssetDto
        {
            Name = "Asset Without Tag",
            Category = "Computer",
            CurrentRoomCode = "LI1"
        };
        var userId = Guid.NewGuid();

        _repositoryMock.Setup(r => r.IsRoomCodeValidAsync(dto.CurrentRoomCode)).ReturnsAsync(true);
        _repositoryMock.Setup(r => r.AddAsync(It.IsAny<SmartInventory.Domain.Asset.Entities.Asset>()))
            .ReturnsAsync((SmartInventory.Domain.Asset.Entities.Asset a) => a);

        var result = await _service.CreateAssetAsync(dto, userId);

        result.Should().NotBeNull();
        result.AssetTag.Should().StartWith("AST-");
        _activityLogServiceMock.Verify(a => a.TrackFacilityChangeAsync(
            "Created",
            "Asset",
            It.Is<string>(tag => tag != null && tag.Length >= 4 && tag.Substring(0, 4) == "AST-"),
            "Asset Without Tag",
            null,
            userId), Times.Once);
    }

    [Fact]
    public async Task CreateAssetAsync_ShouldTrackActivityWithCorrectUserId()
    {
        var dto = new CreateAssetDto
        {
            AssetTag = "AST-USER-001",
            Name = "User Specific Asset",
            Category = "Monitor",
            CurrentRoomCode = "LI2"
        };
        var specificUserId = Guid.NewGuid();

        _repositoryMock.Setup(r => r.IsAssetTagUniqueAsync(dto.AssetTag, null)).ReturnsAsync(true);
        _repositoryMock.Setup(r => r.IsRoomCodeValidAsync(dto.CurrentRoomCode)).ReturnsAsync(true);
        _repositoryMock.Setup(r => r.AddAsync(It.IsAny<SmartInventory.Domain.Asset.Entities.Asset>()))
            .ReturnsAsync((SmartInventory.Domain.Asset.Entities.Asset a) => a);

        await _service.CreateAssetAsync(dto, specificUserId);

        _activityLogServiceMock.Verify(
            a => a.TrackFacilityChangeAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string?>(),
                specificUserId),
            Times.Once);
    }

    [Fact]
    public async Task CreateAssetAsync_SendsCreationNotification()
    {
        var dto = new CreateAssetDto
        {
            AssetTag = "AST-NOTIFY-001",
            Name = "Notification Test Asset",
            Category = "Computer",
            CurrentRoomCode = "LI1"
        };
        var userId = Guid.NewGuid();
        var createdAsset = new SmartInventory.Domain.Asset.Entities.Asset
        {
            Id = Guid.NewGuid(),
            AssetTag = "AST-NOTIFY-001",
            Name = "Notification Test Asset",
            Category = "Computer",
            CurrentRoomCode = "LI1"
        };

        _repositoryMock.Setup(r => r.IsAssetTagUniqueAsync(dto.AssetTag, null)).ReturnsAsync(true);
        _repositoryMock.Setup(r => r.IsRoomCodeValidAsync(dto.CurrentRoomCode)).ReturnsAsync(true);
        _repositoryMock.Setup(r => r.AddAsync(It.IsAny<SmartInventory.Domain.Asset.Entities.Asset>()))
            .ReturnsAsync(createdAsset);

        await _service.CreateAssetAsync(dto, userId);

        _notificationServiceMock.Verify(
            n => n.CreateNotificationAsync(
                It.Is<CreateNotificationDto>(d =>
                    d.EventType == NotificationEventType.EquipmentCrudCreated &&
                    d.Type == NotificationType.Info &&
                    d.TargetRole == UserRole.Supervisor &&
                    d.AssetId == createdAsset.Id),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task DeleteAssetAsync_SendsDeletionNotification()
    {
        var userId = Guid.NewGuid();
        var asset = new SmartInventory.Domain.Asset.Entities.Asset
        {
            Id = Guid.NewGuid(),
            AssetTag = "AST-DELETE-001",
            Name = "Delete Test Asset",
            Category = "Computer",
            CurrentRoomCode = "LI1",
            Status = AssetStatus.Retired
        };

        _repositoryMock.Setup(r => r.GetByIdAsync(asset.Id)).ReturnsAsync(asset);
        _repositoryMock.Setup(r => r.DeleteAsync(asset.Id)).Returns(Task.CompletedTask);
        _historyServiceMock
            .Setup(h => h.TrackChangeAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Guid>()))
            .Returns(Task.CompletedTask);

        await _service.DeleteAssetAsync(asset.Id.ToString(), userId);

        _notificationServiceMock.Verify(
            n => n.CreateNotificationAsync(
                It.Is<CreateNotificationDto>(d =>
                    d.EventType == NotificationEventType.EquipmentCrudDeleted &&
                    d.Type == NotificationType.Warning &&
                    d.TargetRole == UserRole.Supervisor &&
                    d.AssetId == asset.Id),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
}

public class AssetFieldUpdateTests : ApplicationTestBase
{
    private readonly Mock<IAssetRepository> _repositoryMock;
    private readonly Mock<IAssetHistoryService> _historyServiceMock;
    private readonly Mock<IBackgroundJobClient> _queueMock;
    private readonly Mock<SmartInventory.Application.Notification.Interfaces.INotificationService> _notificationServiceMock;
    private readonly Mock<SmartInventory.Application.Location.Interfaces.ILocationRepository> _locationRepositoryMock;
    private readonly Mock<IActivityLogService> _activityLogServiceMock;
    private readonly AssetService _service;

    public AssetFieldUpdateTests()
    {
        _repositoryMock = new Mock<IAssetRepository>();
        _historyServiceMock = new Mock<IAssetHistoryService>();
        _queueMock = new Mock<IBackgroundJobClient>();
        _notificationServiceMock = new Mock<SmartInventory.Application.Notification.Interfaces.INotificationService>();
        _locationRepositoryMock = new Mock<SmartInventory.Application.Location.Interfaces.ILocationRepository>();
        _activityLogServiceMock = new Mock<IActivityLogService>();

        var config = new AutoMapper.MapperConfiguration(cfg =>
        {
            cfg.CreateMap<AssetEntity, AssetDto>();
        });

        _service = new AssetService(
            _repositoryMock.Object,
            _queueMock.Object,
            _historyServiceMock.Object,
            _notificationServiceMock.Object,
            _locationRepositoryMock.Object,
            _activityLogServiceMock.Object,
            config.CreateMapper());
    }

    private AssetEntity CreateTestAsset() => new AssetEntity
    {
        Id = Guid.NewGuid(),
        AssetTag = "AST-001",
        Name = "Dell Laptop",
        Category = "Computer",
        Status = AssetStatus.Active,
        CurrentRoomCode = "LI1"
    };

    // ─── BLE ID Tests ────────────────────────────────────────

    [Fact]
    public async Task UpdateBleIdAsync_Success()
    {
        var asset = CreateTestAsset();
        _repositoryMock.Setup(r => r.GetByIdAsync(asset.Id)).ReturnsAsync(asset);
        _repositoryMock.Setup(r => r.IsBleIdUniqueAsync("BLE-001", asset.Id)).ReturnsAsync(true);
        _repositoryMock.Setup(r => r.UpdateAsync(It.IsAny<AssetEntity>())).ReturnsAsync(asset);
        _historyServiceMock.Setup(h => h.TrackChangeAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<Guid>()))
            .Returns(Task.CompletedTask);

        var result = await _service.UpdateBleIdAsync(asset.Id, "BLE-001", Guid.NewGuid());

        result.Should().NotBeNull();
        asset.BleId.Should().Be("BLE-001");
    }

    [Fact]
    public async Task UpdateBleIdAsync_Duplicate_ThrowsInvalidOperationException()
    {
        var asset = CreateTestAsset();
        _repositoryMock.Setup(r => r.GetByIdAsync(asset.Id)).ReturnsAsync(asset);
        _repositoryMock.Setup(r => r.IsBleIdUniqueAsync("BLE-001", asset.Id)).ReturnsAsync(false);

        var act = () => _service.UpdateBleIdAsync(asset.Id, "BLE-001", Guid.NewGuid());

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*already assigned*");
    }

    [Fact]
    public async Task UpdateBleIdAsync_AssetNotFound_ThrowsArgumentException()
    {
        var id = Guid.NewGuid();
        _repositoryMock.Setup(r => r.GetByIdAsync(id)).ReturnsAsync((AssetEntity?)null);

        var act = () => _service.UpdateBleIdAsync(id, "BLE-001", Guid.NewGuid());

        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task UpdateBleIdAsync_ClearsBleId()
    {
        var asset = CreateTestAsset();
        asset.BleId = "BLE-OLD";
        _repositoryMock.Setup(r => r.GetByIdAsync(asset.Id)).ReturnsAsync(asset);
        _repositoryMock.Setup(r => r.UpdateAsync(It.IsAny<AssetEntity>())).ReturnsAsync(asset);
        _historyServiceMock.Setup(h => h.TrackChangeAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<Guid>()))
            .Returns(Task.CompletedTask);

        var result = await _service.UpdateBleIdAsync(asset.Id, null, Guid.NewGuid());

        result.Should().NotBeNull();
        asset.BleId.Should().BeNull();
    }

    // ─── Price Tests ─────────────────────────────────────────

    [Fact]
    public async Task UpdatePriceAsync_Success()
    {
        var asset = CreateTestAsset();
        _repositoryMock.Setup(r => r.GetByIdAsync(asset.Id)).ReturnsAsync(asset);
        _repositoryMock.Setup(r => r.UpdateAsync(It.IsAny<AssetEntity>())).ReturnsAsync(asset);
        _historyServiceMock.Setup(h => h.TrackChangeAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<Guid>()))
            .Returns(Task.CompletedTask);

        var result = await _service.UpdatePriceAsync(asset.Id, "1500.00", Guid.NewGuid());

        result.Should().NotBeNull();
        asset.Price.Should().Be("1500.00");
    }

    [Fact]
    public async Task UpdatePriceAsync_AssetNotFound_ThrowsArgumentException()
    {
        var id = Guid.NewGuid();
        _repositoryMock.Setup(r => r.GetByIdAsync(id)).ReturnsAsync((AssetEntity?)null);

        var act = () => _service.UpdatePriceAsync(id, "1500.00", Guid.NewGuid());

        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task UpdatePriceAsync_ClearsPrice()
    {
        var asset = CreateTestAsset();
        asset.Price = "2000.00";
        _repositoryMock.Setup(r => r.GetByIdAsync(asset.Id)).ReturnsAsync(asset);
        _repositoryMock.Setup(r => r.UpdateAsync(It.IsAny<AssetEntity>())).ReturnsAsync(asset);
        _historyServiceMock.Setup(h => h.TrackChangeAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<Guid>()))
            .Returns(Task.CompletedTask);

        var result = await _service.UpdatePriceAsync(asset.Id, null, Guid.NewGuid());

        result.Should().NotBeNull();
        asset.Price.Should().BeNull();
    }
}

public class AssetFieldUpdateNotificationTests : ApplicationTestBase
{
    private readonly Mock<IAssetRepository> _repositoryMock;
    private readonly Mock<IAssetHistoryService> _historyServiceMock;
    private readonly Mock<IBackgroundJobClient> _queueMock;
    private readonly Mock<SmartInventory.Application.Notification.Interfaces.INotificationService> _notificationServiceMock;
    private readonly Mock<SmartInventory.Application.Location.Interfaces.ILocationRepository> _locationRepositoryMock;
    private readonly Mock<IActivityLogService> _activityLogServiceMock;
    private readonly AssetService _service;

    public AssetFieldUpdateNotificationTests()
    {
        _repositoryMock = new Mock<IAssetRepository>();
        _historyServiceMock = new Mock<IAssetHistoryService>();
        _queueMock = new Mock<IBackgroundJobClient>();
        _notificationServiceMock = new Mock<SmartInventory.Application.Notification.Interfaces.INotificationService>();
        _locationRepositoryMock = new Mock<SmartInventory.Application.Location.Interfaces.ILocationRepository>();
        _activityLogServiceMock = new Mock<IActivityLogService>();

        var config = new AutoMapper.MapperConfiguration(cfg =>
        {
            cfg.CreateMap<AssetEntity, AssetDto>();
        });

        _service = new AssetService(
            _repositoryMock.Object,
            _queueMock.Object,
            _historyServiceMock.Object,
            _notificationServiceMock.Object,
            _locationRepositoryMock.Object,
            _activityLogServiceMock.Object,
            config.CreateMapper());
    }

    private AssetEntity CreateTestAsset() => new AssetEntity
    {
        Id = Guid.NewGuid(),
        AssetTag = "AST-001",
        Name = "Dell Laptop",
        Category = "Computer",
        Status = AssetStatus.Active,
        CurrentRoomCode = "LI1"
    };

    [Fact]
    public async Task UpdateAssetAsync_SendsUpdateNotification()
    {
        var asset = CreateTestAsset();
        var userId = Guid.NewGuid();
        var dto = new UpdateAssetDto
        {
            AssetTag = asset.AssetTag,
            Name = "Updated Name",
            Description = "Updated description",
            Category = asset.Category,
            Status = asset.Status,
            CurrentRoomCode = asset.CurrentRoomCode
        };

        _repositoryMock.Setup(r => r.GetByIdAsync(asset.Id)).ReturnsAsync(asset);
        _repositoryMock.Setup(r => r.IsAssetTagUniqueAsync(dto.AssetTag, asset.Id)).ReturnsAsync(true);
        _repositoryMock.Setup(r => r.IsRoomCodeValidAsync(dto.CurrentRoomCode)).ReturnsAsync(true);
        _repositoryMock.Setup(r => r.UpdateAsync(It.IsAny<AssetEntity>())).ReturnsAsync(asset);
        _historyServiceMock.Setup(h => h.TrackChangeAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<Guid>()))
            .Returns(Task.CompletedTask);

        await _service.UpdateAssetAsync(asset.Id, dto, userId);

        _notificationServiceMock.Verify(
            n => n.CreateNotificationAsync(
                It.Is<CreateNotificationDto>(d =>
                    d.EventType == NotificationEventType.EquipmentCrudUpdated &&
                    d.Type == NotificationType.Info &&
                    d.TargetRole == UserRole.Supervisor &&
                    d.AssetId == asset.Id),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task SetMaintenanceDueDateAsync_SendsScheduledNotification()
    {
        var asset = CreateTestAsset();
        var userId = Guid.NewGuid();
        var dueDate = DateTime.UtcNow.AddDays(14);

        _repositoryMock.Setup(r => r.GetByIdAsync(asset.Id)).ReturnsAsync(asset);
        _repositoryMock.Setup(r => r.UpdateAsync(It.IsAny<AssetEntity>())).ReturnsAsync(asset);
        _historyServiceMock.Setup(h => h.TrackChangeAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<Guid>()))
            .Returns(Task.CompletedTask);

        await _service.SetMaintenanceDueDateAsync(asset.Id, dueDate, userId);

        _notificationServiceMock.Verify(
            n => n.CreateNotificationAsync(
                It.Is<CreateNotificationDto>(d =>
                    d.EventType == NotificationEventType.MaintenanceScheduled &&
                    d.Type == NotificationType.Info &&
                    d.TargetRole == UserRole.Technicien &&
                    d.AssetId == asset.Id),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task SetMaintenanceDueDateAsync_ClearingDate_DoesNotSendNotification()
    {
        var asset = CreateTestAsset();
        var userId = Guid.NewGuid();

        _repositoryMock.Setup(r => r.GetByIdAsync(asset.Id)).ReturnsAsync(asset);
        _repositoryMock.Setup(r => r.UpdateAsync(It.IsAny<AssetEntity>())).ReturnsAsync(asset);
        _historyServiceMock.Setup(h => h.TrackChangeAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<Guid>()))
            .Returns(Task.CompletedTask);

        await _service.SetMaintenanceDueDateAsync(asset.Id, null, userId);

        _notificationServiceMock.Verify(
            n => n.CreateNotificationAsync(
                It.IsAny<CreateNotificationDto>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }
}

public class AssetBarcodeGenerationTests : ApplicationTestBase
{
    private readonly Mock<IAssetRepository> _repositoryMock;
    private readonly Mock<IAssetHistoryService> _historyServiceMock;
    private readonly Mock<IBackgroundJobClient> _queueMock;
    private readonly Mock<SmartInventory.Application.Notification.Interfaces.INotificationService> _notificationServiceMock;
    private readonly Mock<SmartInventory.Application.Location.Interfaces.ILocationRepository> _locationRepositoryMock;
    private readonly Mock<IActivityLogService> _activityLogServiceMock;
    private readonly AssetService _service;

    public AssetBarcodeGenerationTests()
    {
        _repositoryMock = new Mock<IAssetRepository>();
        _historyServiceMock = new Mock<IAssetHistoryService>();
        _queueMock = new Mock<IBackgroundJobClient>();
        _notificationServiceMock = new Mock<SmartInventory.Application.Notification.Interfaces.INotificationService>();
        _locationRepositoryMock = new Mock<SmartInventory.Application.Location.Interfaces.ILocationRepository>();
        _activityLogServiceMock = new Mock<IActivityLogService>();

        var config = new AutoMapper.MapperConfiguration(cfg =>
        {
            cfg.CreateMap<AssetEntity, AssetDto>();
        });

        _service = new AssetService(
            _repositoryMock.Object,
            _queueMock.Object,
            _historyServiceMock.Object,
            _notificationServiceMock.Object,
            _locationRepositoryMock.Object,
            _activityLogServiceMock.Object,
            config.CreateMapper());
    }

    private AssetEntity CreateTestAsset() => new AssetEntity
    {
        Id = Guid.NewGuid(),
        AssetTag = "AST-001",
        Name = "Dell Laptop",
        Category = "Computer",
        Status = AssetStatus.Active,
        CurrentRoomCode = "LI1"
    };

    [Fact]
    public async Task GenerateBarcodeAsync_ValidAsset_ReturnsPngBytes()
    {
        var asset = CreateTestAsset();
        _repositoryMock.Setup(r => r.GetByIdAsync(asset.Id)).ReturnsAsync(asset);

        var result = await _service.GenerateBarcodeAsync(asset.Id, 300, 80);

        result.Should().NotBeNull();
        result.Length.Should().BeGreaterThan(0);
        result[0].Should().Be(0x89);
        result[1].Should().Be(0x50);
        result[2].Should().Be(0x4E);
        result[3].Should().Be(0x47);
    }

    [Fact]
    public async Task GenerateBarcodeAsync_AssetNotFound_ThrowsArgumentException()
    {
        var id = Guid.NewGuid();
        _repositoryMock.Setup(r => r.GetByIdAsync(id)).ReturnsAsync((AssetEntity?)null);

        var act = () => _service.GenerateBarcodeAsync(id, 300, 80);

        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*Asset with ID*not found*");
    }

    [Fact]
    public async Task GenerateBarcodeAsync_DifferentTags_ReturnsDifferentImages()
    {
        var asset1 = CreateTestAsset();
        asset1.AssetTag = "AST-001";
        var asset2 = CreateTestAsset();
        asset2.AssetTag = "AST-002";

        _repositoryMock.Setup(r => r.GetByIdAsync(asset1.Id)).ReturnsAsync(asset1);
        _repositoryMock.Setup(r => r.GetByIdAsync(asset2.Id)).ReturnsAsync(asset2);

        var result1 = await _service.GenerateBarcodeAsync(asset1.Id, 300, 80);
        var result2 = await _service.GenerateBarcodeAsync(asset2.Id, 300, 80);

        result1.Should().NotBeNull();
        result2.Should().NotBeNull();
        Convert.ToBase64String(result1).Should().NotBe(Convert.ToBase64String(result2));
    }
}

public class AssetCodeCacheTests : ApplicationTestBase
{
    private readonly Mock<IAssetRepository> _repositoryMock;
    private readonly Mock<IBlobCacheService> _blobCacheMock;
    private readonly AssetService _service;

    public AssetCodeCacheTests()
    {
        _repositoryMock = new Mock<IAssetRepository>();
        _blobCacheMock = new Mock<IBlobCacheService>();

        _service = new AssetService(
            _repositoryMock.Object,
            Mock.Of<IBackgroundJobClient>(),
            Mock.Of<IAssetHistoryService>(),
            Mock.Of<SmartInventory.Application.Notification.Interfaces.INotificationService>(),
            Mock.Of<SmartInventory.Application.Location.Interfaces.ILocationRepository>(),
            Mock.Of<IActivityLogService>(),
            Mock.Of<AutoMapper.IMapper>(),
            null,
            _blobCacheMock.Object);
    }

    private AssetEntity CreateTestAsset() => new AssetEntity
    {
        Id = Guid.NewGuid(),
        AssetTag = "AST-001",
        Name = "Dell Laptop",
        Category = "Computer",
        Status = AssetStatus.Active,
        CurrentRoomCode = "LI1"
    };

    [Fact]
    public async Task GenerateQrCodeAsync_WhenCached_ReturnsCachedBytes()
    {
        var asset = CreateTestAsset();
        var cachedBytes = new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A };
        _repositoryMock.Setup(r => r.GetByIdAsync(asset.Id)).ReturnsAsync(asset);
        _blobCacheMock.Setup(b => b.GetAsync($"qrcodes/asset-{asset.Id}.png", It.IsAny<CancellationToken>()))
            .ReturnsAsync(cachedBytes);

        var result = await _service.GenerateQrCodeAsync(asset.Id);

        result.Should().BeSameAs(cachedBytes);
        _blobCacheMock.Verify(b => b.GetAsync($"qrcodes/asset-{asset.Id}.png", It.IsAny<CancellationToken>()), Times.Once);
        _blobCacheMock.Verify(b => b.SetAsync(It.IsAny<string>(), It.IsAny<byte[]>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task GenerateQrCodeAsync_WhenNotCached_GeneratesAndCaches()
    {
        var asset = CreateTestAsset();
        _repositoryMock.Setup(r => r.GetByIdAsync(asset.Id)).ReturnsAsync(asset);
        _blobCacheMock.Setup(b => b.GetAsync($"qrcodes/asset-{asset.Id}.png", It.IsAny<CancellationToken>()))
            .ReturnsAsync((byte[]?)null);

        var result = await _service.GenerateQrCodeAsync(asset.Id);

        result.Should().NotBeNull();
        result.Length.Should().BeGreaterThan(0);
        _blobCacheMock.Verify(b => b.GetAsync($"qrcodes/asset-{asset.Id}.png", It.IsAny<CancellationToken>()), Times.Once);
        _blobCacheMock.Verify(b => b.SetAsync($"qrcodes/asset-{asset.Id}.png", result, "image/png", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GenerateBarcodeAsync_WhenCached_ReturnsCachedBytes()
    {
        var asset = CreateTestAsset();
        var cachedBytes = new byte[] { 0x89, 0x50, 0x4E, 0x47 };
        _repositoryMock.Setup(r => r.GetByIdAsync(asset.Id)).ReturnsAsync(asset);
        _blobCacheMock.Setup(b => b.GetAsync($"barcodes/asset-{asset.Id}.png", It.IsAny<CancellationToken>()))
            .ReturnsAsync(cachedBytes);

        var result = await _service.GenerateBarcodeAsync(asset.Id, 300, 80);

        result.Should().BeSameAs(cachedBytes);
        _blobCacheMock.Verify(b => b.SetAsync(It.IsAny<string>(), It.IsAny<byte[]>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task GenerateBarcodeAsync_WhenNotCached_GeneratesAndCaches()
    {
        var asset = CreateTestAsset();
        _repositoryMock.Setup(r => r.GetByIdAsync(asset.Id)).ReturnsAsync(asset);
        _blobCacheMock.Setup(b => b.GetAsync($"barcodes/asset-{asset.Id}.png", It.IsAny<CancellationToken>()))
            .ReturnsAsync((byte[]?)null);

        var result = await _service.GenerateBarcodeAsync(asset.Id, 300, 80);

        result.Should().NotBeNull();
        result.Length.Should().BeGreaterThan(0);
        _blobCacheMock.Verify(b => b.SetAsync($"barcodes/asset-{asset.Id}.png", result, "image/png", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GenerateQrCodeAsync_WhenBlobCacheNull_FallsThroughToGeneration()
    {
        var asset = CreateTestAsset();
        var serviceWithoutCache = new AssetService(
            _repositoryMock.Object,
            Mock.Of<IBackgroundJobClient>(),
            Mock.Of<IAssetHistoryService>(),
            Mock.Of<SmartInventory.Application.Notification.Interfaces.INotificationService>(),
            Mock.Of<SmartInventory.Application.Location.Interfaces.ILocationRepository>(),
            Mock.Of<IActivityLogService>(),
            Mock.Of<AutoMapper.IMapper>());

        _repositoryMock.Setup(r => r.GetByIdAsync(asset.Id)).ReturnsAsync(asset);

        var result = await serviceWithoutCache.GenerateQrCodeAsync(asset.Id);

        result.Should().NotBeNull();
        result.Length.Should().BeGreaterThan(0);
    }
}