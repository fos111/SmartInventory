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
    public string? PhotoPath { get; set; }
    public string? Price { get; set; }
    public string? BleId { get; set; }

    /// <summary>
    /// Note provided when asset entered the current Maintenance or CriticalIssue status.
    /// Describes what work is needed or what is wrong.
    /// Cleared when asset leaves Maintenance/CriticalIssue.
    /// </summary>
    public string? StatusEntryNote { get; set; }

    /// <summary>
    /// Note provided when asset exits Maintenance or CriticalIssue back to Active/InStock.
    /// Describes what maintenance was performed or how the issue was resolved.
    /// Persists across future status changes for audit reference.
    /// </summary>
    public string? StatusExitNote { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }
}