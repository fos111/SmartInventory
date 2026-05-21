namespace SmartInventory.Application.Asset.DTOs.Reports;

/// <summary>
/// Distinct categories per room. Used for LocationReport.
/// </summary>
public class RoomCategoryDto
{
    public string RoomCode { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
}
