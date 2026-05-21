namespace SmartInventory.Application.Mobile.Lookup.DTOs;

public class MobileDepartmentDto
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int RoomCount { get; set; }
}
