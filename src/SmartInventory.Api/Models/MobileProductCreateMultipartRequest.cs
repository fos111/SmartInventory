using Microsoft.AspNetCore.Http;

namespace SmartInventory.Api.Models;

public class MobileProductCreateMultipartRequest
{
    public string Name { get; set; } = string.Empty;
    public string? Sku { get; set; }
    public string? Type { get; set; }
    public string? Description { get; set; }
    public string? RoomId { get; set; }

    public string? Tags { get; set; }
    public string? Price { get; set; }
    public string? Specifications { get; set; }

    /// <summary>BLE (Bluetooth Low Energy) device identifier</summary>
    public string? BleId { get; set; }

    public IFormFile? Photo { get; set; }
}
