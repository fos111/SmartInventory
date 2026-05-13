namespace SmartInventory.Application.Asset.DTOs.Reports;

public class RoomAuditDto
{
    public string RoomCode { get; set; } = string.Empty;
    public string? ZoneName { get; set; }
    public string? BuildingName { get; set; }
    public List<RoomAssetItem> Assets { get; set; } = new();
}

public class RoomAssetItem
{
    public string AssetTag { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string? RfidTagId { get; set; }
    public DateTime? LastSeen { get; set; }
}
