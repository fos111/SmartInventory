using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AssetHistoryEntity = SmartInventory.Domain.Asset.Entities.AssetHistory;

namespace SmartInventory.Application.Asset.Interfaces;

public interface IAssetHistoryService
{
    Task TrackChangeAsync(Guid assetId, string property, string? oldValue, string? newValue, Guid userId);
    Task<IEnumerable<AssetHistoryEntity>> GetAssetHistoryAsync(Guid assetId);
    Task<IEnumerable<AssetHistoryEntity>> GetAllHistoryAsync(DateTime? from = null, DateTime? to = null);
}
