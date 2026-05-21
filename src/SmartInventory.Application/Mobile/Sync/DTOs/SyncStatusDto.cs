using System;

namespace SmartInventory.Application.Mobile.Sync.DTOs;

public class SyncStatusDto
{
    public int PendingOperations { get; set; }
    public DateTime? LastSyncTimestamp { get; set; }
}
