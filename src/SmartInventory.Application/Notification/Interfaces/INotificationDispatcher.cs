using System;
using System.Threading;
using System.Threading.Tasks;
using SmartInventory.Application.Notification.DTOs;

namespace SmartInventory.Application.Notification.Interfaces;

public interface INotificationDispatcher
{
    Task DispatchAsync(Guid userId, NotificationDto notification, CancellationToken ct = default);
}
