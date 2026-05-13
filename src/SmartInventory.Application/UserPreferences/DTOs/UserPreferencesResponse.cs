namespace SmartInventory.Application.UserPreferences.DTOs;

public class UserPreferencesResponse
{
    public Dictionary<string, string> Preferences { get; set; } = new();
    public Dictionary<string, string> RoleDefaults { get; set; } = new();
}
