using System;

namespace SmartInventory.Domain.Location.Entities
{
    public class Room
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public Guid FloorId { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        public virtual Floor Floor { get; set; } = null!;
        public virtual RoomGeometry? RoomGeometry { get; set; }
    }
}