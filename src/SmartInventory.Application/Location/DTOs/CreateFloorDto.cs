using System.ComponentModel.DataAnnotations;

namespace SmartInventory.Application.Location.DTOs;

public class CreateFloorDto
{
    [Required]
    public int Level { get; set; }

    [MaxLength(500)]
    public string? Description { get; set; }

    [Required]
    public Guid BuildingId { get; set; }
}