using System;
using System.Threading;
using System.Threading.Tasks;
using SmartInventory.Application.Mobile.Sync.Interfaces;
using SmartInventory.Domain.Mobile.Entities;

namespace SmartInventory.Application.Mobile.Sync.Services;

public class SyncQueueService : ISyncQueueService
{
    private readonly ISyncQueueRepository _syncQueueRepository;

    public SyncQueueService(ISyncQueueRepository syncQueueRepository)
    {
        _syncQueueRepository = syncQueueRepository;
    }

    public async Task<SyncQueueEntry> EnqueueAsync(string deviceId, string operationType, string payload,
        string clientOperationId, Guid? assetId = null, string? targetRoomCode = null, string? newStatus = null,
        CancellationToken ct = default)
    {
        var entry = new SyncQueueEntry
        {
            DeviceId = deviceId,
            AssetId = assetId,
            OperationType = operationType,
            Payload = payload,
            TargetRoomCode = targetRoomCode,
            NewStatus = newStatus,
            ClientOperationId = clientOperationId,
            ReceivedAt = DateTime.UtcNow
        };

        await _syncQueueRepository.AddAsync(entry, ct);
        return entry;
    }

    public async Task ProcessPendingAsync(CancellationToken ct = default)
    {
        var pending = await _syncQueueRepository.GetPendingEntriesAsync(ct);
        if (pending.Count == 0) return;

        foreach (var entry in pending)
        {
            try
            {
                entry.IsProcessed = true;
                entry.PerformedAt = DateTime.UtcNow;
                await _syncQueueRepository.UpdateAsync(entry, ct);
            }
            catch (Exception ex)
            {
                entry.ErrorMessage = ex.Message;
                entry.PerformedAt = DateTime.UtcNow;
                await _syncQueueRepository.UpdateAsync(entry, ct);
            }
        }
    }

    public async Task<int> GetPendingCountAsync(CancellationToken ct = default)
    {
        return await _syncQueueRepository.GetPendingCountAsync(ct);
    }

    public async Task<DateTime?> GetLastSyncTimestampAsync(CancellationToken ct = default)
    {
        return await _syncQueueRepository.GetLastSyncTimestampAsync(ct);
    }

    public async Task CleanupOldEntriesAsync(CancellationToken ct = default)
    {
        var processedCutoff = DateTime.UtcNow.AddDays(-7);
        var unprocessedCutoff = DateTime.UtcNow.AddDays(-30);
        await _syncQueueRepository.DeleteOldEntriesAsync(processedCutoff, unprocessedCutoff, ct);
    }
}
