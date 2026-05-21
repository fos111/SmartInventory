using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using SmartInventory.Application.Asset.Interfaces;
using SmartInventory.Application.Mobile.Sync.DTOs;
using SmartInventory.Application.Mobile.Sync.Interfaces;
using SmartInventory.Domain.Asset.Enums;
using SmartInventory.Domain.Auth.Enums;
using SmartInventory.Domain.Mobile.Enums;

namespace SmartInventory.Application.Mobile.Sync.Services;

public class MobileSyncService : IMobileSyncService
{
    private readonly IAssetService _assetService;
    private readonly ISyncQueueService _syncQueueService;
    private readonly IActivityLogService _activityLogService;

    public MobileSyncService(
        IAssetService assetService,
        ISyncQueueService syncQueueService,
        IActivityLogService activityLogService)
    {
        _assetService = assetService;
        _syncQueueService = syncQueueService;
        _activityLogService = activityLogService;
    }

    public async Task<List<BatchOperationResult>> ProcessBatchAsync(BatchOperationRequest request, Guid userId, CancellationToken ct = default)
    {
        var results = new List<BatchOperationResult>();
        var index = 0;

        foreach (var operation in request.Operations)
        {
            if (index >= 100) break;

            var asset = await _assetService.GetAssetByTagAsync(operation.AssetTag);

            if (asset == null)
            {
                results.Add(new BatchOperationResult
                {
                    Index = index,
                    Success = false,
                    AssetTag = operation.AssetTag,
                    Error = "Asset not found"
                });
                index++;
                continue;
            }

            try
            {
                if (!Enum.TryParse<SyncOperationType>(operation.OperationType, ignoreCase: true, out var opType))
                {
                    results.Add(new BatchOperationResult
                    {
                        Index = index,
                        Success = false,
                        AssetTag = operation.AssetTag,
                        Error = "Unsupported operation type"
                    });
                    index++;
                    continue;
                }

                if (operation.PerformedAt < asset.UpdatedAt)
                {
                    results.Add(new BatchOperationResult
                    {
                        Index = index,
                        Success = false,
                        AssetTag = operation.AssetTag,
                        Error = "Conflict: asset was modified after this operation"
                    });
                    index++;
                    continue;
                }

                switch (opType)
                {
                    case SyncOperationType.Move:
                        await _assetService.MoveAssetAsync(asset.Id, operation.TargetRoomCode, userId);

                        await _activityLogService.TrackFacilityChangeAsync(
                            "Moved", "Asset", asset.AssetTag, asset.Name,
                            $"To room: {operation.TargetRoomCode}", userId);

                        results.Add(new BatchOperationResult
                        {
                            Index = index,
                            Success = true,
                            AssetTag = operation.AssetTag
                        });
                        break;

                    case SyncOperationType.StatusChange:
                        var status = Enum.Parse<AssetStatus>(operation.NewStatus, ignoreCase: true);
                        await _assetService.UpdateStatusAsync(asset.Id, status, userId, UserRole.Technicien, operation.Note);

                        var noteSuffix = !string.IsNullOrWhiteSpace(operation.Note)
                            ? $" — {operation.Note}"
                            : string.Empty;
                        await _activityLogService.TrackFacilityChangeAsync(
                            "StatusChanged", "Asset", asset.AssetTag, asset.Name,
                            $"New status: {operation.NewStatus}{noteSuffix}", userId);

                        results.Add(new BatchOperationResult
                        {
                            Index = index,
                            Success = true,
                            AssetTag = operation.AssetTag
                        });
                        break;

                    default:
                        results.Add(new BatchOperationResult
                        {
                            Index = index,
                            Success = false,
                            AssetTag = operation.AssetTag,
                            Error = "Unsupported operation type"
                        });
                        break;
                }
            }
            catch
            {
                results.Add(new BatchOperationResult
                {
                    Index = index,
                    Success = false,
                    AssetTag = operation.AssetTag,
                    Error = "Operation failed"
                });
            }

            index++;
        }

        return results;
    }

    public async Task<SyncStatusDto> ProcessQueueAsync(SyncBatchDto batch, Guid userId, CancellationToken ct = default)
    {
        foreach (var operation in batch.Operations)
        {
            await _syncQueueService.EnqueueAsync(
                batch.DeviceId,
                operation.OperationType,
                JsonSerializer.Serialize(operation),
                operation.ClientOperationId,
                targetRoomCode: operation.TargetRoomCode,
                newStatus: operation.NewStatus,
                ct: ct);
        }

        await _syncQueueService.ProcessPendingAsync(ct);

        return await GetStatusAsync(userId, ct);
    }

    public async Task<SyncStatusDto> GetStatusAsync(Guid userId, CancellationToken ct = default)
    {
        var pendingCount = await _syncQueueService.GetPendingCountAsync(ct);
        var lastSync = await _syncQueueService.GetLastSyncTimestampAsync(ct);

        return new SyncStatusDto
        {
            PendingOperations = pendingCount,
            LastSyncTimestamp = lastSync
        };
    }
}
