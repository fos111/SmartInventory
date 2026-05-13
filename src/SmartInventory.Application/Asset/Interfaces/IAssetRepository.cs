using System;
using System.Collections.Generic;
using System.Threading.Tasks;
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
    Task<bool> IsRoomCodeValidAsync(string roomCode);
    Task<List<AssetEntity>> GetDiscrepantAssetsAsync();
}