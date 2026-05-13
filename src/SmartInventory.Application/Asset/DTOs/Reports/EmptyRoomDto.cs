namespace SmartInventory.Application.Asset.DTOs.Reports;

public class EmptyRoomDto
{
    public string RoomCode { get; set; } = string.Empty;
    public string? ZoneName { get; set; }
    public string? BuildingName { get; set; }
    public int AssetCount { get; set; }
}
