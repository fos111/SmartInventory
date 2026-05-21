using System.ComponentModel.DataAnnotations;

namespace SmartInventory.Application.Asset.DTOs;

public class UpdateBleIdDto
{
    [MaxLength(100)]
    public string? BleId { get; set; }
}
