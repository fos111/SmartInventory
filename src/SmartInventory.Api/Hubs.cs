using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using SmartInventory.Application.Notification.DTOs;
using SmartInventory.Application.Notification.Interfaces;

namespace SmartInventory.Api.Hubs;

public class NotificationsHub : Hub
{
    private readonly INotificationService _notificationService;

    public NotificationsHub(INotificationService notificationService)
    {
        _notificationService = notificationService;
    }

    public async Task JoinUserGroup(string userId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, userId);
    }

    public async Task LeaveUserGroup(string userId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, userId);
    }

    public async Task<List<NotificationDto>> GetUserNotifications(string userId, bool unreadOnly = false)
    {
        var notifications = await _notificationService.GetUserNotificationsAsync(Guid.Parse(userId), unreadOnly);
        return notifications.ToList();
    }

    public async Task MarkAsRead(string notificationId, string userId)
    {
        await _notificationService.MarkAsReadAsync(Guid.Parse(notificationId), Guid.Parse(userId));
    }

    public async Task<int> GetUnreadCount(string userId)
    {
        return await _notificationService.GetUnreadCountAsync(Guid.Parse(userId));
    }
}