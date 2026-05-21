using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using SmartInventory.Application.Asset.DTOs;
using SmartInventory.Application.Asset.Interfaces;
using SmartInventory.Application.Mobile.Sync.DTOs;
using SmartInventory.Application.Mobile.Sync.Interfaces;
using SmartInventory.Application.Mobile.Sync.Services;
using SmartInventory.Domain.Asset.Enums;
using SmartInventory.Domain.Auth.Enums;
using SmartInventory.Domain.Mobile.Entities;
using Xunit;

namespace SmartInventory.Application.Tests.Mobile.Sync.Services;

public class MobileSyncServiceTests
{
    private readonly Mock<IAssetService> _assetServiceMock;
    private readonly Mock<ISyncQueueService> _syncQueueServiceMock;
    private readonly Mock<IActivityLogService> _activityLogServiceMock;
    private readonly MobileSyncService _service;
    private readonly Guid _userId = Guid.NewGuid();

    public MobileSyncServiceTests()
    {
        _assetServiceMock = new Mock<IAssetService>();
        _syncQueueServiceMock = new Mock<ISyncQueueService>();
        _activityLogServiceMock = new Mock<IActivityLogService>();

        _activityLogServiceMock
            .Setup(s => s.TrackFacilityChangeAsync(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<Guid>()))
            .Returns(Task.CompletedTask);

        _service = new MobileSyncService(
            _assetServiceMock.Object,
            _syncQueueServiceMock.Object,
            _activityLogServiceMock.Object);
    }

    [Fact]
    public async Task ProcessBatchAsync_ValidMove_CallsMoveAssetAsync()
    {
        var asset = new AssetDto
        {
            Id = Guid.NewGuid(),
            AssetTag = "AST-001",
            Name = "Test Asset",
            Status = AssetStatus.Active,
            UpdatedAt = DateTime.UtcNow.AddDays(-2)
        };

        _assetServiceMock
            .Setup(s => s.GetAssetByTagAsync(asset.AssetTag))
            .ReturnsAsync(asset);

        _assetServiceMock
            .Setup(s => s.MoveAssetAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<Guid>()))
            .ReturnsAsync(asset);

        var request = new BatchOperationRequest
        {
            Operations = new List<BatchAssetOperationDto>
            {
                new()
                {
                    AssetTag = "AST-001",
                    OperationType = "Move",
                    TargetRoomCode = "Room-B1-01",
                    PerformedAt = DateTime.UtcNow
                }
            }
        };

        var result = await _service.ProcessBatchAsync(request, _userId);

