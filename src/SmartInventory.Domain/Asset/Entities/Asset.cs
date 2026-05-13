using SmartInventory.Domain.Asset.Enums;

namespace SmartInventory.Domain.Asset.Entities;

public class Asset
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string AssetTag { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Category { get; set; } = string.Empty;
    public AssetStatus Status { get; set; } = AssetStatus.InStock;
    public string? Manufacturer { get; set; }
    public string? Model { get; set; }
    public string? SerialNumber { get; set; }
    public string CurrentRoomCode { get; set; } = string.Empty;
    public string? DetectedRoomCode { get; set; }
    public DateTime? LastSeen { get; set; }
    public DateTime? LastDetectedUpdate { get; set; }
    public string? RfidTagId { get; set; }
    public DateTime? MaintenanceDueDate { get; set; }
    public DateTime? LastMaintenanceDate { get; set; }
    public DateTime? InstallDate { get; set; }
    public DateTime? LastServiceDate { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }
}