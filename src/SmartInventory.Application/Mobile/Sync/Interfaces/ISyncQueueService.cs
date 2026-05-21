using System.Threading;
using SmartInventory.Domain.Mobile.Entities;

namespace SmartInventory.Application.Mobile.Sync.Interfaces;

public interface ISyncQueueService
{
    Task<SyncQueueEntry> EnqueueAsync(string deviceId, string operationType, string payload,
        string clientOperationId, Guid? assetId = null, string? targetRoomCode = null, string? newStatus = null,
        CancellationToken ct = default);

    Task ProcessPendingAsync(CancellationToken ct = default);

    Task<int> GetPendingCountAsync(CancellationToken ct = default);

    Task<DateTime?> GetLastSyncTimestampAsync(CancellationToken ct = default);

    Task CleanupOldEntriesAsync(CancellationToken ct = default);
}
