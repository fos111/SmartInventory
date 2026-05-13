using System;
using SmartInventory.Domain.Asset.Enums;

namespace SmartInventory.Application.Asset.DTOs;

public class AssetDto
{
    public Guid Id { get; set; }
    public string AssetTag { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Category { get; set; } = string.Empty;
    public AssetStatus Status { get; set; }
    public string? Manufacturer { get; set; }
    public string? Model { get; set; }
    public string? SerialNumber { get; set; }
    public string CurrentRoomCode { get; set; } = string.Empty;
    public string? DetectedRoomCode { get; set; }
    public DateTime? LastSeen { get; set; }
    public string? RfidTagId { get; set; }
    public DateTime? MaintenanceDueDate { get; set; }
    public DateTime? LastMaintenanceDate { get; set; }
    public DateTime? InstallDate { get; set; }
    public DateTime? LastServiceDate { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public bool HasDiscrepancy => !string.IsNullOrEmpty(DetectedRoomCode) && CurrentRoomCode != DetectedRoomCode;
    public string? ZoneCode { get; set; }
    public string? ZoneName { get; set; }
}