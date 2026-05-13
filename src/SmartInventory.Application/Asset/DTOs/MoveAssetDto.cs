using System.ComponentModel.DataAnnotations;

namespace SmartInventory.Application.Asset.DTOs;

public class MoveAssetDto
{
    [Required]
    [MaxLength(20)]
    public string NewRoomCode { get; set; } = string.Empty;
}