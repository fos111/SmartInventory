using System.Threading;
using Microsoft.EntityFrameworkCore;
using SmartInventory.Application.Mobile.Sync.Interfaces;
using SmartInventory.Domain.Mobile.Entities;
using SmartInventory.Infrastructure.Data;

namespace SmartInventory.Infrastructure.Mobile.Repositories;

public class SyncQueueRepository : ISyncQueueRepository
{
    private readonly ApplicationDbContext _context;

    public SyncQueueRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(SyncQueueEntry entry, CancellationToken ct = default)
    {
        await _context.SyncQueueEntries.AddAsync(entry, ct);
        await _context.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(SyncQueueEntry entry, CancellationToken ct = default)
    {
        _context.SyncQueueEntries.Update(entry);
        await _context.SaveChangesAsync(ct);
    }

    public async Task<List<SyncQueueEntry>> GetPendingEntriesAsync(CancellationToken ct = default)
    {
        return await _context.SyncQueueEntries
            .Where(e => !e.IsProcessed)
            .OrderBy(e => e.ReceivedAt)
            .ToListAsync(ct);
    }

    public async Task<int> GetPendingCountAsync(CancellationToken ct = default)
    {
        return await _context.SyncQueueEntries
            .CountAsync(e => !e.IsProcessed, ct);
    }

    public async Task<SyncQueueEntry?> GetByClientOperationIdAsync(string clientOperationId, CancellationToken ct = default)
    {
        return await _context.SyncQueueEntries
            .FirstOrDefaultAsync(e => e.ClientOperationId == clientOperationId, ct);
    }

    public async Task<DateTime?> GetLastSyncTimestampAsync(CancellationToken ct = default)
    {
        return await _context.SyncQueueEntries
            .Where(e => e.IsProcessed)
            .MaxAsync(e => (DateTime?)e.PerformedAt, ct);
    }

    public async Task DeleteOldEntriesAsync(DateTime processedCutoff, DateTime unprocessedCutoff, CancellationToken ct = default)
    {
        var oldProcessed = await _context.SyncQueueEntries
            .Where(e => e.IsProcessed && e.PerformedAt < processedCutoff)
            .ToListAsync(ct);

        var oldUnprocessed = await _context.SyncQueueEntries
            .Where(e => !e.IsProcessed && e.ReceivedAt < unprocessedCutoff)
            .ToListAsync(ct);

        _context.SyncQueueEntries.RemoveRange(oldProcessed);
        _context.SyncQueueEntries.RemoveRange(oldUnprocessed);
        await _context.SaveChangesAsync(ct);
    }
}
