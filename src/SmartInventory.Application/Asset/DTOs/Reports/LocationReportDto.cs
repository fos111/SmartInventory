using SmartInventory.Domain.Asset.Enums;

namespace SmartInventory.Application.Asset.DTOs.Reports;

public class LocationComprehensiveReportDto
{
    public string Scope { get; set; } = string.Empty;
    public string ScopeId { get; set; } = string.Empty;
    public string ScopeName { get; set; } = string.Empty;
    public LocationHierarchyInfo Hierarchy { get; set; } = new();
    public int TotalAssets { get; set; }
    public List<LocationRoomAssetItem> CurrentAssets { get; set; } = new();
    public List<LocationHistoryEntry> History { get; set; } = new();
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
}

public class LocationHierarchyInfo
{
    public string? SiteName { get; set; }
    public string? ZoneName { get; set; }
    public string? BuildingName { get; set; }
    public int? FloorLevel { get; set; }
    public string? RoomCode { get; set; }
    public string? RoomName { get; set; }
    public string? RoomDescription { get; set; }
}

public class LocationRoomAssetItem
{
    public string AssetTag { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public AssetStatus Status { get; set; }
    public string? SerialNumber { get; set; }
    public string? RfidTagId { get; set; }
    public string? BleId { get; set; }
    public string? Manufacturer { get; set; }
    public string? Model { get; set; }
    public DateTime? LastSeen { get; set; }
    public DateTime? InstallDate { get; set; }
    public DateTime? MaintenanceDueDate { get; set; }
}

public class LocationHistoryEntry
{
    public DateTime Timestamp { get; set; }
    public string EventType { get; set; } = string.Empty;
    public string AssetTag { get; set; } = string.Empty;
    public string AssetName { get; set; } = string.Empty;
    public string? OldValue { get; set; }
    public string? NewValue { get; set; }
    public string ChangedBy { get; set; } = string.Empty;
    public string Source { get; set; } = string.Empty;
}
