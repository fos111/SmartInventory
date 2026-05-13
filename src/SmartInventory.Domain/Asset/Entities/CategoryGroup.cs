namespace SmartInventory.Domain.Asset.Entities;

public class CategoryGroup
{
    public int Id { get; set; }
    public string Category { get; set; } = string.Empty;
    public string Group { get; set; } = string.Empty;
}