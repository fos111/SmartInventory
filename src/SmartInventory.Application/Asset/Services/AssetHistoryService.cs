using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SmartInventory.Application.Asset.Interfaces;
using AssetHistoryEntity = SmartInventory.Domain.Asset.Entities.AssetHistory;

namespace SmartInventory.Application.Asset.Services;

public class AssetHistoryService : IAssetHistoryService
{
    private readonly IAssetHistoryRepository _repository;

    public AssetHistoryService(IAssetHistoryRepository repository)
    {
        _repository = repository;
    }

    public async Task TrackChangeAsync(Guid assetId, string property, string? oldValue, string? newValue, Guid userId)
    {
        var history = new AssetHistoryEntity
        {
            AssetId = assetId,
            PropertyChanged = property,
            OldValue = oldValue,
            NewValue = newValue,
            ChangedBy = userId,
            ChangedAt = DateTime.UtcNow
        };

        await _repository.AddAsync(history);
    }

    public async Task<IEnumerable<AssetHistoryEntity>> GetAssetHistoryAsync(Guid assetId)
    {
        return await _repository.GetByAssetIdAsync(assetId);
    }

    public async Task<IEnumerable<AssetHistoryEntity>> GetAllHistoryAsync(DateTime? from = null, DateTime? to = null)
    {
        return await _repository.GetAllAsync(from, to);
    }
}
