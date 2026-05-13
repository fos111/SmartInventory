namespace SmartInventory.Application.Asset.DTOs;

public class AssetReconciliationDto
{
    public Guid AssetId { get; set; }
    public string AssetTag { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string CurrentRoomCode { get; set; } = string.Empty;
    public string? DetectedRoomCode { get; set; }
    public bool HasDiscrepancy { get; set; }
    public string DiscrepancyType { get; set; } = string.Empty; // "Moved" or "NotDetected"
}