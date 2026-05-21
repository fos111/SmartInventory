using SmartInventory.Domain.Asset.Enums;

namespace SmartInventory.Application.Asset.DTOs.Reports;

/// <summary>
/// Per-room asset counts grouped by status at the DB level.
/// Used for ZoneInventory, BuildingStocktake, EmptyRoom, Department reports.
/// </summary>
public class RoomAssetCountDto
{
    public string RoomCode { get; set; } = string.Empty;
    public AssetStatus Status { get; set; }
    public int Count { get; set; }
}
