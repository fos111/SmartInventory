using SmartInventory.Application.UserPreferences.DTOs;
using SmartInventory.Domain.Auth.Enums;

namespace SmartInventory.Application.UserPreferences.Interfaces;

public interface IUserPreferenceService
{
    Task<UserPreferencesResponse> GetPreferencesAsync(Guid userId, UserRole role);
    Task<RoleDefaultsResponse> GetRoleDefaultsAsync(UserRole role);
    Task<UserPreferencesResponse> UpdatePreferencesAsync(Guid userId, UserRole role, Dictionary<string, string> preferences);
}
