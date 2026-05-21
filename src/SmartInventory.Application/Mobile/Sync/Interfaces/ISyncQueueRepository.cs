using System.Threading;
using SmartInventory.Domain.Mobile.Entities;

namespace SmartInventory.Application.Mobile.Sync.Interfaces;

public interface ISyncQueueRepository
{
    Task AddAsync(SyncQueueEntry entry, CancellationToken ct = default);
    Task UpdateAsync(SyncQueueEntry entry, CancellationToken ct = default);
    Task<List<SyncQueueEntry>> GetPendingEntriesAsync(CancellationToken ct = default);
    Task<int> GetPendingCountAsync(CancellationToken ct = default);
    Task<SyncQueueEntry?> GetByClientOperationIdAsync(string clientOperationId, CancellationToken ct = default);
    Task<DateTime?> GetLastSyncTimestampAsync(CancellationToken ct = default);
    Task DeleteOldEntriesAsync(DateTime processedCutoff, DateTime unprocessedCutoff, CancellationToken ct = default);
}
