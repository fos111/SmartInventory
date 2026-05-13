using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SmartInventory.Application.Asset.Filters;
using SmartInventory.Application.Asset.Interfaces;
using SmartInventory.Infrastructure.Data;
using AssetEntity = SmartInventory.Domain.Asset.Entities.Asset;
using CategoryGroupEntity = SmartInventory.Domain.Asset.Entities.CategoryGroup;

namespace SmartInventory.Infrastructure.Asset.Repositories;

public class AssetRepository : IAssetRepository
{
    private readonly ApplicationDbContext _context;

    public AssetRepository(ApplicationDbContext context)
    {
        _context = context;
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
        return await _context.Assets.FirstOrDefaultAsync(a => a.Id == id);
    }

    public async Task<AssetEntity?> GetByTagAsync(string assetTag)
    {
        return await _context.Assets.FirstOrDefaultAsync(a => a.AssetTag == assetTag);
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
}