using System;
using System.Collections.Generic;

namespace SmartInventory.Domain.Location.Entities
{
    public class Building
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public Guid ZoneId { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        public virtual Zone Zone { get; set; } = null!;
        public virtual ICollection<Floor> Floors { get; set; } = new List<Floor>();
    }
}