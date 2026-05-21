using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SmartInventory.Application.Asset.DTOs.Reports;
using SmartInventory.Application.Asset.Filters;
using SmartInventory.Application.Asset.Interfaces;
using SmartInventory.Application.Caching;
using SmartInventory.Infrastructure.Data;
using AssetEntity = SmartInventory.Domain.Asset.Entities.Asset;
using CategoryGroupEntity = SmartInventory.Domain.Asset.Entities.CategoryGroup;

namespace SmartInventory.Infrastructure.Asset.Repositories;

public class AssetRepository : IAssetRepository
{
    private readonly ApplicationDbContext _context;
    private readonly ICacheService? _cacheService;

    public AssetRepository(ApplicationDbContext context, ICacheService? cacheService = null)
    {
        _context = context;
        _cacheService = cacheService;
    }

    public async Task<(List<AssetEntity> Items, int TotalCount)> GetAssetsAsync(AssetFilter filter, int page, int pageSize)
    {
        var query = _context.Assets.AsQueryable();

        if (!string.IsNullOrEmpty(filter.RoomCode))
            query = query.Where(a => a.CurrentRoomCode == filter.RoomCode);

        if (!string.IsNullOrEmpty(filter.Category))
            query = query.Where(a => a.Category == filter.Category);

        if (!string.IsNullOrEmpty(filter.Group))
        {
            query = query.Where(a => _context.CategoryGroups
                .Where(cg => cg.Group == filter.Group)
                .Select(cg => cg.Category)
                .Contains(a.Category));
        }

        if (filter.Status.HasValue)
            query = query.Where(a => a.Status == filter.Status.Value);

        if (!string.IsNullOrEmpty(filter.Search))
            query = query.Where(a => a.Name.Contains(filter.Search) || a.AssetTag.Contains(filter.Search));

        if (filter.ShowDiscrepantOnly)
            query = query.Where(a => a.DetectedRoomCode != null && a.CurrentRoomCode != a.DetectedRoomCode);

        if (filter.UpdatedAtFrom.HasValue)
        {
            query = query.IgnoreQueryFilters()
                .Where(a => (a.UpdatedAt != null && a.UpdatedAt >= filter.UpdatedAtFrom.Value)
                    || (a.DeletedAt != null && a.DeletedAt >= filter.UpdatedAtFrom.Value));
        }

        var totalCount = await query.CountAsync();
        
        var items = await query
            .AsNoTracking()
            .OrderBy(a => a.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, totalCount);
    }

    public async Task<AssetEntity?> GetByIdAsync(Guid id)
    {
        var cacheKey = $"asset:id:{id}";

        if (_cacheService != null)
        {
            var cached = await _cacheService.GetAsync<AssetEntity>(cacheKey);
            if (cached != null) return cached;
        }

        var asset = await _context.Assets.FirstOrDefaultAsync(a => a.Id == id);

        if (_cacheService != null && asset != null)
            await _cacheService.SetAsync(cacheKey, asset, TimeSpan.FromMinutes(5));

        return asset;
    }

    public async Task<AssetEntity?> GetByTagAsync(string assetTag)
    {
        var cacheKey = $"asset:tag:{assetTag.ToLowerInvariant()}";

        if (_cacheService != null)
        {
            var cached = await _cacheService.GetAsync<AssetEntity>(cacheKey);
            if (cached != null) return cached;
        }

        var asset = await _context.Assets.FirstOrDefaultAsync(a => a.AssetTag == assetTag);

        if (_cacheService != null && asset != null)
            await _cacheService.SetAsync(cacheKey, asset, TimeSpan.FromMinutes(5));

        return asset;
    }

    public async Task<AssetEntity?> GetByRfidAsync(string rfidTagId)
    {
        if (string.IsNullOrEmpty(rfidTagId))
            return null;
        return await _context.Assets.FirstOrDefaultAsync(a => a.RfidTagId == rfidTagId);
    }

    public async Task<AssetEntity> AddAsync(AssetEntity asset)
    {
        _context.Assets.Add(asset);
        await _context.SaveChangesAsync();
        return asset;
    }

