namespace SmartInventory.Domain.UserPreferences.Enums;

/// <summary>
/// Constants for preference key names used in the UserPreference key-value store.
/// Convention: pref:&lt;category&gt;:&lt;name&gt;
/// </summary>
public static class PreferenceKeys
{
    // Legacy alert preferences (3-category system — kept for backward compat)
    public const string AlertCritical = "pref:alert:critical";
    public const string AlertWarning = "pref:alert:warning";
    public const string AlertInfo = "pref:alert:info";

    // Equipment Status Changes
    public const string EquipmentStatusCriticalIssue = "pref:event:equipment:status.critical_issue";
    public const string EquipmentStatusMaintenance = "pref:event:equipment:status.maintenance";
    public const string EquipmentStatusLost = "pref:event:equipment:status.lost";
    public const string EquipmentStatusRetired = "pref:event:equipment:status.retired";
    public const string EquipmentStatusInStock = "pref:event:equipment:status.in_stock";
    public const string EquipmentStatusOperational = "pref:event:equipment:status.operational";

    // Asset CRUD
    public const string EquipmentCrudCreated = "pref:event:equipment:crud.created";
    public const string EquipmentCrudUpdated = "pref:event:equipment:crud.updated";
    public const string EquipmentCrudDeleted = "pref:event:equipment:crud.deleted";
    public const string EquipmentCrudBulkDeleted = "pref:event:equipment:crud.bulk_deleted";

    // Maintenance
    public const string MaintenanceScheduled = "pref:event:maintenance:scheduled";
    public const string MaintenanceDueSoon = "pref:event:maintenance:due_soon";
    public const string MaintenanceOverdue = "pref:event:maintenance:overdue";

    // Bulk Import
    public const string ImportCompletedSuccess = "pref:event:import:completed_success";
    public const string ImportCompletedWarnings = "pref:event:import:completed_warnings";
    public const string ImportCompletedErrors = "pref:event:import:completed_errors";

    // Location / Reconciliation
    public const string LocationMismatch = "pref:event:location:mismatch";
    public const string LocationReconciled = "pref:event:location:reconciled";

    // Facility CRUD
    public const string FacilityBuildingCreated = "pref:event:facility:building.created";
    public const string FacilityBuildingUpdated = "pref:event:facility:building.updated";
    public const string FacilityBuildingDeleted = "pref:event:facility:building.deleted";
    public const string FacilityFloorCreated = "pref:event:facility:floor.created";
    public const string FacilityFloorUpdated = "pref:event:facility:floor.updated";
    public const string FacilityFloorDeleted = "pref:event:facility:floor.deleted";
    public const string FacilityRoomCreated = "pref:event:facility:room.created";
    public const string FacilityRoomUpdated = "pref:event:facility:room.updated";
    public const string FacilityRoomDeleted = "pref:event:facility:room.deleted";

    // Auth & Account
    public const string AuthLoginSuccess = "pref:event:auth:login_success";
    public const string AuthLoginError = "pref:event:auth:login_error";
    public const string AuthRegisterError = "pref:event:auth:register_error";
    public const string AuthEmailConfirmed = "pref:event:auth:email_confirmed";

    // Settings
    public const string SettingsProfileUpdated = "pref:event:settings:profile_updated";
    public const string SettingsPasswordChanged = "pref:event:settings:password_changed";
    public const string SettingsPreferencesSaved = "pref:event:settings:preferences_saved";
}
