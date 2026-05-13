using SmartInventory.Domain.UserPreferences.Entities;

namespace SmartInventory.Application.UserPreferences.Interfaces;

public interface IUserPreferenceRepository
{
    Task<Dictionary<string, string>> GetUserPreferencesAsync(Guid userId);
    Task UpsertAsync(Guid userId, Dictionary<string, string> preferences);
}
