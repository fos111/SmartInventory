using System;
using System.Collections.Generic;

namespace SmartInventory.Domain.Location.Entities
{
    public class Zone
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public Guid SiteId { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        public virtual Site Site { get; set; } = null!;
        public virtual ICollection<Building> Buildings { get; set; } = new List<Building>();
    }
}