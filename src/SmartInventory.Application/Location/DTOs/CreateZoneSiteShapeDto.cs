using System;

namespace SmartInventory.Application.Location.DTOs;

public class CreateZoneSiteShapeDto
{
    public Guid ZoneId { get; set; }
    public string Points { get; set; } = "[]";
    public string? Color { get; set; }
    public string? Label { get; set; }
}
