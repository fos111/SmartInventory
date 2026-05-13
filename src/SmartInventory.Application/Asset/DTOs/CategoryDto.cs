namespace SmartInventory.Application.Asset.DTOs;

public class CategoryDto
{
    public string Name { get; set; } = string.Empty;
    public string Group { get; set; } = string.Empty;
}

public class CategoryGroupDto
{
    public string Group { get; set; } = string.Empty;
    public List<string> Categories { get; set; } = new();
}