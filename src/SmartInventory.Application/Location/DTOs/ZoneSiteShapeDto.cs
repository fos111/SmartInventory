using System;

namespace SmartInventory.Application.Location.DTOs;

public class ZoneSiteShapeDto
{
    public Guid Id { get; set; }
    public Guid ZoneId { get; set; }
    public string Points { get; set; } = "[]";
    public string Color { get; set; } = "#3b82f6";
    public string? Label { get; set; }
}
