using SmartInventory.Domain.Notification.Enums;
using SmartInventory.Domain.UserPreferences.Enums;

namespace SmartInventory.Domain.Notification.Mappings;

public static class NotificationEventTypeMapping
{
    public static string ToPreferenceKey(this NotificationEventType eventType)
        => eventType switch
        {
            NotificationEventType.EquipmentStatusCriticalIssue => PreferenceKeys.EquipmentStatusCriticalIssue,
            NotificationEventType.EquipmentStatusMaintenance => PreferenceKeys.EquipmentStatusMaintenance,
            NotificationEventType.EquipmentStatusLost => PreferenceKeys.EquipmentStatusLost,
            NotificationEventType.EquipmentStatusRetired => PreferenceKeys.EquipmentStatusRetired,
            NotificationEventType.EquipmentStatusInStock => PreferenceKeys.EquipmentStatusInStock,
            NotificationEventType.EquipmentStatusOperational => PreferenceKeys.EquipmentStatusOperational,
            NotificationEventType.EquipmentCrudCreated => PreferenceKeys.EquipmentCrudCreated,
            NotificationEventType.EquipmentCrudUpdated => PreferenceKeys.EquipmentCrudUpdated,
            NotificationEventType.EquipmentCrudDeleted => PreferenceKeys.EquipmentCrudDeleted,
            NotificationEventType.EquipmentCrudBulkDeleted => PreferenceKeys.EquipmentCrudBulkDeleted,
            NotificationEventType.MaintenanceScheduled => PreferenceKeys.MaintenanceScheduled,
            NotificationEventType.MaintenanceDueSoon => PreferenceKeys.MaintenanceDueSoon,
            NotificationEventType.MaintenanceOverdue => PreferenceKeys.MaintenanceOverdue,
            NotificationEventType.ImportCompletedSuccess => PreferenceKeys.ImportCompletedSuccess,
            NotificationEventType.ImportCompletedWarnings => PreferenceKeys.ImportCompletedWarnings,
            NotificationEventType.ImportCompletedErrors => PreferenceKeys.ImportCompletedErrors,
            NotificationEventType.LocationMismatch => PreferenceKeys.LocationMismatch,
            NotificationEventType.LocationReconciled => PreferenceKeys.LocationReconciled,
            NotificationEventType.FacilityBuildingCreated => PreferenceKeys.FacilityBuildingCreated,
            NotificationEventType.FacilityBuildingUpdated => PreferenceKeys.FacilityBuildingUpdated,
            NotificationEventType.FacilityBuildingDeleted => PreferenceKeys.FacilityBuildingDeleted,
            NotificationEventType.FacilityFloorCreated => PreferenceKeys.FacilityFloorCreated,
            NotificationEventType.FacilityFloorUpdated => PreferenceKeys.FacilityFloorUpdated,
            NotificationEventType.FacilityFloorDeleted => PreferenceKeys.FacilityFloorDeleted,
            NotificationEventType.FacilityRoomCreated => PreferenceKeys.FacilityRoomCreated,
            NotificationEventType.FacilityRoomUpdated => PreferenceKeys.FacilityRoomUpdated,
            NotificationEventType.FacilityRoomDeleted => PreferenceKeys.FacilityRoomDeleted,
            NotificationEventType.AuthLoginSuccess => PreferenceKeys.AuthLoginSuccess,
            NotificationEventType.AuthLoginError => PreferenceKeys.AuthLoginError,
            NotificationEventType.AuthRegisterError => PreferenceKeys.AuthRegisterError,
            NotificationEventType.AuthRegisterSuccess => PreferenceKeys.AuthRegisterSuccess,
            NotificationEventType.AuthEmailConfirmed => PreferenceKeys.AuthEmailConfirmed,
            NotificationEventType.AuthUserApproved => PreferenceKeys.AuthUserApproved,
            NotificationEventType.AuthUserRejected => PreferenceKeys.AuthUserRejected,
            NotificationEventType.AuthReEvaluationRequested => PreferenceKeys.AuthReEvaluationRequested,
            NotificationEventType.SettingsProfileUpdated => PreferenceKeys.SettingsProfileUpdated,
            NotificationEventType.SettingsPasswordChanged => PreferenceKeys.SettingsPasswordChanged,
            NotificationEventType.SettingsPreferencesSaved => PreferenceKeys.SettingsPreferencesSaved,
            _ => throw new ArgumentOutOfRangeException(nameof(eventType), eventType, $"Unknown event type: {eventType}")
        };
}
