using System;
using System.Collections.Generic;

namespace SmartInventory.Application.Location.DTOs;

public class BatchUpdateRoomGeometriesDto
{
    public List<SingleRoomGeometryUpdate> Updates { get; set; } = new();
}

public class SingleRoomGeometryUpdate
{
    public Guid RoomId { get; set; }
    public double? X { get; set; }
    public double? Y { get; set; }
    public double? Width { get; set; }
    public double? Height { get; set; }
    public string? Color { get; set; }
    public string? Stroke { get; set; }
}
