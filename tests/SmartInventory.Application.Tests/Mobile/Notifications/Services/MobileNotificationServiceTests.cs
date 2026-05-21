using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using SmartInventory.Application.Mobile.Notifications.DTOs;
using SmartInventory.Application.Mobile.Notifications.Interfaces;
using SmartInventory.Application.Mobile.Notifications.Services;
using SmartInventory.Application.Notification.DTOs;
using SmartInventory.Application.Notification.Interfaces;
using Xunit;

namespace SmartInventory.Application.Tests.Mobile.Notifications.Services;

public class MobileNotificationServiceTests
{
    private readonly Mock<INotificationService> _innerServiceMock;
    private readonly IMobileNotificationService _service;

    public MobileNotificationServiceTests()
    {
        _innerServiceMock = new Mock<INotificationService>();
        _service = new MobileNotificationService(_innerServiceMock.Object);
    }

    #region GetNotificationsAsync

    [Fact]
    public async Task GetNotificationsAsync_ReturnsMappedList()
    {
        var userId = Guid.NewGuid();
        var notificationDtos = new List<NotificationDto>
        {
            new()
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Type = "Info",
                Title = "Test Title",
                Message = "Test Message",
                IsRead = false,
                CreatedAt = new DateTime(2026, 5, 15, 10, 0, 0, DateTimeKind.Utc)
            }
        };

        _innerServiceMock
            .Setup(s => s.GetUserNotificationsAsync(userId, false))
            .ReturnsAsync(notificationDtos);

        var result = await _service.GetNotificationsAsync(userId, false, CancellationToken.None);

        result.Should().HaveCount(1);
        var item = result[0];
        item.Id.Should().Be(notificationDtos[0].Id);
        item.Type.Should().Be("Info");
        item.Title.Should().Be("Test Title");
        item.Message.Should().Be("Test Message");
        item.IsRead.Should().BeFalse();
        item.CreatedAt.Should().Be(notificationDtos[0].CreatedAt);
    }

    [Fact]
    public async Task GetNotificationsAsync_WithUnreadOnly_PassesFilterThrough()
    {
        var userId = Guid.NewGuid();

        _innerServiceMock
            .Setup(s => s.GetUserNotificationsAsync(userId, true))
            .ReturnsAsync(new List<NotificationDto>());

        await _service.GetNotificationsAsync(userId, true, CancellationToken.None);

        _innerServiceMock.Verify(s => s.GetUserNotificationsAsync(userId, true), Times.Once);
    }

    [Fact]
    public async Task GetNotificationsAsync_EmptyList_ReturnsEmpty()
    {
        var userId = Guid.NewGuid();

        _innerServiceMock
            .Setup(s => s.GetUserNotificationsAsync(userId, false))
            .ReturnsAsync(Enumerable.Empty<NotificationDto>());

        var result = await _service.GetNotificationsAsync(userId, false, CancellationToken.None);

        result.Should().BeEmpty();
    }

    #endregion

    #region GetUnreadCountAsync

    [Fact]
    public async Task GetUnreadCountAsync_ReturnsCount()
    {
        var userId = Guid.NewGuid();
        var expectedCount = 5;

        _innerServiceMock
            .Setup(s => s.GetUnreadCountAsync(userId))
            .ReturnsAsync(expectedCount);

        var result = await _service.GetUnreadCountAsync(userId, CancellationToken.None);

        result.Should().Be(expectedCount);
    }

    #endregion

    #region MarkAsReadAsync

    [Fact]
    public async Task MarkAsReadAsync_DelegatesToInnerService()
    {
        var notificationId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        await _service.MarkAsReadAsync(notificationId, userId, CancellationToken.None);

        _innerServiceMock.Verify(s => s.MarkAsReadAsync(notificationId, userId), Times.Once);
    }

    #endregion

    #region MarkAllAsReadAsync

    [Fact]
    public async Task MarkAllAsReadAsync_DelegatesAndReturnsCount()
    {
        var userId = Guid.NewGuid();
        var expectedCount = 3;

        _innerServiceMock
            .Setup(s => s.MarkAllAsReadAsync(userId))
            .ReturnsAsync(expectedCount);

        var result = await _service.MarkAllAsReadAsync(userId, CancellationToken.None);

        result.Should().Be(expectedCount);
    }

    [Fact]
    public async Task MarkAllAsReadAsync_NoUnread_ReturnsZero()
    {
        var userId = Guid.NewGuid();

        _innerServiceMock
            .Setup(s => s.MarkAllAsReadAsync(userId))
            .ReturnsAsync(0);

        var result = await _service.MarkAllAsReadAsync(userId, CancellationToken.None);

        result.Should().Be(0);
    }

    #endregion

    #region DeleteNotificationAsync

    [Fact]
    public async Task DeleteNotificationAsync_ValidDelete_ReturnsTrue()
    {
        var notificationId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        _innerServiceMock
            .Setup(s => s.DeleteNotificationAsync(notificationId, userId))
            .ReturnsAsync(true);

        var result = await _service.DeleteNotificationAsync(notificationId, userId, CancellationToken.None);

        result.Should().BeTrue();
    }

    [Fact]
    public async Task DeleteNotificationAsync_NotFound_ReturnsFalse()
    {
        var notificationId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        _innerServiceMock
            .Setup(s => s.DeleteNotificationAsync(notificationId, userId))
            .ReturnsAsync(false);

        var result = await _service.DeleteNotificationAsync(notificationId, userId, CancellationToken.None);

        result.Should().BeFalse();
    }

    #endregion

    #region DeleteAllNotificationsAsync

    [Fact]
    public async Task DeleteAllNotificationsAsync_ReturnsCount()
    {
        var userId = Guid.NewGuid();
        var expectedCount = 5;

        _innerServiceMock
            .Setup(s => s.DeleteAllNotificationsAsync(userId))
            .ReturnsAsync(expectedCount);

        var result = await _service.DeleteAllNotificationsAsync(userId, CancellationToken.None);

        result.Should().Be(expectedCount);
    }

    [Fact]
    public async Task DeleteAllNotificationsAsync_NoNotifications_ReturnsZero()
    {
        var userId = Guid.NewGuid();

        _innerServiceMock
            .Setup(s => s.DeleteAllNotificationsAsync(userId))
            .ReturnsAsync(0);

        var result = await _service.DeleteAllNotificationsAsync(userId, CancellationToken.None);

        result.Should().Be(0);
    }

    #endregion
}
