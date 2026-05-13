using System.ComponentModel.DataAnnotations;

namespace SmartInventory.Application.Asset.DTOs;

public class UpdateRfidDto
{
    [MaxLength(50)]
    public string? RfidTagId { get; set; }
}