using System;

namespace SmartInventory.Application.Mobile.Sync.DTOs;

public class SyncOperationDto
{
    public string OperationType { get; set; } = string.Empty;
    public string AssetTag { get; set; } = string.Empty;
    public string? TargetRoomCode { get; set; }
    public string? NewStatus { get; set; }
    public DateTime PerformedAt { get; set; }
    public string ClientOperationId { get; set; } = string.Empty;
}
