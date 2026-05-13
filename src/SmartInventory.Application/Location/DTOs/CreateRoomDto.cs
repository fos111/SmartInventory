using System;
using System.ComponentModel.DataAnnotations;

namespace SmartInventory.Application.Location.DTOs
{
    public class CreateRoomDto
    {
        [Required]
        [MaxLength(20)]
        [RegularExpression(@"^[A-Za-z0-9\-]+$", ErrorMessage = "Code must contain only alphanumeric characters and hyphens.")]
        public string Code { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Description { get; set; }

        [Required]
        public Guid FloorId { get; set; }

        // Optional geometry (when creating from map editor)
        public double? BoundsX { get; set; }
        public double? BoundsY { get; set; }
        public double? BoundsWidth { get; set; }
        public double? BoundsHeight { get; set; }
        public string? Color { get; set; }
        public string? Stroke { get; set; }
    }
}