using System;

namespace SmartInventory.Application.Mobile.Products.DTOs;

public class AssetScanDto
{
    public Guid Id { get; set; }
    public string AssetTag { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string CurrentRoomCode { get; set; } = string.Empty;
    public string? PhotoUrl { get; set; }
    public string? StatusEntryNote { get; set; }
    public string? StatusExitNote { get; set; }
}
