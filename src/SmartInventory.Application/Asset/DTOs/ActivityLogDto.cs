using System;

namespace SmartInventory.Application.Asset.DTOs;

/// <summary>
/// Unified activity feed DTO — used by ReportingService.GetActivityLogAsync.
/// Represents both asset history events and facility activity events.
/// </summary>
public class ActivityLogDto
{
    public Guid Id { get; set; }
    public Guid? AssetId { get; set; }
    public string AssetTag { get; set; } = string.Empty;
    public string AssetName { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public string? OldValue { get; set; }
    public string? NewValue { get; set; }
    public Guid ChangedBy { get; set; }
    public string ChangedByName { get; set; } = string.Empty;
    public DateTime ChangedAt { get; set; }

    // Facility-specific fields (populated for non-asset events)
    public string? EntityType { get; set; }
    public string? EntityId { get; set; }
    public string? Details { get; set; }
}
