using System;

namespace SmartInventory.Application.Location.DTOs;

public class RoomGeometryDto
{
    public Guid Id { get; set; }
    public Guid RoomId { get; set; }
    public string ShapeType { get; set; } = "rectangle";
    public double X { get; set; }
    public double Y { get; set; }
    public double Width { get; set; }
    public double Height { get; set; }
    public string Color { get; set; } = "#e2e8f0";
    public string Stroke { get; set; } = "#94a3b8";
}
