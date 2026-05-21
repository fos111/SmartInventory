using System;

namespace SmartInventory.Application.Mobile.Products.DTOs;

public class CreateProductRequestDto
{
    public string? AssetTag { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Category { get; set; } = string.Empty;
    public string? Status { get; set; }
    public string? Manufacturer { get; set; }
    public string? Model { get; set; }
    public string? SerialNumber { get; set; }
    public string CurrentRoomCode { get; set; } = string.Empty;
    public DateTime? InstallDate { get; set; }
    public DateTime? LastServiceDate { get; set; }
}
