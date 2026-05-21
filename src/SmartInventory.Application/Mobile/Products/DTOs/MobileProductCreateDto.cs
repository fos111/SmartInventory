namespace SmartInventory.Application.Mobile.Products.DTOs;

/// <summary>
/// DTO for product create/update from the Flutter mobile app.
/// Matches the Node.js-style fields that Flutter sends via multipart/form-data.
/// </summary>
public class MobileProductCreateDto
{
    public string Name { get; set; } = string.Empty;

    /// <summary>Maps to Asset.AssetTag</summary>
    public string? Sku { get; set; }

    /// <summary>Maps to Asset.Category</summary>
    public string? Type { get; set; }

    public string? Description { get; set; }

    /// <summary>Room UUID or room code — resolved by service</summary>
    public string? RoomId { get; set; }

    /// <summary>URL returned by IFileStorageService after saving the photo</summary>
    public string? PhotoPath { get; set; }

    public string? Tags { get; set; }
    public string? Price { get; set; }
    public string? Specifications { get; set; }

    /// <summary>BLE (Bluetooth Low Energy) device identifier</summary>
    public string? BleId { get; set; }
}
