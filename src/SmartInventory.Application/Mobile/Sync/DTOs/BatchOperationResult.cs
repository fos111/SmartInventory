namespace SmartInventory.Application.Mobile.Sync.DTOs;

public class BatchOperationResult
{
    public int Index { get; set; }
    public bool Success { get; set; }
    public string AssetTag { get; set; } = string.Empty;
    public string? Error { get; set; }
}
