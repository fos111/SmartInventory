using System;
using System.Collections.Generic;

namespace SmartInventory.Application.Location.DTOs
{
    public class HierarchyDto
    {
        public SiteDto Site { get; set; } = null!;
    }

    public class SiteDto
    {
        public Guid Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public List<ZoneDto> Zones { get; set; } = new();
    }

    public class ZoneDto
    {
        public Guid Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public List<BuildingDto> Buildings { get; set; } = new();
    }

    public class BuildingDto
    {
        public Guid Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public List<FloorDto> Floors { get; set; } = new();
    }

    public class FloorDto
    {
        public Guid Id { get; set; }
        public Guid BuildingId { get; set; }
        public int Level { get; set; }
        public string? Description { get; set; }
        public List<RoomDto> Rooms { get; set; } = new();
    }

    public class RoomDto
    {
        public Guid Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int FloorLevel { get; set; }

        // Geometry (from RoomGeometry, nullable for rooms without map data)
        public double? BoundsX { get; set; }
        public double? BoundsY { get; set; }
        public double? BoundsWidth { get; set; }
        public double? BoundsHeight { get; set; }
        public string? Color { get; set; }
        public string? Stroke { get; set; }
    }
}