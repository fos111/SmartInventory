using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using SmartInventory.Application.Mobile.Sync.Interfaces;
using SmartInventory.Application.Mobile.Sync.Services;
using SmartInventory.Domain.Mobile.Entities;
using Xunit;

namespace SmartInventory.Application.Tests.Mobile.Sync.Services;

public class SyncQueueServiceTests
{
    private readonly Mock<ISyncQueueRepository> _syncQueueRepositoryMock;
    private readonly SyncQueueService _service;

    public SyncQueueServiceTests()
    {
        _syncQueueRepositoryMock = new Mock<ISyncQueueRepository>();
        _service = new SyncQueueService(_syncQueueRepositoryMock.Object);
    }

    [Fact]
    public async Task EnqueueAsync_CreatesEntryWithCorrectProperties()
    {
        var deviceId = "device-001";
        var operationType = "AssetUpdated";
        var payload = "{\"id\":\"123\"}";
        var clientOperationId = Guid.NewGuid().ToString();

        var result = await _service.EnqueueAsync(deviceId, operationType, payload, clientOperationId);

        result.DeviceId.Should().Be(deviceId);
        result.OperationType.Should().Be(operationType);
        result.Payload.Should().Be(payload);
        result.ClientOperationId.Should().Be(clientOperationId);
        _syncQueueRepositoryMock.Verify(x => x.AddAsync(It.IsAny<SyncQueueEntry>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task EnqueueAsync_SetsReceivedAtToUtcNow()
    {
        var result = await _service.EnqueueAsync("device-001", "AssetUpdated", "{}", "op-1");

        result.ReceivedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public async Task ProcessPendingAsync_MarksAllPendingAsProcessed()
    {
        var entries = new List<SyncQueueEntry>
        {
            new() { Id = Guid.NewGuid(), DeviceId = "d1" },
            new() { Id = Guid.NewGuid(), DeviceId = "d2" }
        };

        _syncQueueRepositoryMock
            .Setup(x => x.GetPendingEntriesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(entries);

        await _service.ProcessPendingAsync();

        _syncQueueRepositoryMock.Verify(
            x => x.UpdateAsync(It.Is<SyncQueueEntry>(e => e.IsProcessed && e.PerformedAt != default), It.IsAny<CancellationToken>()),
            Times.Exactly(2));
    }

    [Fact]
    public async Task ProcessPendingAsync_NoPendingEntries_DoesNothing()
    {
        _syncQueueRepositoryMock
            .Setup(x => x.GetPendingEntriesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<SyncQueueEntry>());

        await _service.ProcessPendingAsync();

        _syncQueueRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<SyncQueueEntry>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ProcessPendingAsync_SetsErrorMessageOnFailure()
    {
        var entry = new SyncQueueEntry { Id = Guid.NewGuid(), DeviceId = "d1" };

        _syncQueueRepositoryMock
            .Setup(x => x.GetPendingEntriesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<SyncQueueEntry> { entry });

        _syncQueueRepositoryMock
            .SetupSequence(x => x.UpdateAsync(It.IsAny<SyncQueueEntry>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Test error"))
            .Returns(Task.CompletedTask);

        await _service.ProcessPendingAsync();

        entry.ErrorMessage.Should().Be("Test error");
    }

    [Fact]
    public async Task GetLastSyncTimestampAsync_ReturnsMaxPerformedAt()
    {
        var expected = new DateTime(2026, 5, 15, 12, 0, 0, DateTimeKind.Utc);

        _syncQueueRepositoryMock
            .Setup(x => x.GetLastSyncTimestampAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);

        var result = await _service.GetLastSyncTimestampAsync();

        result.Should().Be(expected);
    }

    [Fact]
    public async Task GetLastSyncTimestampAsync_NoProcessedEntries_ReturnsNull()
    {
        _syncQueueRepositoryMock
            .Setup(x => x.GetLastSyncTimestampAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync((DateTime?)null);

        var result = await _service.GetLastSyncTimestampAsync();

        result.Should().BeNull();
    }

    [Fact]
    public async Task CleanupOldEntriesAsync_DeletesOldEntries()
    {
        await _service.CleanupOldEntriesAsync();

        _syncQueueRepositoryMock.Verify(
            x => x.DeleteOldEntriesAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()), Times.Once);
    }
}
