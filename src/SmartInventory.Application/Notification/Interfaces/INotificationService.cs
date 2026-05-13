using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SmartInventory.Domain.Notification.Enums;
using SmartInventory.Application.Notification.DTOs;

namespace SmartInventory.Application.Notification.Interfaces;

public interface INotificationService
{
    Task CreateNotificationAsync(Guid userId, Guid? assetId, NotificationType type, string title, string message, NotificationEventType? eventType = null);
    Task<IEnumerable<NotificationDto>> GetUserNotificationsAsync(Guid userId, bool unreadOnly = false);
    Task MarkAsReadAsync(Guid notificationId, Guid userId);
    Task<int> GetUnreadCountAsync(Guid userId);
}