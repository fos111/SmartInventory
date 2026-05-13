using FluentAssertions;
using Moq;
using SmartInventory.Application.Notification.DTOs;
using SmartInventory.Application.Notification.Interfaces;
using SmartInventory.Application.Notification.Services;
using SmartInventory.Domain.Notification.Entities;
using SmartInventory.Domain.Notification.Enums;
using Xunit;
using NotificationEntity = SmartInventory.Domain.Notification.Entities.Notification;

namespace SmartInventory.Application.Tests;

public class NotificationServiceTests : ApplicationTestBase
{
    private readonly Mock<INotificationRepository> _repositoryMock;
    private readonly Mock<AutoMapper.IMapper> _mapperMock;
    private readonly NotificationService _service;

    public NotificationServiceTests()
    {
        _repositoryMock = new Mock<INotificationRepository>();
        _mapperMock = new Mock<AutoMapper.IMapper>();
        _service = new NotificationService(_repositoryMock.Object, _mapperMock.Object);
    }

    [Fact]
    public async Task CreateNotificationAsync_ValidInput_CallsRepository()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var assetId = Guid.NewGuid();
        var type = NotificationType.Critical;
        var title = "Asset Retired";
        var message = "An asset has been retired";

        NotificationEntity? capturedNotification = null;
        _repositoryMock.Setup(r => r.AddAsync(It.IsAny<NotificationEntity>()))
            .Callback<NotificationEntity>(n => capturedNotification = n)
            .ReturnsAsync((NotificationEntity n) => n);

        // Act
        await _service.CreateNotificationAsync(userId, assetId, type, title, message);

        // Assert
        capturedNotification.Should().NotBeNull();
        capturedNotification!.UserId.Should().Be(userId);
        capturedNotification.AssetId.Should().Be(assetId);
        capturedNotification.Type.Should().Be(type);
        capturedNotification.Title.Should().Be(title);
        capturedNotification.Message.Should().Be(message);
        capturedNotification.IsRead.Should().BeFalse();
    }

    [Fact]
    public async Task CreateNotificationAsync_WithNullAssetId_CreatesNotification()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var type = NotificationType.Warning;
        var title = "Status Changed";
        var message = "Status updated";

        NotificationEntity? capturedNotification = null;
        _repositoryMock.Setup(r => r.AddAsync(It.IsAny<NotificationEntity>()))
            .Callback<NotificationEntity>(n => capturedNotification = n)
            .ReturnsAsync((NotificationEntity n) => n);

        // Act
        await _service.CreateNotificationAsync(userId, null, type, title, message);

        // Assert
        capturedNotification.Should().NotBeNull();
        capturedNotification!.AssetId.Should().BeNull();
    }

    [Fact]
    public async Task GetUserNotificationsAsync_ReturnsNotificationDtos()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var notifications = new List<NotificationEntity>
        {
            new NotificationEntity
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Type = NotificationType.Critical,
                Title = "Critical Issue",
                Message = "Asset needs attention",
                IsRead = false,
                CreatedAt = DateTime.UtcNow
            }
        };

        _repositoryMock.Setup(r => r.GetByUserIdAsync(userId, false))
            .ReturnsAsync(notifications);

        _mapperMock.Setup(m => m.Map<NotificationDto>(It.IsAny<NotificationEntity>()))
            .Returns(new NotificationDto
            {
                Id = notifications[0].Id,
                UserId = notifications[0].UserId,
                Type = notifications[0].Type.ToString(),
                Title = notifications[0].Title,
                Message = notifications[0].Message,
                IsRead = notifications[0].IsRead,
                CreatedAt = notifications[0].CreatedAt
            });

        // Act
        var result = await _service.GetUserNotificationsAsync(userId);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(1);
    }

    [Fact]
    public async Task GetUserNotificationsAsync_UnreadOnly_GetsUnreadNotifications()
    {
        // Arrange
        var userId = Guid.NewGuid();

        _repositoryMock.Setup(r => r.GetByUserIdAsync(userId, true))
            .ReturnsAsync(new List<NotificationEntity>());

        // Act
        var result = await _service.GetUserNotificationsAsync(userId, true);

        // Assert
        result.Should().BeEmpty();
        _repositoryMock.Verify(r => r.GetByUserIdAsync(userId, true), Times.Once);
    }

    [Fact]
    public async Task MarkAsReadAsync_ValidNotification_CallsRepository()
    {
        // Arrange
        var notificationId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var notification = new NotificationEntity
        {
            Id = notificationId,
            UserId = userId,
            Title = "Test",
            IsRead = false
        };

        _repositoryMock.Setup(r => r.GetByIdAsync(notificationId))
            .ReturnsAsync(notification);

        // Act
        await _service.MarkAsReadAsync(notificationId, userId);

        // Assert
        _repositoryMock.Verify(r => r.MarkAsReadAsync(notificationId), Times.Once);
    }

    [Fact]
    public async Task MarkAsReadAsync_WrongUser_DoesNotCallRepository()
    {
        // Arrange
        var notificationId = Guid.NewGuid();
        var ownerUserId = Guid.NewGuid();
        var differentUserId = Guid.NewGuid();
        var notification = new NotificationEntity
        {
            Id = notificationId,
            UserId = ownerUserId,
            Title = "Test",
            IsRead = false
        };

        _repositoryMock.Setup(r => r.GetByIdAsync(notificationId))
            .ReturnsAsync(notification);

        // Act
        await _service.MarkAsReadAsync(notificationId, differentUserId);

        // Assert
        _repositoryMock.Verify(r => r.MarkAsReadAsync(It.IsAny<Guid>()), Times.Never);
    }

    [Fact]
    public async Task MarkAsReadAsync_NotificationNotFound_DoesNotThrow()
    {
        // Arrange
        var notificationId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        _repositoryMock.Setup(r => r.GetByIdAsync(notificationId))
            .ReturnsAsync((NotificationEntity?)null);

        // Act
        var act = () => _service.MarkAsReadAsync(notificationId, userId);

        // Assert
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task GetUnreadCountAsync_ReturnsCount()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var expectedCount = 5;

        _repositoryMock.Setup(r => r.GetUnreadCountAsync(userId))
            .ReturnsAsync(expectedCount);

        // Act
        var result = await _service.GetUnreadCountAsync(userId);

        // Assert
        result.Should().Be(expectedCount);
    }
}