using SmartInventory.Domain.Notification.Enums;
using SmartInventory.Domain.Auth.Enums;

namespace SmartInventory.Application.Notification.DTOs;

public record CreateNotificationDto
{
    public required NotificationEventType EventType { get; init; }
    public required NotificationType Type { get; init; }
    public required string Title { get; init; }
    public required string Message { get; init; }
    public Guid? AssetId { get; init; }
    public List<Guid>? TargetUserIds { get; init; }
    public UserRole? TargetRole { get; init; }
}
