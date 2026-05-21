using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SmartInventory.Application.Auth.Interfaces;
using SmartInventory.Application.Notification.DTOs;
using SmartInventory.Application.Notification.Interfaces;
using SmartInventory.Application.UserPreferences.Interfaces;
using SmartInventory.Domain.Notification.Entities;
using SmartInventory.Domain.Notification.Enums;
using SmartInventory.Domain.Notification.Mappings;
using NotificationEntity = SmartInventory.Domain.Notification.Entities.Notification;

namespace SmartInventory.Application.Notification.Services;

public class NotificationService : INotificationService
{
    private readonly INotificationRepository _repository;
    private readonly IUserPreferenceService _userPreferenceService;
    private readonly IEmailSender _emailSender;
    private readonly IAuthRepository _authRepository;
    private readonly INotificationDispatcher _dispatcher;

    public NotificationService(
        INotificationRepository repository,
        IUserPreferenceService userPreferenceService,
        IEmailSender emailSender,
        IAuthRepository authRepository,
        INotificationDispatcher dispatcher)
    {
        _repository = repository;
        _userPreferenceService = userPreferenceService;
        _emailSender = emailSender;
        _authRepository = authRepository;
        _dispatcher = dispatcher;
    }

    public async Task CreateNotificationAsync(CreateNotificationDto dto, CancellationToken ct = default)
    {
        var targetUserIds = await ResolveTargetUserIdsAsync(dto, ct);

        foreach (var userId in targetUserIds)
        {
            if (!await IsUserOptedInAsync(userId, dto.EventType, ct))
                continue;

            var notification = new NotificationEntity
            {
                UserId = userId,
                AssetId = dto.AssetId,
                Type = dto.Type,
                EventType = dto.EventType,
                Title = dto.Title,
                Message = dto.Message,
                IsRead = false,
                CreatedAt = DateTime.UtcNow
            };

            await _repository.AddAsync(notification);

            await BroadcastViaSignalRAsync(userId, notification, ct);

            if (dto.Type == NotificationType.Critical)
            {
                await DispatchEmailAsync(userId, dto.Title, dto.Message, ct);
            }
        }
    }

    private async Task<List<Guid>> ResolveTargetUserIdsAsync(CreateNotificationDto dto, CancellationToken ct)
    {
        if (dto.TargetUserIds is { Count: > 0 })
            return dto.TargetUserIds;

        if (dto.TargetRole.HasValue)
        {
            var users = await _authRepository.GetUsersByRoleAsync(dto.TargetRole.Value, ct);
            return users.Select(u => u.Id).ToList();
        }

        return new List<Guid>();
    }

    private async Task<bool> IsUserOptedInAsync(Guid userId, NotificationEventType eventType, CancellationToken ct)
    {
        var preferenceKey = eventType.ToPreferenceKey();

        var user = await _authRepository.GetByIdAsync(userId, ct);
        if (user == null || !user.Role.HasValue)
            return false;

        var prefs = await _userPreferenceService.GetPreferencesAsync(userId, user.Role.Value);

        return prefs.Preferences.TryGetValue(preferenceKey, out var value)
            && value.Equals("true", StringComparison.OrdinalIgnoreCase);
    }

    private async Task BroadcastViaSignalRAsync(Guid userId, NotificationEntity notification, CancellationToken ct)
    {
        var dto = new NotificationDto
        {
            Id = notification.Id,
            UserId = notification.UserId,
            AssetId = notification.AssetId,
            Type = notification.Type.ToString(),
            EventType = notification.EventType?.ToString(),
            Title = notification.Title,
            Message = notification.Message,
            IsRead = notification.IsRead,
            CreatedAt = notification.CreatedAt
        };

        await _dispatcher.DispatchAsync(userId, dto, ct);
    }

    private async Task DispatchEmailAsync(Guid userId, string title, string message, CancellationToken ct)
    {
        var user = await _authRepository.GetByIdAsync(userId, ct);
        if (user == null || string.IsNullOrWhiteSpace(user.Email))
            return;

        var htmlBody = $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .critical {{ border-left: 4px solid #dc3545; padding: 12px; background: #fef2f2; }}
        .footer {{ color: #666; font-size: 12px; margin-top: 30px; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='critical'>
            <h2>Critical Alert</h2>
            <p><strong>{title}</strong></p>
            <p>{message}</p>
        </div>
        <div class='footer'>
            <p>SmartInventory Notification System</p>
        </div>
    </div>
</body>
</html>";

        await _emailSender.SendEmailAsync(user.Email, $"[CRITICAL] {title}", htmlBody, ct);
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

    public async Task<int> MarkAllAsReadAsync(Guid userId)
    {
        return await _repository.MarkAllAsReadAsync(userId);
    }

    public async Task<bool> DeleteNotificationAsync(Guid id, Guid userId)
    {
        var notification = await _repository.GetByIdAsync(id);
        if (notification == null || notification.UserId != userId)
            return false;

        return await _repository.DeleteNotificationAsync(id, userId);
    }

    public async Task<int> DeleteAllNotificationsAsync(Guid userId)
    {
        return await _repository.DeleteAllNotificationsAsync(userId);
    }
}
