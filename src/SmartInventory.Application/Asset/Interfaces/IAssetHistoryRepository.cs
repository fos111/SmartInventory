using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AssetHistoryEntity = SmartInventory.Domain.Asset.Entities.AssetHistory;

namespace SmartInventory.Application.Asset.Interfaces;

public interface IAssetHistoryRepository
{
    Task<AssetHistoryEntity> AddAsync(AssetHistoryEntity history);
    Task<IEnumerable<AssetHistoryEntity>> GetByAssetIdAsync(Guid assetId);
    Task<IEnumerable<AssetHistoryEntity>> GetAllAsync(DateTime? from = null, DateTime? to = null);
}
