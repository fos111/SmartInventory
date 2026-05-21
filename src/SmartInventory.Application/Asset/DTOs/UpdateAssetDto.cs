using System;
using System.ComponentModel.DataAnnotations;
using SmartInventory.Domain.Asset.Enums;

namespace SmartInventory.Application.Asset.DTOs;

public class UpdateAssetDto
{
    [Required]
    [MaxLength(50)]
    public string AssetTag { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Description { get; set; }

    [Required]
    [MaxLength(50)]
    public string Category { get; set; } = string.Empty;

    public AssetStatus Status { get; set; }

    [MaxLength(100)]
    public string? Manufacturer { get; set; }

    [MaxLength(100)]
    public string? Model { get; set; }

    [MaxLength(100)]
    public string? SerialNumber { get; set; }

    [Required]
    [MaxLength(20)]
    public string CurrentRoomCode { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? PhotoPath { get; set; }

    [MaxLength(50)]
    public string? Price { get; set; }

    [MaxLength(100)]
    public string? BleId { get; set; }

    public DateTime? InstallDate { get; set; }
    public DateTime? LastServiceDate { get; set; }
}