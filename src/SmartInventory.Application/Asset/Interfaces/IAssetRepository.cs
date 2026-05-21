using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SmartInventory.Application.Asset.DTOs.Reports;
using SmartInventory.Application.Asset.Filters;
using AssetEntity = SmartInventory.Domain.Asset.Entities.Asset;

namespace SmartInventory.Application.Asset.Interfaces;

public interface IAssetRepository
{
    Task<(List<AssetEntity> Items, int TotalCount)> GetAssetsAsync(AssetFilter filter, int page, int pageSize);
    Task<AssetEntity?> GetByIdAsync(Guid id);
    Task<AssetEntity?> GetByTagAsync(string assetTag);
    Task<AssetEntity?> GetByRfidAsync(string rfidTagId);
    Task<AssetEntity> AddAsync(AssetEntity asset);
    Task<AssetEntity> UpdateAsync(AssetEntity asset);
    Task DeleteAsync(Guid id);
    Task<bool> IsAssetTagUniqueAsync(string assetTag, Guid? excludeId = null);
    Task<bool> IsRfidUniqueAsync(string rfidTagId, Guid? excludeId = null);
    Task<bool> IsBleIdUniqueAsync(string bleId, Guid? excludeId = null);
    Task<bool> IsRoomCodeValidAsync(string roomCode);
    Task<List<AssetEntity>> GetDiscrepantAssetsAsync();

    // ── Aggregation methods (replace int.MaxValue pattern) ──────────────

    /// Returns asset count grouped by AssetStatus using SQL GROUP BY.
    Task<List<StatusCountDto>> GetStatusCountsAsync();

    /// Returns asset count grouped by Category using SQL GROUP BY.
    Task<List<CategoryCountDto>> GetCategoryCountsAsync();

    /// Returns asset count grouped by CurrentRoomCode using SQL GROUP BY.
    Task<List<LocationCountDto>> GetLocationCountsAsync();

    /// Returns per-room asset counts broken down by status using SQL GROUP BY.
    Task<List<RoomAssetCountDto>> GetRoomAssetCountsAsync();

    /// Returns distinct (RoomCode, Category) pairs for room-level category data.
    Task<List<RoomCategoryDto>> GetRoomCategoriesAsync();

    /// Returns assets matching filter without pagination (for filtered lists that need full entity data).
    Task<List<AssetEntity>> GetFilteredListAsync(AssetFilter filter);

    /// Returns assets with non-null MaintenanceDueDate within the given date range.
    Task<List<AssetEntity>> GetAssetsWithMaintenanceAsync(DateTime from, DateTime to);

    /// Returns assets matching any of the given statuses (e.g. CriticalIssue, Lost).
    Task<List<AssetEntity>> GetAssetsByStatusAsync(ICollection<Domain.Asset.Enums.AssetStatus> statuses);

    /// Returns per-category per-status counts using SQL GROUP BY.
    Task<List<CategoryStatusBreakdownDto>> GetCategoryStatusBreakdownAsync();
}