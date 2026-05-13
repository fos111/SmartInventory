namespace SmartInventory.Application.UserPreferences.DTOs;

public class RoleDefaultsResponse
{
    public string Role { get; set; } = string.Empty;
    public Dictionary<string, string> Defaults { get; set; } = new();
}
