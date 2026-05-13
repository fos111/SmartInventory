namespace SmartInventory.Application.Asset.DTOs;

public class LocationReportDto
{
    public string RoomCode { get; set; } = string.Empty;
    public string? BuildingName { get; set; }
    public string? FloorName { get; set; }
    public int TotalAssets { get; set; }
    public int ActiveAssets { get; set; }
    public int MaintenanceAssets { get; set; }
    public int RetiredAssets { get; set; }
    public List<string> Categories { get; set; } = new();
}