    public async Task<AssetEntity> UpdateAsync(AssetEntity asset)
    {
        asset.UpdatedAt = DateTime.UtcNow;
        _context.Assets.Update(asset);
        await _context.SaveChangesAsync();
        return asset;
    }

    public async Task DeleteAsync(Guid id)
    {
        var asset = await _context.Assets.FirstOrDefaultAsync(a => a.Id == id);
        if (asset != null)
        {
            asset.DeletedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }
    }

    public async Task<bool> IsAssetTagUniqueAsync(string assetTag, Guid? excludeId = null)
    {
        var query = _context.Assets.Where(a => a.AssetTag == assetTag);
        if (excludeId.HasValue)
            query = query.Where(a => a.Id != excludeId.Value);
        return !await query.AnyAsync();
    }

    public async Task<bool> IsRfidUniqueAsync(string rfidTagId, Guid? excludeId = null)
    {
        if (string.IsNullOrEmpty(rfidTagId))
            return true;
        var query = _context.Assets.Where(a => a.RfidTagId == rfidTagId);
        if (excludeId.HasValue)
            query = query.Where(a => a.Id != excludeId.Value);
        return !await query.AnyAsync();
    }

    public async Task<bool> IsBleIdUniqueAsync(string bleId, Guid? excludeId = null)
    {
        if (string.IsNullOrEmpty(bleId))
            return true;
        var query = _context.Assets.Where(a => a.BleId == bleId);
        if (excludeId.HasValue)
            query = query.Where(a => a.Id != excludeId.Value);
        return !await query.AnyAsync();
    }

    public async Task<bool> IsRoomCodeValidAsync(string roomCode)
    {
        return await _context.Rooms.AnyAsync(r => r.Code.ToLower() == roomCode.ToLower());
    }

    public async Task<List<AssetEntity>> GetDiscrepantAssetsAsync()
    {
        return await _context.Assets
            .Where(a => a.DetectedRoomCode != null && a.CurrentRoomCode != a.DetectedRoomCode)
            .OrderBy(a => a.CurrentRoomCode)
            .ToListAsync();
    }

    // ── Aggregation methods ─────────────────────────────────────────────

    public async Task<List<StatusCountDto>> GetStatusCountsAsync()
    {
        return await _context.Assets
            .AsNoTracking()
            .GroupBy(a => a.Status)
            .Select(g => new StatusCountDto
            {
                Status = g.Key,
                Count = g.Count()
            })
            .ToListAsync();
    }

    public async Task<List<CategoryCountDto>> GetCategoryCountsAsync()
    {
        return await _context.Assets
            .AsNoTracking()
            .GroupBy(a => a.Category)
            .Select(g => new CategoryCountDto
            {
                Category = g.Key,
                Count = g.Count()
            })
            .ToListAsync();
    }

    public async Task<List<LocationCountDto>> GetLocationCountsAsync()
    {
        return await _context.Assets
            .AsNoTracking()
            .Where(a => a.CurrentRoomCode != null && a.CurrentRoomCode != "")
            .GroupBy(a => a.CurrentRoomCode)
            .Select(g => new LocationCountDto
            {
                RoomCode = g.Key,
                Count = g.Count()
            })
            .ToListAsync();
    }

    public async Task<List<RoomAssetCountDto>> GetRoomAssetCountsAsync()
    {
        return await _context.Assets
            .AsNoTracking()
            .Where(a => a.CurrentRoomCode != null && a.CurrentRoomCode != "")
            .GroupBy(a => new { a.CurrentRoomCode, a.Status })
            .Select(g => new RoomAssetCountDto
            {
                RoomCode = g.Key.CurrentRoomCode,
                Status = g.Key.Status,
                Count = g.Count()
            })
            .ToListAsync();
    }

