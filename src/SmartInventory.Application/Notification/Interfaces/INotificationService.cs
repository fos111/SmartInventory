using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using SmartInventory.Application.Notification.DTOs;

namespace SmartInventory.Application.Notification.Interfaces;

public interface INotificationService
{
    Task CreateNotificationAsync(CreateNotificationDto dto, CancellationToken ct = default);
    Task<IEnumerable<NotificationDto>> GetUserNotificationsAsync(Guid userId, bool unreadOnly = false);
    Task MarkAsReadAsync(Guid notificationId, Guid userId);
    Task<int> GetUnreadCountAsync(Guid userId);
    Task<int> MarkAllAsReadAsync(Guid userId);
    Task<bool> DeleteNotificationAsync(Guid id, Guid userId);
    Task<int> DeleteAllNotificationsAsync(Guid userId);
}