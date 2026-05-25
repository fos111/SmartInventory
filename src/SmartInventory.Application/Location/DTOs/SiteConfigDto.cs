using System.Collections.Generic;

namespace SmartInventory.Application.Location.DTOs;

public class SiteConfigDto
{
    public string? SatelliteImageUrl { get; set; }
    public List<ZoneSiteShapeDto> ZoneShapes { get; set; } = new();
}
