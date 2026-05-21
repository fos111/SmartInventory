using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SmartInventory.Application.Asset.Common;
using SmartInventory.Application.Asset.DTOs;
using SmartInventory.Application.Asset.Filters;
using SmartInventory.Application.Asset.Interfaces;
using SmartInventory.Application.Mobile.Products.DTOs;
using SmartInventory.Application.Mobile.Products.Interfaces;
using SmartInventory.Domain.Asset.Enums;

namespace SmartInventory.Application.Mobile.Products.Services;

public class MobileProductService : IMobileProductService
{
    private readonly IAssetService _assetService;
    private readonly IActivityLogService _activityLogService;

    public MobileProductService(
        IAssetService assetService,
        IActivityLogService activityLogService)
    {
        _assetService = assetService;
        _activityLogService = activityLogService;
    }

    public async Task<MobilePagedResultDto<AssetListItemDto>> GetProductsAsync(
        MobileProductFilterDto filter, CancellationToken ct = default)
    {
        var assetFilter = MapToAssetFilter(filter);
        var paged = await _assetService.GetAssetsAsync(assetFilter);

        return new MobilePagedResultDto<AssetListItemDto>
        {
            Items = paged.Items.Select(MapToListItem).ToList(),
            TotalCount = paged.TotalCount,
            Page = paged.Page,
            PageSize = paged.PageSize,
            HasNextPage = paged.HasNextPage
        };
    }

    public async Task<AssetScanDto?> GetProductByIdAsync(
        Guid id, CancellationToken ct = default)
    {
        var asset = await _assetService.GetAssetByIdAsync(id);
        return asset == null ? null : MapToScanDto(asset);
    }

    public async Task<AssetScanDto?> ScanByTagAsync(
        string assetTag, Guid userId, CancellationToken ct = default)
    {
        var asset = await _assetService.GetAssetByTagAsync(assetTag);
        if (asset == null) return null;

        await _activityLogService.TrackFacilityChangeAsync(
            "Scanned", "Asset", asset.AssetTag, asset.Name,
            $"Room: {asset.CurrentRoomCode}", userId);

        return MapToScanDto(asset);
    }

    public async Task<IEnumerable<ScanHistoryEntryDto>> GetScanHistoryAsync(
        DateTime? from, DateTime? to, Guid userId, CancellationToken ct = default)
    {
        var allActivity = await _activityLogService.GetAllActivityLogsAsync(from, to);

        return allActivity
            .Where(a => a.Action == "Scanned")
            .Select(a => new ScanHistoryEntryDto
            {
                Id = a.Id,
                AssetTag = a.AssetTag,
                AssetName = a.AssetName,
                Location = a.Details,
                ScannedAt = a.ChangedAt,
                ScannedByName = a.ChangedByName
            })
            .OrderByDescending(s => s.ScannedAt);
    }

    private static AssetFilter MapToAssetFilter(MobileProductFilterDto filter)
    {
        var status = Enum.TryParse<AssetStatus>(filter.Status, ignoreCase: true, out var parsed)
            ? parsed
            : (AssetStatus?)null;

        return new AssetFilter
        {
            Search = filter.Search,
            Status = status,
            RoomCode = filter.RoomCode,
            Group = filter.Department,
            Category = filter.Type,
            UpdatedAtFrom = filter.Since,
            Page = filter.Page,
            PageSize = filter.Limit
        };
    }

    private static AssetListItemDto MapToListItem(AssetDto asset)
    {
        return new AssetListItemDto
        {
            Id = asset.Id,
            AssetTag = asset.AssetTag,
            Name = asset.Name,
            Status = asset.Status.ToString(),
            CurrentRoomCode = asset.CurrentRoomCode,
            Category = asset.Category,
            LastSeen = asset.LastSeen,
            HasDiscrepancy = asset.HasDiscrepancy,
            IsDeleted = asset.IsDeleted,
            StatusEntryNote = asset.StatusEntryNote,
            StatusExitNote = asset.StatusExitNote
        };
    }

    private static AssetScanDto MapToScanDto(AssetDto asset)
    {
        return new AssetScanDto
        {
            Id = asset.Id,
            AssetTag = asset.AssetTag,
            Name = asset.Name,
            Status = asset.Status.ToString(),
            CurrentRoomCode = asset.CurrentRoomCode
        };
    }
}
