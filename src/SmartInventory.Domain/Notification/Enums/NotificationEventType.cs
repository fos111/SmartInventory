namespace SmartInventory.Domain.Notification.Enums;

/// <summary>
/// Granular event types for notifications. Maps to the event taxonomy used
/// for per-user alert preferences.
/// Convention: Category + EventName (PascalCase, no separators)
/// </summary>
public enum NotificationEventType
{
    // Equipment Status Changes
    EquipmentStatusCriticalIssue,
    EquipmentStatusMaintenance,
    EquipmentStatusLost,
    EquipmentStatusRetired,
    EquipmentStatusInStock,
    EquipmentStatusOperational,

    // Asset CRUD
    EquipmentCrudCreated,
    EquipmentCrudUpdated,
    EquipmentCrudDeleted,
    EquipmentCrudBulkDeleted,

    // Maintenance
    MaintenanceScheduled,
    MaintenanceDueSoon,
    MaintenanceOverdue,

    // Bulk Import
    ImportCompletedSuccess,
    ImportCompletedWarnings,
    ImportCompletedErrors,

    // Location / Reconciliation
    LocationMismatch,
    LocationReconciled,

    // Facility CRUD
    FacilityBuildingCreated,
    FacilityBuildingUpdated,
    FacilityBuildingDeleted,
    FacilityFloorCreated,
    FacilityFloorUpdated,
    FacilityFloorDeleted,
    FacilityRoomCreated,
    FacilityRoomUpdated,
    FacilityRoomDeleted,

    // Auth & Account
    AuthLoginSuccess,
    AuthLoginError,
    AuthRegisterError,
    AuthRegisterSuccess,
    AuthEmailConfirmed,
    AuthUserApproved,
    AuthUserRejected,
    AuthReEvaluationRequested,

    // Settings
    SettingsProfileUpdated,
    SettingsPasswordChanged,
    SettingsPreferencesSaved,
}
