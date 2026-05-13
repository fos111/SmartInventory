using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using SmartInventory.Application.Notification.DTOs;
using SmartInventory.Application.Notification.Interfaces;
using SmartInventory.Domain.Notification.Entities;
using SmartInventory.Domain.Notification.Enums;

namespace SmartInventory.Application.Notification.Services;

public class NotificationService : INotificationService
{
    private readonly INotificationRepository _repository;
    private readonly IMapper _mapper;

    public NotificationService(INotificationRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task CreateNotificationAsync(Guid userId, Guid? assetId, NotificationType type, string title, string message, NotificationEventType? eventType = null)
    {
        var notification = new Domain.Notification.Entities.Notification
        {
            UserId = userId,
            AssetId = assetId,
            Type = type,
            EventType = eventType,
            Title = title,
            Message = message,
            IsRead = false,
            CreatedAt = DateTime.UtcNow
        };

        await _repository.AddAsync(notification);
    }

    public async Task<IEnumerable<NotificationDto>> GetUserNotificationsAsync(Guid userId, bool unreadOnly = false)
    {
        var notifications = await _repository.GetByUserIdAsync(userId, unreadOnly);
        return notifications.Select(n => new NotificationDto
        {
            Id = n.Id,
            UserId = n.UserId,
            AssetId = n.AssetId,
            Type = n.Type.ToString(),
            EventType = n.EventType?.ToString(),
            Title = n.Title,
            Message = n.Message,
            IsRead = n.IsRead,
            CreatedAt = n.CreatedAt
        });
    }

    public async Task MarkAsReadAsync(Guid notificationId, Guid userId)
    {
        var notification = await _repository.GetByIdAsync(notificationId);
        if (notification != null && notification.UserId == userId)
        {
            await _repository.MarkAsReadAsync(notificationId);
        }
    }

    public async Task<int> GetUnreadCountAsync(Guid userId)
    {
        return await _repository.GetUnreadCountAsync(userId);
    }
}