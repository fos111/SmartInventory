using System;
using System.Collections.Generic;

namespace SmartInventory.Domain.Location.Entities
{
    public class Floor
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public int Level { get; set; }
        public string? Description { get; set; }
        public Guid BuildingId { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        public virtual Building Building { get; set; } = null!;
        public virtual ICollection<Room> Rooms { get; set; } = new List<Room>();
    }
}