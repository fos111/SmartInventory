using System;

namespace SmartInventory.Domain.Location.Entities
{
    public class ZoneSiteShape
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid ZoneId { get; set; }
        public string Points { get; set; } = "[]";
        public string Color { get; set; } = "#3b82f6";
        public string? Label { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        public virtual Zone Zone { get; set; } = null!;
    }
}
