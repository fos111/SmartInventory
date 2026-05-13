using SmartInventory.Application.UserPreferences.DTOs;
using SmartInventory.Application.UserPreferences.Interfaces;
using SmartInventory.Domain.Auth.Enums;
using SmartInventory.Domain.UserPreferences.Enums;

namespace SmartInventory.Application.UserPreferences.Services;

public class UserPreferenceService : IUserPreferenceService
{
    private readonly IUserPreferenceRepository _repository;

    private static readonly Dictionary<UserRole, Dictionary<string, string>> RoleDefaults = new()
    {
        [UserRole.Technicien] = new Dictionary<string, string>
        {
            // Legacy alert preferences (kept for backward compat)
            [PreferenceKeys.AlertCritical] = "true",
            [PreferenceKeys.AlertWarning] = "true",
            [PreferenceKeys.AlertInfo] = "false",

            // Equipment Status Changes — ON for all roles
            [PreferenceKeys.EquipmentStatusCriticalIssue] = "true",
            [PreferenceKeys.EquipmentStatusMaintenance] = "true",
            [PreferenceKeys.EquipmentStatusLost] = "true",
            [PreferenceKeys.EquipmentStatusRetired] = "true",
            [PreferenceKeys.EquipmentStatusInStock] = "true",
            [PreferenceKeys.EquipmentStatusOperational] = "true",

            // Asset CRUD — OFF for Technicien
            [PreferenceKeys.EquipmentCrudCreated] = "false",
            [PreferenceKeys.EquipmentCrudUpdated] = "false",
            [PreferenceKeys.EquipmentCrudDeleted] = "false",
            [PreferenceKeys.EquipmentCrudBulkDeleted] = "false",

            // Maintenance — ON for all roles
            [PreferenceKeys.MaintenanceScheduled] = "true",
            [PreferenceKeys.MaintenanceDueSoon] = "true",
            [PreferenceKeys.MaintenanceOverdue] = "true",

            // Bulk Import — ON for all roles
            [PreferenceKeys.ImportCompletedSuccess] = "true",
            [PreferenceKeys.ImportCompletedWarnings] = "true",
            [PreferenceKeys.ImportCompletedErrors] = "true",

            // Location — OFF for Technicien
            [PreferenceKeys.LocationMismatch] = "false",
            [PreferenceKeys.LocationReconciled] = "false",

            // Facilities — OFF for all roles
            [PreferenceKeys.FacilityBuildingCreated] = "false",
            [PreferenceKeys.FacilityBuildingUpdated] = "false",
            [PreferenceKeys.FacilityBuildingDeleted] = "false",
            [PreferenceKeys.FacilityFloorCreated] = "false",
            [PreferenceKeys.FacilityFloorUpdated] = "false",
            [PreferenceKeys.FacilityFloorDeleted] = "false",
            [PreferenceKeys.FacilityRoomCreated] = "false",
            [PreferenceKeys.FacilityRoomUpdated] = "false",
            [PreferenceKeys.FacilityRoomDeleted] = "false",

            // Auth — OFF for all roles
            [PreferenceKeys.AuthLoginSuccess] = "false",
            [PreferenceKeys.AuthLoginError] = "false",
            [PreferenceKeys.AuthRegisterError] = "false",
            [PreferenceKeys.AuthEmailConfirmed] = "false",

            // Settings — OFF for all roles
            [PreferenceKeys.SettingsProfileUpdated] = "false",
            [PreferenceKeys.SettingsPasswordChanged] = "false",
            [PreferenceKeys.SettingsPreferencesSaved] = "false",
        },
        [UserRole.Supervisor] = new Dictionary<string, string>
        {
            // Legacy alert preferences (kept for backward compat)
            [PreferenceKeys.AlertCritical] = "true",
            [PreferenceKeys.AlertWarning] = "true",
            [PreferenceKeys.AlertInfo] = "true",

            // Equipment Status Changes — ON for all roles
            [PreferenceKeys.EquipmentStatusCriticalIssue] = "true",
            [PreferenceKeys.EquipmentStatusMaintenance] = "true",
            [PreferenceKeys.EquipmentStatusLost] = "true",
            [PreferenceKeys.EquipmentStatusRetired] = "true",
            [PreferenceKeys.EquipmentStatusInStock] = "true",
            [PreferenceKeys.EquipmentStatusOperational] = "true",

            // Asset CRUD — ON for Supervisor
            [PreferenceKeys.EquipmentCrudCreated] = "true",
            [PreferenceKeys.EquipmentCrudUpdated] = "true",
            [PreferenceKeys.EquipmentCrudDeleted] = "true",
            [PreferenceKeys.EquipmentCrudBulkDeleted] = "true",

            // Maintenance — ON for all roles
            [PreferenceKeys.MaintenanceScheduled] = "true",
            [PreferenceKeys.MaintenanceDueSoon] = "true",
            [PreferenceKeys.MaintenanceOverdue] = "true",

            // Bulk Import — ON for all roles
            [PreferenceKeys.ImportCompletedSuccess] = "true",
            [PreferenceKeys.ImportCompletedWarnings] = "true",
            [PreferenceKeys.ImportCompletedErrors] = "true",

            // Location — ON for Supervisor
            [PreferenceKeys.LocationMismatch] = "true",
            [PreferenceKeys.LocationReconciled] = "true",

            // Facilities — OFF for all roles
            [PreferenceKeys.FacilityBuildingCreated] = "false",
            [PreferenceKeys.FacilityBuildingUpdated] = "false",
            [PreferenceKeys.FacilityBuildingDeleted] = "false",
            [PreferenceKeys.FacilityFloorCreated] = "false",
            [PreferenceKeys.FacilityFloorUpdated] = "false",
            [PreferenceKeys.FacilityFloorDeleted] = "false",
            [PreferenceKeys.FacilityRoomCreated] = "false",
            [PreferenceKeys.FacilityRoomUpdated] = "false",
            [PreferenceKeys.FacilityRoomDeleted] = "false",

            // Auth — OFF for all roles
            [PreferenceKeys.AuthLoginSuccess] = "false",
            [PreferenceKeys.AuthLoginError] = "false",
            [PreferenceKeys.AuthRegisterError] = "false",
            [PreferenceKeys.AuthEmailConfirmed] = "false",

            // Settings — OFF for all roles
            [PreferenceKeys.SettingsProfileUpdated] = "false",
            [PreferenceKeys.SettingsPasswordChanged] = "false",
            [PreferenceKeys.SettingsPreferencesSaved] = "false",
        },
        [UserRole.Gestionnaire] = new Dictionary<string, string>
        {
            // Legacy alert preferences (kept for backward compat)
            [PreferenceKeys.AlertCritical] = "true",
            [PreferenceKeys.AlertWarning] = "true",
            [PreferenceKeys.AlertInfo] = "true",

            // Equipment Status Changes — ON for all roles
            [PreferenceKeys.EquipmentStatusCriticalIssue] = "true",
            [PreferenceKeys.EquipmentStatusMaintenance] = "true",
            [PreferenceKeys.EquipmentStatusLost] = "true",
            [PreferenceKeys.EquipmentStatusRetired] = "true",
            [PreferenceKeys.EquipmentStatusInStock] = "true",
            [PreferenceKeys.EquipmentStatusOperational] = "true",

            // Asset CRUD — ON for Gestionnaire
            [PreferenceKeys.EquipmentCrudCreated] = "true",
            [PreferenceKeys.EquipmentCrudUpdated] = "true",
            [PreferenceKeys.EquipmentCrudDeleted] = "true",
            [PreferenceKeys.EquipmentCrudBulkDeleted] = "true",

            // Maintenance — ON for all roles
            [PreferenceKeys.MaintenanceScheduled] = "true",
            [PreferenceKeys.MaintenanceDueSoon] = "true",
            [PreferenceKeys.MaintenanceOverdue] = "true",

            // Bulk Import — ON for all roles
            [PreferenceKeys.ImportCompletedSuccess] = "true",
            [PreferenceKeys.ImportCompletedWarnings] = "true",
            [PreferenceKeys.ImportCompletedErrors] = "true",

            // Location — ON for Gestionnaire
            [PreferenceKeys.LocationMismatch] = "true",
            [PreferenceKeys.LocationReconciled] = "true",

            // Facilities — OFF for all roles
            [PreferenceKeys.FacilityBuildingCreated] = "false",
            [PreferenceKeys.FacilityBuildingUpdated] = "false",
            [PreferenceKeys.FacilityBuildingDeleted] = "false",
            [PreferenceKeys.FacilityFloorCreated] = "false",
            [PreferenceKeys.FacilityFloorUpdated] = "false",
            [PreferenceKeys.FacilityFloorDeleted] = "false",
            [PreferenceKeys.FacilityRoomCreated] = "false",
            [PreferenceKeys.FacilityRoomUpdated] = "false",
            [PreferenceKeys.FacilityRoomDeleted] = "false",

            // Auth — OFF for all roles
            [PreferenceKeys.AuthLoginSuccess] = "false",
            [PreferenceKeys.AuthLoginError] = "false",
            [PreferenceKeys.AuthRegisterError] = "false",
            [PreferenceKeys.AuthEmailConfirmed] = "false",

            // Settings — OFF for all roles
            [PreferenceKeys.SettingsProfileUpdated] = "false",
            [PreferenceKeys.SettingsPasswordChanged] = "false",
            [PreferenceKeys.SettingsPreferencesSaved] = "false",
        },
    };

