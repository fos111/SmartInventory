namespace SmartInventory.Application.Asset.DTOs;

public class InventorySummaryDto
{
    public string GroupKey { get; set; } = string.Empty;
    public string GroupLabel { get; set; } = string.Empty;
    public int Count { get; set; }
}