        _assetServiceMock.Verify(
            s => s.MoveAssetAsync(asset.Id, "Room-B1-01", _userId), Times.Once);
        result[0].Success.Should().BeTrue();
    }

    [Fact]
    public async Task ProcessBatchAsync_ValidStatusChange_CallsUpdateStatusAsync()
    {
        var asset = new AssetDto
        {
            Id = Guid.NewGuid(),
            AssetTag = "AST-001",
            Name = "Test Asset",
            Status = AssetStatus.Active,
            UpdatedAt = DateTime.UtcNow.AddDays(-2)
        };

        _assetServiceMock
            .Setup(s => s.GetAssetByTagAsync(asset.AssetTag))
            .ReturnsAsync(asset);

        _assetServiceMock
            .Setup(s => s.UpdateStatusAsync(
                It.IsAny<Guid>(), It.IsAny<AssetStatus>(), It.IsAny<Guid>(), It.IsAny<UserRole>(), It.IsAny<string?>()))
            .ReturnsAsync(asset);

        var request = new BatchOperationRequest
        {
            Operations = new List<BatchAssetOperationDto>
            {
                new()
                {
                    AssetTag = "AST-001",
                    OperationType = "StatusChange",
                    NewStatus = "Maintenance",
                    PerformedAt = DateTime.UtcNow
                }
            }
        };

        var result = await _service.ProcessBatchAsync(request, _userId);

        _assetServiceMock.Verify(
            s => s.UpdateStatusAsync(asset.Id, AssetStatus.Maintenance, _userId, UserRole.Technicien, It.IsAny<string?>()), Times.Once);
        result[0].Success.Should().BeTrue();
    }

    [Fact]
    public async Task ProcessBatchAsync_ConflictDetection_ReturnsFailure()
    {
        var asset = new AssetDto
        {
            Id = Guid.NewGuid(),
            AssetTag = "AST-001",
            Name = "Test Asset",
            Status = AssetStatus.Active,
            UpdatedAt = DateTime.UtcNow
        };

        _assetServiceMock
            .Setup(s => s.GetAssetByTagAsync(asset.AssetTag))
            .ReturnsAsync(asset);

        var request = new BatchOperationRequest
        {
            Operations = new List<BatchAssetOperationDto>
            {
                new()
                {
                    AssetTag = "AST-001",
                    OperationType = "Move",
                    TargetRoomCode = "Room-B1-01",
                    PerformedAt = DateTime.UtcNow.AddDays(-1)
                }
            }
        };

        var result = await _service.ProcessBatchAsync(request, _userId);

        _assetServiceMock.Verify(
            s => s.MoveAssetAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<Guid>()), Times.Never);
        result[0].Success.Should().BeFalse();
        result[0].Error.Should().Contain("Conflict");
    }

    [Fact]
    public async Task ProcessBatchAsync_NonExistentAsset_ReturnsNotFound()
    {
        _assetServiceMock
            .Setup(s => s.GetAssetByTagAsync(It.IsAny<string>()))
            .ReturnsAsync((AssetDto?)null);

        var request = new BatchOperationRequest
        {
            Operations = new List<BatchAssetOperationDto>
            {
                new()
                {
                    AssetTag = "AST-999",
                    OperationType = "Move",
                    TargetRoomCode = "Room-B1-01",
                    PerformedAt = DateTime.UtcNow
                }
            }
        };

        var result = await _service.ProcessBatchAsync(request, _userId);

        result[0].Success.Should().BeFalse();
        result[0].Error.Should().Contain("not found");
    }

    [Fact]
    public async Task ProcessBatchAsync_UnsupportedOperation_ReturnsError()
    {
        var asset = new AssetDto
        {
            Id = Guid.NewGuid(),
            AssetTag = "AST-001",
            Name = "Test Asset",
            Status = AssetStatus.Active,
            UpdatedAt = DateTime.UtcNow.AddDays(-2)
        };

        _assetServiceMock
            .Setup(s => s.GetAssetByTagAsync(asset.AssetTag))
            .ReturnsAsync(asset);

        var request = new BatchOperationRequest
        {
            Operations = new List<BatchAssetOperationDto>
            {
                new()
                {
                    AssetTag = "AST-001",
                    OperationType = "InvalidOp",
                    PerformedAt = DateTime.UtcNow
                }
            }
        };

        var result = await _service.ProcessBatchAsync(request, _userId);

        result[0].Success.Should().BeFalse();
        result[0].Error.Should().Contain("Unsupported");
    }

    [Fact]
    public async Task ProcessBatchAsync_MixedResults_ReturnsPerItemResults()
    {
        var asset = new AssetDto
        {
            Id = Guid.NewGuid(),
            AssetTag = "AST-001",
            Name = "Test Asset",
            Status = AssetStatus.Active,
            UpdatedAt = DateTime.UtcNow.AddDays(-2)
        };

        _assetServiceMock
            .SetupSequence(s => s.GetAssetByTagAsync(It.IsAny<string>()))
            .ReturnsAsync(asset)
            .ReturnsAsync((AssetDto?)null)
            .ReturnsAsync(asset);

        _assetServiceMock
            .Setup(s => s.MoveAssetAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<Guid>()))
            .ReturnsAsync(asset);

        var request = new BatchOperationRequest
        {
            Operations = new List<BatchAssetOperationDto>
            {
                new()
                {
                    AssetTag = "AST-001",
                    OperationType = "Move",
                    TargetRoomCode = "Room-B1-01",
                    PerformedAt = DateTime.UtcNow
                },
                new()
                {
                    AssetTag = "AST-999",
                    OperationType = "Move",
                    TargetRoomCode = "Room-B1-02",
                    PerformedAt = DateTime.UtcNow
                },
                new()
                {
                    AssetTag = "AST-003",
                    OperationType = "InvalidOp",
                    PerformedAt = DateTime.UtcNow
                }
            }
        };

        var result = await _service.ProcessBatchAsync(request, _userId);

        result[0].Success.Should().BeTrue();
        result[1].Success.Should().BeFalse();
        result[2].Success.Should().BeFalse();

        _activityLogServiceMock.Verify(
            s => s.TrackFacilityChangeAsync(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<Guid>()),
            Times.Once);
    }

    [Fact]
    public async Task ProcessQueueAsync_EnqueuesAllOperations()
    {
        var batchLastSyncTimestamp = DateTime.UtcNow.AddHours(-1);

        _syncQueueServiceMock
            .Setup(s => s.EnqueueAsync(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<string>(), null, null, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new SyncQueueEntry());
        _syncQueueServiceMock
            .Setup(s => s.GetPendingCountAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(2);
        _syncQueueServiceMock
            .Setup(s => s.GetLastSyncTimestampAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(batchLastSyncTimestamp);
        var batch = new SyncBatchDto
        {
            DeviceId = "device-001",
            LastSyncTimestamp = batchLastSyncTimestamp,
            Operations = new List<SyncOperationDto>
            {
                new()
                {
                    OperationType = "Move",
                    AssetTag = "AST-001",
                    TargetRoomCode = "Room-B1-01",
                    ClientOperationId = Guid.NewGuid().ToString(),
                    PerformedAt = DateTime.UtcNow
                },
                new()
                {
                    OperationType = "StatusChange",
                    AssetTag = "AST-002",
                    NewStatus = "Maintenance",
                    ClientOperationId = Guid.NewGuid().ToString(),
                    PerformedAt = DateTime.UtcNow
                }
            }
        };

        var result = await _service.ProcessQueueAsync(batch, _userId);

        _syncQueueServiceMock.Verify(
            s => s.EnqueueAsync(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<string>(), It.IsAny<Guid?>(), It.IsAny<string?>(), It.IsAny<string?>(),
                It.IsAny<CancellationToken>()),
            Times.Exactly(2));
        result.PendingOperations.Should().Be(2);
        result.LastSyncTimestamp.Should().Be(batch.LastSyncTimestamp);
    }

    [Fact]
    public async Task GetStatusAsync_ReturnsPendingCountAndTimestamp()
    {
        var expectedTimestamp = DateTime.UtcNow.AddHours(-2);

        _syncQueueServiceMock
            .Setup(s => s.GetLastSyncTimestampAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedTimestamp);

        var result = await _service.GetStatusAsync(_userId);

        result.Should().BeOfType<SyncStatusDto>();
        result.PendingOperations.Should().Be(0);
        result.LastSyncTimestamp.Should().Be(expectedTimestamp);
    }
}
