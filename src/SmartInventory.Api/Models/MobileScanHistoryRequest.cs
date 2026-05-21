namespace SmartInventory.Api.Models;

public class MobileProductScanHistoryRequest
{
    public string AssetTag { get; set; } = string.Empty;
}

public class MobileLookupScanHistoryRequest
{
    public string DepartmentCode { get; set; } = string.Empty;
    public string DepartmentName { get; set; } = string.Empty;
}
