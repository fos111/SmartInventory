using SmartInventory.Domain.Notification.Enums;

namespace SmartInventory.Domain.Notification.Entities;

public class Notification
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid UserId { get; set; }
    public Guid? AssetId { get; set; }
    public NotificationType Type { get; set; }
    public NotificationEventType? EventType { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public bool IsRead { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}