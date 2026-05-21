using System;
using System.Collections.Generic;

namespace SmartInventory.Application.Mobile.Sync.DTOs;

public class SyncBatchDto
{
    public string DeviceId { get; set; } = string.Empty;
    public List<SyncOperationDto> Operations { get; set; } = new();
    public DateTime? LastSyncTimestamp { get; set; }
}
