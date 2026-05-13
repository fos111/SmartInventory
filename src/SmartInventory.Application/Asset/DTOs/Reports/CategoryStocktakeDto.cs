namespace SmartInventory.Application.Asset.DTOs.Reports;

public class CategoryStocktakeDto
{
    public string Category { get; set; } = string.Empty;
    public int Count { get; set; }
    public double Percentage { get; set; }
    public List<StatusCountItem> StatusBreakdown { get; set; } = new();
}

public class StatusCountItem
{
    public string Status { get; set; } = string.Empty;
    public int Count { get; set; }
}
