using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using SmartInventory.Application.Notification.DTOs;
using SmartInventory.Application.Notification.Interfaces;

namespace SmartInventory.Api.Hubs;

public class SignalRNotificationDispatcher : INotificationDispatcher
{
    private readonly IHubContext<NotificationsHub> _hubContext;

    public SignalRNotificationDispatcher(IHubContext<NotificationsHub> hubContext)
    {
        _hubContext = hubContext;
    }

    public async Task DispatchAsync(Guid userId, NotificationDto notification, CancellationToken ct = default)
    {
        await _hubContext.Clients
            .Group(userId.ToString())
            .SendAsync("ReceiveNotification", notification, ct);
    }
}
