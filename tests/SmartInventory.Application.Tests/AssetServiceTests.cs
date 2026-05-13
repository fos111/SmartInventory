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
}