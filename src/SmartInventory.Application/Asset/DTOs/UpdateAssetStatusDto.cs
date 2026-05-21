using System.ComponentModel.DataAnnotations;
using SmartInventory.Domain.Asset.Enums;

namespace SmartInventory.Application.Asset.DTOs;

public class UpdateAssetStatusDto
{
    [Required]
    public AssetStatus Status { get; set; }

    /// <summary>
    /// Note describing the reason for the status change.
    /// Required when transitioning TO Maintenance or CriticalIssue.
    /// Optional when transitioning OUT OF Maintenance/CriticalIssue (exit/resolution note).
    /// Max 1000 characters.
    /// </summary>
    [MaxLength(1000)]
    public string? Note { get; set; }
}