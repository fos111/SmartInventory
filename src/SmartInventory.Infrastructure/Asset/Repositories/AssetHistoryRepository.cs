using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SmartInventory.Application.Asset.Interfaces;
using SmartInventory.Infrastructure.Data;
using AssetHistoryEntity = SmartInventory.Domain.Asset.Entities.AssetHistory;

namespace SmartInventory.Infrastructure.Asset.Repositories;

public class AssetHistoryRepository : IAssetHistoryRepository
{
    private readonly ApplicationDbContext _context;

    public AssetHistoryRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<AssetHistoryEntity> AddAsync(AssetHistoryEntity history)
    {
        _context.AssetHistories.Add(history);
        await _context.SaveChangesAsync();
        return history;
    }

    public async Task<IEnumerable<AssetHistoryEntity>> GetByAssetIdAsync(Guid assetId)
    {
        return await _context.AssetHistories
            .Where(h => h.AssetId == assetId)
            .OrderByDescending(h => h.ChangedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<AssetHistoryEntity>> GetAllAsync(DateTime? from = null, DateTime? to = null)
    {
        var query = _context.AssetHistories.AsQueryable();

        // Ensure UTC Kind — query-string DateTimes come as Unspecified,
        // which Npgsql rejects for 'timestamp with time zone' columns.
        if (from.HasValue)
            query = query.Where(h => h.ChangedAt >= DateTime.SpecifyKind(from.Value, DateTimeKind.Utc));
        if (to.HasValue)
            query = query.Where(h => h.ChangedAt <= DateTime.SpecifyKind(to.Value, DateTimeKind.Utc));

        return await query
            .OrderByDescending(h => h.ChangedAt)
            .ToListAsync();
    }
}
