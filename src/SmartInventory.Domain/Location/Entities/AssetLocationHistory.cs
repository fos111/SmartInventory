using System;

namespace SmartInventory.Domain.Location.Entities;

public class AssetLocationHistory
{
    public Guid Id { get; set; }
    public Guid AssetId { get; set; }
    public string? PreviousRoomCode { get; set; }
    public string NewRoomCode { get; set; } = string.Empty;
    public string Source { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
