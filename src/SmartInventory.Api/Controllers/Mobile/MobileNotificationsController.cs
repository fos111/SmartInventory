using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartInventory.Api.Models;
using SmartInventory.Application.Mobile.Notifications.DTOs;
using SmartInventory.Application.Mobile.Notifications.Interfaces;

namespace SmartInventory.Api.Controllers.Mobile;

[ApiController]
[Route("api/mobile/notifications")]
[Authorize]
public class MobileNotificationsController : ControllerBase
{
    private readonly IMobileNotificationService _notificationService;

    public MobileNotificationsController(IMobileNotificationService notificationService)
    {
        _notificationService = notificationService;
    }

    [HttpGet]
    public async Task<ActionResult<MobileEnvelope<List<MobileNotificationListItemDto>>>> GetNotifications(
        [FromQuery] bool unreadOnly = false,
        CancellationToken ct = default)
    {
        var userId = GetUserId();
        var notifications = await _notificationService.GetNotificationsAsync(userId, unreadOnly, ct);
        return Ok(MobileEnvelope<List<MobileNotificationListItemDto>>.SuccessResult(notifications));
    }

    [HttpGet("unread-count")]
    public async Task<ActionResult<MobileEnvelope<int>>> GetUnreadCount(CancellationToken ct = default)
    {
        var userId = GetUserId();
        var count = await _notificationService.GetUnreadCountAsync(userId, ct);
        return Ok(MobileEnvelope<int>.SuccessResult(count));
    }

    [HttpPut("{id:guid}/read")]
    public async Task<ActionResult<MobileEnvelope>> MarkAsRead(Guid id, CancellationToken ct = default)
    {
        var userId = GetUserId();
        await _notificationService.MarkAsReadAsync(id, userId, ct);
        return Ok(MobileEnvelope.SuccessResult("Notification marked as read"));
    }

    [HttpPut("read-all")]
    public async Task<ActionResult<MobileEnvelope>> MarkAllAsRead(CancellationToken ct = default)
    {
        var userId = GetUserId();
        var count = await _notificationService.MarkAllAsReadAsync(userId, ct);
        return Ok(MobileEnvelope.SuccessResult($"{count} notifications marked as read"));
    }

    [HttpDelete("{id:guid}")]
    public async Task<ActionResult<MobileEnvelope>> DeleteNotification(Guid id, CancellationToken ct = default)
    {
        var userId = GetUserId();
        var deleted = await _notificationService.DeleteNotificationAsync(id, userId, ct);
        if (!deleted)
            return Ok(MobileEnvelope.FailureResult($"Notification with ID '{id}' not found."));
        return Ok(MobileEnvelope.SuccessResult("Notification deleted"));
    }

    [HttpDelete]
    public async Task<ActionResult<MobileEnvelope>> DeleteAllNotifications(CancellationToken ct = default)
    {
        var userId = GetUserId();
        var count = await _notificationService.DeleteAllNotificationsAsync(userId, ct);
        return Ok(MobileEnvelope.SuccessResult($"{count} notifications deleted"));
    }

    private Guid GetUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(userIdClaim, out var userId) ? userId : Guid.Empty;
    }
}
