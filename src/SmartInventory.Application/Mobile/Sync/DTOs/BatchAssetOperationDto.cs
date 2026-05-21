using System;

namespace SmartInventory.Application.Mobile.Sync.DTOs;

public class BatchAssetOperationDto
{
    public string AssetTag { get; set; } = string.Empty;
    public string OperationType { get; set; } = string.Empty;
    public string? TargetRoomCode { get; set; }
    public string? NewStatus { get; set; }
    public string? Note { get; set; }
    public DateTime PerformedAt { get; set; }
}
