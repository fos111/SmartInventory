namespace SmartInventory.Application.Asset.DTOs.Reports;

public class StatusSummaryDto
{
    public string Status { get; set; } = string.Empty;
    public int Count { get; set; }
    public double Percentage { get; set; }
}
