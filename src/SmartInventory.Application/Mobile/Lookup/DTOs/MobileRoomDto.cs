namespace SmartInventory.Application.Mobile.Lookup.DTOs;

public class MobileRoomDto
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int FloorLevel { get; set; }
    public string? BuildingName { get; set; }
    public string? ZoneName { get; set; }
}
