namespace SmartInventory.Application.Asset.DTOs.Reports;

public class ZoneInventoryDto
{
    public string ZoneName { get; set; } = string.Empty;
    public string BuildingName { get; set; } = string.Empty;
    public int FloorLevel { get; set; }
    public string RoomCode { get; set; } = string.Empty;
    public int TotalAssets { get; set; }
    public int ActiveCount { get; set; }
    public int MaintenanceCount { get; set; }
    public int CriticalCount { get; set; }
    public int InStockCount { get; set; }
}
