namespace SmartInventory.Application.Mobile.Lookup.DTOs;

public class BarcodeCheckResultDto
{
    public bool Exists { get; set; }
    public string? AssetTag { get; set; }
    public string? Name { get; set; }
}
