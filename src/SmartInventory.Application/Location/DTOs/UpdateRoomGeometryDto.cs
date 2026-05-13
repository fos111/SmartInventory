namespace SmartInventory.Application.Location.DTOs;

public class UpdateRoomGeometryDto
{
    public double? X { get; set; }
    public double? Y { get; set; }
    public double? Width { get; set; }
    public double? Height { get; set; }
    public string? Color { get; set; }
    public string? Stroke { get; set; }
}
