using FluentAssertions;
using Moq;
using SmartInventory.Application.Auth.Interfaces;
using SmartInventory.Application.Notification.DTOs;
using SmartInventory.Application.Notification.Interfaces;
using SmartInventory.Application.Notification.Services;
using SmartInventory.Application.UserPreferences.DTOs;
using SmartInventory.Application.UserPreferences.Interfaces;
using SmartInventory.Domain.Auth.Entities;
using SmartInventory.Domain.Auth.Enums;
using SmartInventory.Domain.Notification.Entities;
using SmartInventory.Domain.Notification.Enums;
using SmartInventory.Domain.UserPreferences.Enums;
using Xunit;
using NotificationEntity = SmartInventory.Domain.Notification.Entities.Notification;

namespace SmartInventory.Application.Tests;

public class NotificationServiceTests
{
    private readonly Mock<INotificationRepository> _repositoryMock;
    private readonly Mock<IUserPreferenceService> _preferencesMock;
    private readonly Mock<IEmailSender> _emailSenderMock;
    private readonly Mock<IAuthRepository> _authRepositoryMock;
    private readonly Mock<INotificationDispatcher> _dispatcherMock;
    private readonly NotificationService _service;

    public NotificationServiceTests()
    {
        _repositoryMock = new Mock<INotificationRepository>();
        _preferencesMock = new Mock<IUserPreferenceService>();
        _emailSenderMock = new Mock<IEmailSender>();
        _authRepositoryMock = new Mock<IAuthRepository>();
        _dispatcherMock = new Mock<INotificationDispatcher>();

        _service = new NotificationService(
            _repositoryMock.Object,
            _preferencesMock.Object,
            _emailSenderMock.Object,
            _authRepositoryMock.Object,
            _dispatcherMock.Object
        );
    }

    private void SetupUserWithRole(Guid userId, UserRole role)
    {
        _authRepositoryMock
            .Setup(a => a.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new User
            {
                Id = userId,
                Username = "testuser",
                Email = "test@example.com",
                Role = role,
                Status = AccountStatus.Active
            });
    }

    private void SetupPreference(Guid userId, UserRole role, string preferenceKey, bool optedIn)
    {
        var prefs = new Dictionary<string, string>
        {
            [preferenceKey] = optedIn ? "true" : "false"
        };

        _preferencesMock
            .Setup(p => p.GetPreferencesAsync(userId, role))
            .ReturnsAsync(new UserPreferencesResponse
            {
                Preferences = prefs,
                RoleDefaults = new Dictionary<string, string>()
            });
    }

    [Fact]
    public async Task CreateNotificationAsync_ValidInputWithTargetUserIds_CreatesNotification()
    {
        var userId = Guid.NewGuid();
        var assetId = Guid.NewGuid();

        SetupUserWithRole(userId, UserRole.Supervisor);
        SetupPreference(userId, UserRole.Supervisor, PreferenceKeys.EquipmentStatusCriticalIssue, true);

        NotificationEntity? capturedNotification = null;
        _repositoryMock
            .Setup(r => r.AddAsync(It.IsAny<NotificationEntity>()))
            .Callback<NotificationEntity>(n => capturedNotification = n)
            .ReturnsAsync((NotificationEntity n) => n);

        var dto = new CreateNotificationDto
        {
            EventType = NotificationEventType.EquipmentStatusCriticalIssue,
            Type = NotificationType.Critical,
            Title = "Critical Issue",
            Message = "Asset has a critical issue",
            AssetId = assetId,
            TargetUserIds = new List<Guid> { userId }
        };

        await _service.CreateNotificationAsync(dto);

        capturedNotification.Should().NotBeNull();
        capturedNotification!.UserId.Should().Be(userId);
        capturedNotification.AssetId.Should().Be(assetId);
        capturedNotification.Type.Should().Be(NotificationType.Critical);
        capturedNotification.EventType.Should().Be(NotificationEventType.EquipmentStatusCriticalIssue);
        capturedNotification.Title.Should().Be("Critical Issue");
        capturedNotification.Message.Should().Be("Asset has a critical issue");
        capturedNotification.IsRead.Should().BeFalse();
    }

    [Fact]
    public async Task CreateNotificationAsync_UserOptedOut_SkipsNotification()
    {
        var userId = Guid.NewGuid();

        SetupUserWithRole(userId, UserRole.Supervisor);
        SetupPreference(userId, UserRole.Supervisor, PreferenceKeys.EquipmentStatusMaintenance, false);

        var dto = new CreateNotificationDto
        {
            EventType = NotificationEventType.EquipmentStatusMaintenance,
            Type = NotificationType.Warning,
            Title = "Maintenance",
            Message = "Asset needs maintenance",
            TargetUserIds = new List<Guid> { userId }
        };

        await _service.CreateNotificationAsync(dto);

        _repositoryMock.Verify(r => r.AddAsync(It.IsAny<NotificationEntity>()), Times.Never);
    }

