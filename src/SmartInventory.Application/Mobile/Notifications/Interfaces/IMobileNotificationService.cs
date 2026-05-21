using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using SmartInventory.Application.Mobile.Notifications.DTOs;

namespace SmartInventory.Application.Mobile.Notifications.Interfaces;

public interface IMobileNotificationService
{
    Task<List<MobileNotificationListItemDto>> GetNotificationsAsync(
        Guid userId, bool unreadOnly, CancellationToken ct = default);

    Task<int> GetUnreadCountAsync(Guid userId, CancellationToken ct = default);

    Task MarkAsReadAsync(Guid id, Guid userId, CancellationToken ct = default);

    Task<int> MarkAllAsReadAsync(Guid userId, CancellationToken ct = default);

    Task<bool> DeleteNotificationAsync(Guid id, Guid userId, CancellationToken ct = default);

    Task<int> DeleteAllNotificationsAsync(Guid userId, CancellationToken ct = default);
}
