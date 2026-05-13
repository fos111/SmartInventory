namespace SmartInventory.Application.Asset.DTOs.Reports;

public class LocationDiscrepancyDto
{
    public Guid Id { get; set; }
    public string AssetTag { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string CurrentRoomCode { get; set; } = string.Empty;
    public string? DetectedRoomCode { get; set; }
}