    public UserPreferenceService(IUserPreferenceRepository repository)
    {
        _repository = repository;
    }

    public async Task<UserPreferencesResponse> GetPreferencesAsync(Guid userId, UserRole role)
    {
        var userPrefs = await _repository.GetUserPreferencesAsync(userId);
        var defaults = GetDefaultsForRole(role);
        var merged = new Dictionary<string, string>(defaults);

        foreach (var (key, value) in userPrefs)
            merged[key] = value;

        return new UserPreferencesResponse
        {
            Preferences = merged,
            RoleDefaults = defaults,
        };
    }

    public Task<RoleDefaultsResponse> GetRoleDefaultsAsync(UserRole role)
    {
        var defaults = GetDefaultsForRole(role);
        return Task.FromResult(new RoleDefaultsResponse
        {
            Role = role.ToString(),
            Defaults = defaults,
        });
    }

    public async Task<UserPreferencesResponse> UpdatePreferencesAsync(Guid userId, UserRole role, Dictionary<string, string> preferences)
    {
        await _repository.UpsertAsync(userId, preferences);
        return await GetPreferencesAsync(userId, role);
    }

    private Dictionary<string, string> GetDefaultsForRole(UserRole role)
    {
        return RoleDefaults.TryGetValue(role, out var defaults)
            ? new Dictionary<string, string>(defaults)
            : new Dictionary<string, string>();
    }
}
