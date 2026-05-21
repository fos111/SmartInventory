using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using SmartInventory.Application.Mobile.Sync.DTOs;

namespace SmartInventory.Application.Mobile.Sync.Interfaces;

public interface IMobileSyncService
{
    Task<List<BatchOperationResult>> ProcessBatchAsync(BatchOperationRequest request, Guid userId, CancellationToken ct = default);
    Task<SyncStatusDto> ProcessQueueAsync(SyncBatchDto batch, Guid userId, CancellationToken ct = default);
    Task<SyncStatusDto> GetStatusAsync(Guid userId, CancellationToken ct = default);
}
