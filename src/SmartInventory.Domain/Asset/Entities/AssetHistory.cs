using SmartInventory.Domain.Asset.Entities;

namespace SmartInventory.Domain.Asset.Entities;

public class AssetHistory
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid AssetId { get; set; }
    public string PropertyChanged { get; set; } = string.Empty;
    public string? OldValue { get; set; }
    public string? NewValue { get; set; }
    public Guid ChangedBy { get; set; }
    public DateTime ChangedAt { get; set; } = DateTime.UtcNow;

    // Navigation property
    public virtual Asset? Asset { get; set; }
}
