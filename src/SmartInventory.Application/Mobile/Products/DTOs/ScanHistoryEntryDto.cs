using System;

namespace SmartInventory.Application.Mobile.Products.DTOs;

public class ScanHistoryEntryDto
{
    public Guid Id { get; set; }
    public string AssetTag { get; set; } = string.Empty;
    public string AssetName { get; set; } = string.Empty;
    public string? Location { get; set; }
    public DateTime ScannedAt { get; set; }
    public string? ScannedByName { get; set; }
}
