using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using SmartInventory.Api.Controllers.Mobile;
using SmartInventory.Api.Models;
using SmartInventory.Application.Mobile.Notifications.DTOs;
using SmartInventory.Application.Mobile.Notifications.Interfaces;
using Xunit;

namespace SmartInventory.Api.Tests.Mobile;

public class MobileNotificationsControllerTests
{
    private readonly Mock<IMobileNotificationService> _serviceMock;
    private readonly MobileNotificationsController _controller;
    private readonly Guid _defaultUserId = Guid.NewGuid();

    public MobileNotificationsControllerTests()
    {
        _serviceMock = new Mock<IMobileNotificationService>();
        _controller = new MobileNotificationsController(_serviceMock.Object);

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, _defaultUserId.ToString())
        };
        var identity = new ClaimsIdentity(claims, "Test");
        var principal = new ClaimsPrincipal(identity);
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = principal }
        };
    }

    [Fact]
    public async Task GetNotifications_ReturnsOkWithEnvelope()
    {
        var items = new List<MobileNotificationListItemDto>
        {
            new()
            {
                Id = Guid.NewGuid(),
                Type = "Info",
                Title = "Test",
                Message = "Message",
                IsRead = false,
                CreatedAt = DateTime.UtcNow
            }
        };

        _serviceMock
            .Setup(s => s.GetNotificationsAsync(_defaultUserId, false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(items);

        var result = await _controller.GetNotifications(false, CancellationToken.None);

        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var envelope = okResult.Value.Should().BeOfType<MobileEnvelope<List<MobileNotificationListItemDto>>>().Subject;
        envelope.Success.Should().BeTrue();
        envelope.Data.Should().HaveCount(1);
        envelope.Data![0].Title.Should().Be("Test");
    }

    [Fact]
    public async Task GetNotifications_WithUnreadOnly_PassesFilterThrough()
    {
        _serviceMock
            .Setup(s => s.GetNotificationsAsync(_defaultUserId, true, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<MobileNotificationListItemDto>());

        var result = await _controller.GetNotifications(true, CancellationToken.None);

        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var envelope = okResult.Value.Should().BeOfType<MobileEnvelope<List<MobileNotificationListItemDto>>>().Subject;
        envelope.Success.Should().BeTrue();

        _serviceMock.Verify(s => s.GetNotificationsAsync(_defaultUserId, true, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetUnreadCount_ReturnsOkWithEnvelope()
    {
        var expectedCount = 5;

        _serviceMock
            .Setup(s => s.GetUnreadCountAsync(_defaultUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedCount);

        var result = await _controller.GetUnreadCount(CancellationToken.None);

        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var envelope = okResult.Value.Should().BeOfType<MobileEnvelope<int>>().Subject;
        envelope.Success.Should().BeTrue();
        envelope.Data.Should().Be(expectedCount);
    }

    [Fact]
    public async Task MarkAsRead_ReturnsOkWithSuccessEnvelope()
    {
        var notificationId = Guid.NewGuid();

        _serviceMock
            .Setup(s => s.MarkAsReadAsync(notificationId, _defaultUserId, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var result = await _controller.MarkAsRead(notificationId, CancellationToken.None);

        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var envelope = okResult.Value.Should().BeOfType<MobileEnvelope>().Subject;
        envelope.Success.Should().BeTrue();
        envelope.Message.Should().Be("Notification marked as read");
    }

    [Fact]
    public async Task MarkAllAsRead_ReturnsOkWithSuccessEnvelope()
    {
        var count = 3;

        _serviceMock
            .Setup(s => s.MarkAllAsReadAsync(_defaultUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(count);

        var result = await _controller.MarkAllAsRead(CancellationToken.None);

        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var envelope = okResult.Value.Should().BeOfType<MobileEnvelope>().Subject;
        envelope.Success.Should().BeTrue();
        envelope.Message.Should().Contain($"{count}");
    }

    [Fact]
    public async Task DeleteNotification_Valid_ReturnsOkWithSuccessEnvelope()
    {
        var notificationId = Guid.NewGuid();

        _serviceMock
            .Setup(s => s.DeleteNotificationAsync(notificationId, _defaultUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var result = await _controller.DeleteNotification(notificationId, CancellationToken.None);

        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var envelope = okResult.Value.Should().BeOfType<MobileEnvelope>().Subject;
        envelope.Success.Should().BeTrue();
        envelope.Message.Should().Be("Notification deleted");
    }

    [Fact]
    public async Task DeleteNotification_NotFound_ReturnsOkWithFailureEnvelope()
    {
        var notificationId = Guid.NewGuid();

        _serviceMock
            .Setup(s => s.DeleteNotificationAsync(notificationId, _defaultUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var result = await _controller.DeleteNotification(notificationId, CancellationToken.None);

        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var envelope = okResult.Value.Should().BeOfType<MobileEnvelope>().Subject;
        envelope.Success.Should().BeFalse();
        envelope.Message.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task DeleteAllNotifications_ReturnsOkWithSuccessEnvelope()
    {
        var count = 5;

        _serviceMock
            .Setup(s => s.DeleteAllNotificationsAsync(_defaultUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(count);

        var result = await _controller.DeleteAllNotifications(CancellationToken.None);

        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var envelope = okResult.Value.Should().BeOfType<MobileEnvelope>().Subject;
        envelope.Success.Should().BeTrue();
        envelope.Message.Should().Contain($"{count}");
    }

    [Fact]
    public async Task GetNotifications_UsesUserIdFromClaim()
    {
        var customUserId = Guid.NewGuid();

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, customUserId.ToString())
        };
        var identity = new ClaimsIdentity(claims, "Test");
        var principal = new ClaimsPrincipal(identity);
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = principal }
        };

        _serviceMock
            .Setup(s => s.GetNotificationsAsync(customUserId, false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<MobileNotificationListItemDto>());

        await _controller.GetNotifications(false, CancellationToken.None);

        _serviceMock.Verify(s => s.GetNotificationsAsync(customUserId, false, It.IsAny<CancellationToken>()), Times.Once);
    }
}
