namespace SmartInventory.Application.Asset.DTOs;

public class AssetHistoryDto
{
    public Guid Id { get; set; }
    public Guid AssetId { get; set; }
    public string AssetTag { get; set; } = string.Empty;
    public string PropertyChanged { get; set; } = string.Empty;
    public string? OldValue { get; set; }
    public string? NewValue { get; set; }
    public Guid ChangedBy { get; set; }
    public DateTime ChangedAt { get; set; }
}
