using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SmartInventory.Application.Mobile.Notifications.DTOs;
using SmartInventory.Application.Mobile.Notifications.Interfaces;
using SmartInventory.Application.Notification.DTOs;
using SmartInventory.Application.Notification.Interfaces;

namespace SmartInventory.Application.Mobile.Notifications.Services;

public class MobileNotificationService : IMobileNotificationService
{
    private readonly INotificationService _notificationService;

    public MobileNotificationService(INotificationService notificationService)
    {
        _notificationService = notificationService;
    }

    public async Task<List<MobileNotificationListItemDto>> GetNotificationsAsync(
        Guid userId, bool unreadOnly, CancellationToken ct = default)
    {
        var notifications = await _notificationService.GetUserNotificationsAsync(userId, unreadOnly);
        return notifications.Select(MapToListItem).ToList();
    }

    public async Task<int> GetUnreadCountAsync(Guid userId, CancellationToken ct = default)
    {
        return await _notificationService.GetUnreadCountAsync(userId);
    }

    public async Task MarkAsReadAsync(Guid id, Guid userId, CancellationToken ct = default)
    {
        await _notificationService.MarkAsReadAsync(id, userId);
    }

    public async Task<int> MarkAllAsReadAsync(Guid userId, CancellationToken ct = default)
    {
        return await _notificationService.MarkAllAsReadAsync(userId);
    }

    public async Task<bool> DeleteNotificationAsync(Guid id, Guid userId, CancellationToken ct = default)
    {
        return await _notificationService.DeleteNotificationAsync(id, userId);
    }

    public async Task<int> DeleteAllNotificationsAsync(Guid userId, CancellationToken ct = default)
    {
        return await _notificationService.DeleteAllNotificationsAsync(userId);
    }

    private static MobileNotificationListItemDto MapToListItem(NotificationDto dto)
    {
        return new MobileNotificationListItemDto
        {
            Id = dto.Id,
            Type = dto.Type,
            Title = dto.Title,
            Message = dto.Message,
            IsRead = dto.IsRead,
            CreatedAt = dto.CreatedAt
        };
    }
}
