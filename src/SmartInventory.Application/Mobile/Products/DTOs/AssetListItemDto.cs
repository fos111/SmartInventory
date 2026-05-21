using System;

namespace SmartInventory.Application.Mobile.Products.DTOs;

public class AssetListItemDto
{
    public Guid Id { get; set; }
    public string AssetTag { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string CurrentRoomCode { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public DateTime? LastSeen { get; set; }
    public bool HasDiscrepancy { get; set; }
    public bool IsDeleted { get; set; }
    public string? StatusEntryNote { get; set; }
    public string? StatusExitNote { get; set; }
}
