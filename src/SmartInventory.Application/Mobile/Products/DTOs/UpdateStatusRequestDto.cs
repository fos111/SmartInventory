using System.ComponentModel.DataAnnotations;

namespace SmartInventory.Application.Mobile.Products.DTOs;

public class UpdateStatusRequestDto
{
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// Note describing the reason for the status change.
    /// Required when transitioning TO Maintenance or CriticalIssue.
    /// Optional when resolving/leaving Maintenance or CriticalIssue.
    /// </summary>
    [MaxLength(1000)]
    public string? Note { get; set; }
}
