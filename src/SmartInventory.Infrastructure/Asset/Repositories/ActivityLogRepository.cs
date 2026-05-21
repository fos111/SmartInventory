using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SmartInventory.Application.Asset.Interfaces;
using SmartInventory.Infrastructure.Data;
using ActivityLogEntity = SmartInventory.Domain.Asset.Entities.ActivityLog;

namespace SmartInventory.Infrastructure.Asset.Repositories;

public class ActivityLogRepository : IActivityLogRepository
{
    private readonly ApplicationDbContext _context;

    public ActivityLogRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ActivityLogEntity> AddAsync(ActivityLogEntity log)
    {
        _context.ActivityLogs.Add(log);
        await _context.SaveChangesAsync();
        return log;
    }

    public async Task<IEnumerable<ActivityLogEntity>> GetAllAsync(DateTime? from = null, DateTime? to = null)
    {
        var query = _context.ActivityLogs.AsQueryable();

        // Ensure UTC Kind — query-string DateTimes come as Unspecified,
        // which Npgsql rejects for 'timestamp with time zone' columns.
        if (from.HasValue)
            query = query.Where(l => l.ChangedAt >= DateTime.SpecifyKind(from.Value, DateTimeKind.Utc));
        if (to.HasValue)
            query = query.Where(l => l.ChangedAt <= DateTime.SpecifyKind(to.Value, DateTimeKind.Utc));

        return await query
            .OrderByDescending(l => l.ChangedAt)
            .ToListAsync();
    }
}
