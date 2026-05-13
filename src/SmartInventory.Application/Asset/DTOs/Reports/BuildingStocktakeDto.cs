namespace SmartInventory.Application.Asset.DTOs.Reports;

public class BuildingStocktakeDto
{
    public string BuildingName { get; set; } = string.Empty;
    public int FloorCount { get; set; }
    public int TotalAssets { get; set; }
    public List<string> Categories { get; set; } = new();
}
