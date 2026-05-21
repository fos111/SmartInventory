namespace SmartInventory.Application.Mobile.Home.DTOs;

public class ActivityLogEntryDto
{
    public Guid Id { get; set; }
    public string Action { get; set; } = string.Empty;
    public string EntityName { get; set; } = string.Empty;
    public string? Details { get; set; }
    public DateTime ChangedAt { get; set; }
    public string? ChangedByName { get; set; }
}
