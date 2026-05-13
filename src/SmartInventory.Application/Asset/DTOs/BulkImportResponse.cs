namespace SmartInventory.Application.Asset.DTOs;

public class BulkImportResponse
{
    public string JobId { get; set; } = string.Empty;
    public int TotalRows { get; set; }
    public int ProcessedRows { get; set; }
    public int SuccessCount { get; set; }
    public int FailureCount { get; set; }
    public List<BulkImportError> Errors { get; set; } = new();
    public string Status { get; set; } = string.Empty;
}

public class BulkImportError
{
    public int RowNumber { get; set; }
    public string AssetTag { get; set; } = string.Empty;
    public string ErrorMessage { get; set; } = string.Empty;
}