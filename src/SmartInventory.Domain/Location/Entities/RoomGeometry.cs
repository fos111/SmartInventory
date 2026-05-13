using System;

namespace SmartInventory.Domain.Location.Entities
{
    public class RoomGeometry
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid RoomId { get; set; }
        public string ShapeType { get; set; } = "rectangle";
        public double X { get; set; }
        public double Y { get; set; }
        public double Width { get; set; }
        public double Height { get; set; }
        public string Color { get; set; } = "#e2e8f0";
        public string Stroke { get; set; } = "#94a3b8";
        public string? Properties { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        public virtual Room Room { get; set; } = null!;
    }
}
