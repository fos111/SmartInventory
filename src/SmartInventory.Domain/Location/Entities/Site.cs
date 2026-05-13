using System;
using System.Collections.Generic;

namespace SmartInventory.Domain.Location.Entities
{
    public class Site
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        public virtual ICollection<Zone> Zones { get; set; } = new List<Zone>();
    }
}