using System.ComponentModel.DataAnnotations;

namespace SmartInventory.Application.Location.DTOs;

public class CreateBuildingDto
{
    [Required]
    [MaxLength(50)]
    [RegularExpression(@"^[A-Za-z0-9\-]+$", ErrorMessage = "Code must contain only alphanumeric characters and hyphens.")]
    public string Code { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Description { get; set; }

    [Required]
    public Guid ZoneId { get; set; }
}