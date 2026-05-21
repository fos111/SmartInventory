using System.ComponentModel.DataAnnotations;

namespace SmartInventory.Application.Asset.DTOs;

public class UpdatePriceDto
{
    [MaxLength(50)]
    public string? Price { get; set; }
}
