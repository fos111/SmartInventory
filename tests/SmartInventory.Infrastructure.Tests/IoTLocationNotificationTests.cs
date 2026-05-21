using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using SmartInventory.Application.Notification.DTOs;
using SmartInventory.Application.Notification.Interfaces;
using SmartInventory.Domain.Location.Entities;
using SmartInventory.Domain.Notification.Enums;
using SmartInventory.Infrastructure.Data;
using SmartInventory.IoTLocation.Contracts;
using SmartInventory.IoTLocation.Services;
using Xunit;
using AssetEntity = SmartInventory.Domain.Asset.Entities.Asset;

namespace SmartInventory.Infrastructure.Tests;

public class IoTLocationNotificationTests
{
    private readonly Mock<INotificationService> _notificationServiceMock;
    private readonly Mock<ILogger<IoTLocationService>> _loggerMock;
    private readonly ApplicationDbContext _dbContext;
    private readonly IoTLocationService _service;
    private readonly Guid _assetId;
    private const string RoomCodeLi1 = "LI1";
    private const string RoomCodeLi2 = "LI2";

    public IoTLocationNotificationTests()
    {
        _notificationServiceMock = new Mock<INotificationService>();
        _loggerMock = new Mock<ILogger<IoTLocationService>>();

        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase($"IoTLocationNotificationTests_{Guid.NewGuid()}")
            .Options;
        _dbContext = new ApplicationDbContext(options);

        _dbContext.Rooms.Add(new Room { Code = RoomCodeLi1, Name = "Room LI1" });
        _dbContext.Rooms.Add(new Room { Code = RoomCodeLi2, Name = "Room LI2" });

        _assetId = Guid.NewGuid();
        _dbContext.Assets.Add(new AssetEntity
        {
            Id = _assetId,
            AssetTag = "AST-IOT-001",
            Name = "IoT Test Asset",
            Category = "Sensor",
            CurrentRoomCode = RoomCodeLi1,
            Status = Domain.Asset.Enums.AssetStatus.Active
        });

        _dbContext.SaveChanges();

        _service = new IoTLocationService(
            _dbContext,
            _notificationServiceMock.Object,
            _loggerMock.Object,
            cacheService: null);
    }

    private string BuildPayload(string roomCode) =>
        System.Text.Json.JsonSerializer.Serialize(new IoTLocationMessage
        {
            AssetId = _assetId,
            RoomCode = roomCode,
            Timestamp = DateTime.UtcNow
        });

    [Fact]
    public async Task ProcessLocationAsync_LocationMismatch_SendsNotification()
    {
        var payload = BuildPayload(RoomCodeLi2);

        CreateNotificationDto? capturedDto = null;
        _notificationServiceMock
            .Setup(n => n.CreateNotificationAsync(It.IsAny<CreateNotificationDto>(), It.IsAny<CancellationToken>()))
            .Callback<CreateNotificationDto, CancellationToken>((dto, _) => capturedDto = dto)
            .Returns(Task.CompletedTask);

        var result = await _service.ProcessLocationAsync(payload);

        result.Success.Should().BeTrue();

        _notificationServiceMock.Verify(
            n => n.CreateNotificationAsync(It.IsAny<CreateNotificationDto>(), It.IsAny<CancellationToken>()),
            Times.Once);

        capturedDto.Should().NotBeNull();
        capturedDto!.EventType.Should().Be(NotificationEventType.LocationMismatch);
        capturedDto.Type.Should().Be(NotificationType.Warning);
        capturedDto.Title.Should().Be("Location Mismatch Detected");
        capturedDto.AssetId.Should().Be(_assetId);
        capturedDto.TargetRole.Should().Be(SmartInventory.Domain.Auth.Enums.UserRole.Supervisor);
        capturedDto.Message.Should().Contain(RoomCodeLi2);
        capturedDto.Message.Should().Contain(RoomCodeLi1);
    }

    [Fact]
    public async Task ProcessLocationAsync_LocationMatch_DoesNotSendNotification()
    {
        var payload = BuildPayload(RoomCodeLi1);

        var result = await _service.ProcessLocationAsync(payload);

        result.Success.Should().BeTrue();

        _notificationServiceMock.Verify(
            n => n.CreateNotificationAsync(It.IsAny<CreateNotificationDto>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task ProcessLocationAsync_FirstDetectionWithMismatch_SendsNotification()
    {
        var assetId = Guid.NewGuid();
        _dbContext.Assets.Add(new AssetEntity
        {
            Id = assetId,
            AssetTag = "AST-IOT-002",
            Name = "First Detection Asset",
            Category = "Sensor",
            CurrentRoomCode = RoomCodeLi1,
            DetectedRoomCode = null,
            Status = Domain.Asset.Enums.AssetStatus.Active
        });
        _dbContext.SaveChanges();

        var payload = System.Text.Json.JsonSerializer.Serialize(new IoTLocationMessage
        {
            AssetId = assetId,
            RoomCode = RoomCodeLi2,
            Timestamp = DateTime.UtcNow
        });

        CreateNotificationDto? capturedDto = null;
        _notificationServiceMock
            .Setup(n => n.CreateNotificationAsync(It.IsAny<CreateNotificationDto>(), It.IsAny<CancellationToken>()))
            .Callback<CreateNotificationDto, CancellationToken>((dto, _) => capturedDto = dto)
            .Returns(Task.CompletedTask);

        var result = await _service.ProcessLocationAsync(payload);

        result.Success.Should().BeTrue();
        capturedDto.Should().NotBeNull();
        capturedDto!.EventType.Should().Be(NotificationEventType.LocationMismatch);
    }

    [Fact]
    public async Task ProcessLocationAsync_NotificationFailure_DoesNotThrow()
    {
        var payload = BuildPayload(RoomCodeLi2);

        _notificationServiceMock
            .Setup(n => n.CreateNotificationAsync(It.IsAny<CreateNotificationDto>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Notification service down"));

        var result = await _service.ProcessLocationAsync(payload);

        result.Success.Should().BeTrue();
    }
}