    [Fact]
    public async Task CreateNotificationAsync_WithNullAssetId_CreatesNotification()
    {
        var userId = Guid.NewGuid();

        SetupUserWithRole(userId, UserRole.Supervisor);
        SetupPreference(userId, UserRole.Supervisor, PreferenceKeys.ImportCompletedSuccess, true);

        NotificationEntity? capturedNotification = null;
        _repositoryMock
            .Setup(r => r.AddAsync(It.IsAny<NotificationEntity>()))
            .Callback<NotificationEntity>(n => capturedNotification = n)
            .ReturnsAsync((NotificationEntity n) => n);

        var dto = new CreateNotificationDto
        {
            EventType = NotificationEventType.ImportCompletedSuccess,
            Type = NotificationType.Info,
            Title = "Import Complete",
            Message = "Bulk import finished",
            TargetUserIds = new List<Guid> { userId }
        };

        await _service.CreateNotificationAsync(dto);

        capturedNotification.Should().NotBeNull();
        capturedNotification!.AssetId.Should().BeNull();
    }

    [Fact]
    public async Task CreateNotificationAsync_TargetByRole_ResolvesUsers()
    {
        var userId1 = Guid.NewGuid();
        var userId2 = Guid.NewGuid();
        var role = UserRole.Supervisor;

        _authRepositoryMock
            .Setup(a => a.GetUsersByRoleAsync(role, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<User>
            {
                new() { Id = userId1, Username = "u1", Email = "u1@test.com", Role = role, Status = AccountStatus.Active },
                new() { Id = userId2, Username = "u2", Email = "u2@test.com", Role = role, Status = AccountStatus.Active }
            });

        SetupUserWithRole(userId1, role);
        SetupUserWithRole(userId2, role);
        SetupPreference(userId1, role, PreferenceKeys.EquipmentStatusCriticalIssue, true);
        SetupPreference(userId2, role, PreferenceKeys.EquipmentStatusCriticalIssue, true);

        var dto = new CreateNotificationDto
        {
            EventType = NotificationEventType.EquipmentStatusCriticalIssue,
            Type = NotificationType.Critical,
            Title = "Critical",
            Message = "Critical issue",
            TargetRole = role
        };

        await _service.CreateNotificationAsync(dto);

        _repositoryMock.Verify(r => r.AddAsync(It.IsAny<NotificationEntity>()), Times.Exactly(2));
    }

    [Fact]
    public async Task CreateNotificationAsync_CriticalEvent_SendsEmail()
    {
        var userId = Guid.NewGuid();

        SetupUserWithRole(userId, UserRole.Supervisor);
        SetupPreference(userId, UserRole.Supervisor, PreferenceKeys.EquipmentStatusCriticalIssue, true);

        _repositoryMock
            .Setup(r => r.AddAsync(It.IsAny<NotificationEntity>()))
            .ReturnsAsync((NotificationEntity n) => n);

        var dto = new CreateNotificationDto
        {
            EventType = NotificationEventType.EquipmentStatusCriticalIssue,
            Type = NotificationType.Critical,
            Title = "Critical Issue",
            Message = "Asset has a critical issue",
            TargetUserIds = new List<Guid> { userId }
        };

        await _service.CreateNotificationAsync(dto);

        _emailSenderMock.Verify(
            s => s.SendEmailAsync(
                It.IsAny<string>(),
                It.Is<string>(subject => subject.Contains("CRITICAL")),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task CreateNotificationAsync_NonCriticalEvent_DoesNotSendEmail()
    {
        var userId = Guid.NewGuid();

        SetupUserWithRole(userId, UserRole.Supervisor);
        SetupPreference(userId, UserRole.Supervisor, PreferenceKeys.EquipmentStatusInStock, true);

        _repositoryMock
            .Setup(r => r.AddAsync(It.IsAny<NotificationEntity>()))
            .ReturnsAsync((NotificationEntity n) => n);

        var dto = new CreateNotificationDto
        {
            EventType = NotificationEventType.EquipmentStatusInStock,
            Type = NotificationType.Info,
            Title = "In Stock",
            Message = "Asset is now in stock",
            TargetUserIds = new List<Guid> { userId }
        };

        await _service.CreateNotificationAsync(dto);

        _emailSenderMock.Verify(
            s => s.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task CreateNotificationAsync_BroadcastsViaDispatcher()
    {
        var userId = Guid.NewGuid();

        SetupUserWithRole(userId, UserRole.Supervisor);
        SetupPreference(userId, UserRole.Supervisor, PreferenceKeys.EquipmentStatusCriticalIssue, true);

        _repositoryMock
            .Setup(r => r.AddAsync(It.IsAny<NotificationEntity>()))
            .ReturnsAsync((NotificationEntity n) => n);

        var dto = new CreateNotificationDto
        {
            EventType = NotificationEventType.EquipmentStatusCriticalIssue,
            Type = NotificationType.Critical,
            Title = "Critical",
            Message = "Critical issue",
            TargetUserIds = new List<Guid> { userId }
        };

        await _service.CreateNotificationAsync(dto);

        _dispatcherMock.Verify(
            d => d.DispatchAsync(
                userId,
                It.Is<NotificationDto>(n => n.Title == "Critical"),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetUserNotificationsAsync_ReturnsNotificationDtos()
    {
        var userId = Guid.NewGuid();
        var notifications = new List<NotificationEntity>
        {
            new()
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

        var result = await _service.GetUserNotificationsAsync(userId);

        result.Should().NotBeNull();
        result.Should().HaveCount(1);
    }

    [Fact]
    public async Task GetUserNotificationsAsync_UnreadOnly_GetsUnreadNotifications()
    {
        var userId = Guid.NewGuid();

        _repositoryMock.Setup(r => r.GetByUserIdAsync(userId, true))
            .ReturnsAsync(new List<NotificationEntity>());

        var result = await _service.GetUserNotificationsAsync(userId, true);

        result.Should().BeEmpty();
        _repositoryMock.Verify(r => r.GetByUserIdAsync(userId, true), Times.Once);
    }

    [Fact]
    public async Task MarkAsReadAsync_ValidNotification_CallsRepository()
    {
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

        await _service.MarkAsReadAsync(notificationId, userId);

        _repositoryMock.Verify(r => r.MarkAsReadAsync(notificationId), Times.Once);
    }

    [Fact]
    public async Task MarkAsReadAsync_WrongUser_DoesNotCallRepository()
    {
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

        await _service.MarkAsReadAsync(notificationId, differentUserId);

        _repositoryMock.Verify(r => r.MarkAsReadAsync(It.IsAny<Guid>()), Times.Never);
    }

    [Fact]
    public async Task MarkAsReadAsync_NotificationNotFound_DoesNotThrow()
    {
        var notificationId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        _repositoryMock.Setup(r => r.GetByIdAsync(notificationId))
            .ReturnsAsync((NotificationEntity?)null);

        var act = () => _service.MarkAsReadAsync(notificationId, userId);

        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task GetUnreadCountAsync_ReturnsCount()
    {
        var userId = Guid.NewGuid();
        var expectedCount = 5;

        _repositoryMock.Setup(r => r.GetUnreadCountAsync(userId))
            .ReturnsAsync(expectedCount);

        var result = await _service.GetUnreadCountAsync(userId);

        result.Should().Be(expectedCount);
    }

    [Fact]
    public async Task MarkAllAsReadAsync_UpdatesUnreadNotifications_ReturnsCount()
    {
        var userId = Guid.NewGuid();
        var expectedCount = 3;

        _repositoryMock.Setup(r => r.MarkAllAsReadAsync(userId))
            .ReturnsAsync(expectedCount);

        var result = await _service.MarkAllAsReadAsync(userId);

        result.Should().Be(expectedCount);
    }

    [Fact]
    public async Task DeleteNotificationAsync_ValidNotification_ReturnsTrue()
    {
        var notificationId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var notification = new NotificationEntity
        {
            Id = notificationId,
            UserId = userId,
            Title = "Test"
        };

        _repositoryMock.Setup(r => r.GetByIdAsync(notificationId))
            .ReturnsAsync(notification);
        _repositoryMock.Setup(r => r.DeleteNotificationAsync(notificationId, userId))
            .ReturnsAsync(true);

        var result = await _service.DeleteNotificationAsync(notificationId, userId);

        result.Should().BeTrue();
    }

    [Fact]
    public async Task DeleteNotificationAsync_WrongUser_ReturnsFalse()
    {
        var notificationId = Guid.NewGuid();
        var ownerUserId = Guid.NewGuid();
        var differentUserId = Guid.NewGuid();
        var notification = new NotificationEntity
        {
            Id = notificationId,
            UserId = ownerUserId,
            Title = "Test"
        };

        _repositoryMock.Setup(r => r.GetByIdAsync(notificationId))
            .ReturnsAsync(notification);

        var result = await _service.DeleteNotificationAsync(notificationId, differentUserId);

        result.Should().BeFalse();
        _repositoryMock.Verify(r => r.DeleteNotificationAsync(It.IsAny<Guid>(), It.IsAny<Guid>()), Times.Never);
    }

    [Fact]
    public async Task DeleteNotificationAsync_NotFound_ReturnsFalse()
    {
        var notificationId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        _repositoryMock.Setup(r => r.GetByIdAsync(notificationId))
            .ReturnsAsync((NotificationEntity?)null);

        var result = await _service.DeleteNotificationAsync(notificationId, userId);

        result.Should().BeFalse();
    }

    [Fact]
    public async Task DeleteAllNotificationsAsync_ReturnsCount()
    {
        var userId = Guid.NewGuid();
        var expectedCount = 5;

        _repositoryMock.Setup(r => r.DeleteAllNotificationsAsync(userId))
            .ReturnsAsync(expectedCount);

        var result = await _service.DeleteAllNotificationsAsync(userId);

        result.Should().Be(expectedCount);
    }
}