    public async Task<List<RoomCategoryDto>> GetRoomCategoriesAsync()
    {
        return await _context.Assets
            .AsNoTracking()
            .Where(a => a.CurrentRoomCode != null && a.CurrentRoomCode != "")
            .Select(a => new { a.CurrentRoomCode, a.Category })
            .Distinct()
            .Select(x => new RoomCategoryDto
            {
                RoomCode = x.CurrentRoomCode,
                Category = x.Category
            })
            .ToListAsync();
    }

    public async Task<List<AssetEntity>> GetFilteredListAsync(AssetFilter filter)
    {
        var cacheKey = BuildFilterCacheKey(filter);

        if (_cacheService != null)
        {
            var cached = await _cacheService.GetAsync<List<AssetEntity>>(cacheKey);
            if (cached != null) return cached;
        }

        var query = _context.Assets.AsNoTracking().AsQueryable();

        if (!string.IsNullOrEmpty(filter.RoomCode))
            query = query.Where(a => a.CurrentRoomCode == filter.RoomCode);

        if (!string.IsNullOrEmpty(filter.Category))
            query = query.Where(a => a.Category == filter.Category);

        if (!string.IsNullOrEmpty(filter.Group))
        {
            query = query.Where(a => _context.CategoryGroups
                .Where(cg => cg.Group == filter.Group)
                .Select(cg => cg.Category)
                .Contains(a.Category));
        }

        if (filter.Status.HasValue)
            query = query.Where(a => a.Status == filter.Status.Value);

        if (!string.IsNullOrEmpty(filter.Search))
            query = query.Where(a => a.Name.Contains(filter.Search) || a.AssetTag.Contains(filter.Search));

        if (filter.ShowDiscrepantOnly)
            query = query.Where(a => a.DetectedRoomCode != null && a.CurrentRoomCode != a.DetectedRoomCode);

        if (filter.UpdatedAtFrom.HasValue)
        {
            query = query.IgnoreQueryFilters()
                .Where(a => (a.UpdatedAt != null && a.UpdatedAt >= filter.UpdatedAtFrom.Value)
                    || (a.DeletedAt != null && a.DeletedAt >= filter.UpdatedAtFrom.Value));
        }

        var result = await query.OrderBy(a => a.Name).ToListAsync();

        if (_cacheService != null)
            await _cacheService.SetAsync(cacheKey, result, TimeSpan.FromMinutes(5));

        return result;
    }

    private static string BuildFilterCacheKey(AssetFilter filter)
    {
        var parts = new[]
        {
            filter.RoomCode ?? "",
            filter.Category ?? "",
            filter.Group ?? "",
            filter.Status?.ToString() ?? "",
            filter.Search ?? "",
            filter.ShowDiscrepantOnly ? "1" : "0",
            filter.UpdatedAtFrom?.Ticks.ToString() ?? ""
        };
        return "asset:filtered:" + string.Join("|", parts);
    }

    public async Task<List<AssetEntity>> GetAssetsWithMaintenanceAsync(DateTime from, DateTime to)
    {
        return await _context.Assets
            .AsNoTracking()
            .Where(a => a.MaintenanceDueDate.HasValue
                && a.MaintenanceDueDate >= from
                && a.MaintenanceDueDate <= to
                && a.Status != Domain.Asset.Enums.AssetStatus.Retired
                && a.Status != Domain.Asset.Enums.AssetStatus.Lost)
            .OrderBy(a => a.MaintenanceDueDate)
            .ToListAsync();
    }

    public async Task<List<AssetEntity>> GetAssetsByStatusAsync(ICollection<Domain.Asset.Enums.AssetStatus> statuses)
    {
        return await _context.Assets
            .AsNoTracking()
            .Where(a => statuses.Contains(a.Status))
            .OrderBy(a => a.LastSeen)
            .ToListAsync();
    }

    public async Task<List<CategoryStatusBreakdownDto>> GetCategoryStatusBreakdownAsync()
    {
        return await _context.Assets
            .AsNoTracking()
            .GroupBy(a => new { a.Category, a.Status })
            .Select(g => new CategoryStatusBreakdownDto
            {
                Category = g.Key.Category,
                Status = g.Key.Status,
                Count = g.Count()
            })
            .ToListAsync();
    }
}