using SmartInventory.Domain.Mobile.Enums;

namespace SmartInventory.Domain.Mobile.Entities;

public class SyncQueueEntry
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string DeviceId { get; set; } = string.Empty;
    public Guid? AssetId { get; set; }
    public string OperationType { get; set; } = string.Empty;
    public string Payload { get; set; } = string.Empty;
    public string? TargetRoomCode { get; set; }
    public string? NewStatus { get; set; }
    public string ClientOperationId { get; set; } = string.Empty;
    public DateTime ReceivedAt { get; set; } = DateTime.UtcNow;
    public DateTime PerformedAt { get; set; }
    public bool IsProcessed { get; set; }
    public string? ErrorMessage { get; set; }
}
