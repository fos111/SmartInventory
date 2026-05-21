using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NotificationEntity = SmartInventory.Domain.Notification.Entities.Notification;

namespace SmartInventory.Application.Notification.Interfaces;

public interface INotificationRepository
{
    Task<NotificationEntity> AddAsync(NotificationEntity notification);
    Task<IEnumerable<NotificationEntity>> GetByUserIdAsync(Guid userId, bool unreadOnly = false);
    Task<NotificationEntity?> GetByIdAsync(Guid id);
    Task MarkAsReadAsync(Guid id);
    Task<int> GetUnreadCountAsync(Guid userId);
    Task<int> MarkAllAsReadAsync(Guid userId);
    Task<bool> DeleteNotificationAsync(Guid id, Guid userId);
    Task<int> DeleteAllNotificationsAsync(Guid userId);
}