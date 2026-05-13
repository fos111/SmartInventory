namespace SmartInventory.Application.Asset.DTOs.Reports;

public class DepartmentReportDto
{
    public string ZoneName { get; set; } = string.Empty;
    public int TotalAssets { get; set; }
    public double AvailabilityRate { get; set; }
    public int CriticalCount { get; set; }
    public int MaintenanceCount { get; set; }
    public int InStockCount { get; set; }
}
