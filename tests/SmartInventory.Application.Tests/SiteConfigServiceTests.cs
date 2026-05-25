using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using FluentAssertions;
using Moq;
using SmartInventory.Application.Asset.Interfaces;
using SmartInventory.Application.Location.DTOs;
using SmartInventory.Application.Location.Interfaces;
using SmartInventory.Application.Location.Services;
using SmartInventory.Application.Notification.DTOs;
using SmartInventory.Application.Notification.Interfaces;
using SmartInventory.Domain.Auth.Enums;
using SmartInventory.Domain.Location.Entities;
using SmartInventory.Domain.Notification.Enums;
using Xunit;

namespace SmartInventory.Application.Tests;

public class SiteConfigServiceTests
{
    private readonly Mock<ILocationRepository> _repositoryMock;
    private readonly Mock<IActivityLogService> _activityLogServiceMock;
    private readonly Mock<INotificationService> _notificationServiceMock;
    private readonly IMapper _mapper;

    public SiteConfigServiceTests()
    {
        _repositoryMock = new Mock<ILocationRepository>();
        _activityLogServiceMock = new Mock<IActivityLogService>();
        _notificationServiceMock = new Mock<INotificationService>();

        var config = new MapperConfiguration(cfg =>
            cfg.AddProfile<SmartInventory.Application.Location.Mappings.LocationMappingProfile>());
        _mapper = config.CreateMapper();
    }

    private LocationService CreateService() => new LocationService(
        _repositoryMock.Object,
        _activityLogServiceMock.Object,
        _notificationServiceMock.Object,
        _mapper);

    private static ZoneSiteShape CreateTestShape(Guid? zoneId = null, string color = "#dbeafe")
    {
        var id = Guid.NewGuid();
        return new ZoneSiteShape
        {
            Id = id,
            ZoneId = zoneId ?? Guid.NewGuid(),
            Points = "[{\"x\":100,\"y\":50},{\"x\":400,\"y\":50},{\"x\":400,\"y\":250},{\"x\":100,\"y\":250}]",
            Color = color,
            Label = "Test Zone",
            CreatedAt = DateTime.UtcNow.AddDays(-1)
        };
    }

    private static Site CreateTestSite(string? satelliteUrl = null)
    {
        return new Site
        {
            Id = Guid.NewGuid(),
            Code = "ISETMA",
            Name = "ISETMA Mahdia",
            SatelliteImageUrl = satelliteUrl
        };
    }

    // --- GetSiteConfigAsync ---

    [Fact]
    public async Task GetSiteConfigAsync_WithData_ReturnsConfig()
    {
        var site = CreateTestSite("https://example.com/sat.jpg");
        var shape = CreateTestShape();
        _repositoryMock.Setup(r => r.GetSiteAsync()).ReturnsAsync(site);
        _repositoryMock.Setup(r => r.GetZoneSiteShapesAsync()).ReturnsAsync(new List<ZoneSiteShape> { shape });

        var result = await CreateService().GetSiteConfigAsync();

        result.Should().NotBeNull();
        result.SatelliteImageUrl.Should().Be("https://example.com/sat.jpg");
        result.ZoneShapes.Should().HaveCount(1);
        result.ZoneShapes[0].Id.Should().Be(shape.Id);
        result.ZoneShapes[0].ZoneId.Should().Be(shape.ZoneId);
        result.ZoneShapes[0].Color.Should().Be("#dbeafe");
    }

    [Fact]
    public async Task GetSiteConfigAsync_NoSite_ReturnsEmptyConfig()
    {
        _repositoryMock.Setup(r => r.GetSiteAsync()).ReturnsAsync((Site?)null);
        _repositoryMock.Setup(r => r.GetZoneSiteShapesAsync()).ReturnsAsync(new List<ZoneSiteShape>());

        var result = await CreateService().GetSiteConfigAsync();

        result.Should().NotBeNull();
        result.SatelliteImageUrl.Should().BeNull();
        result.ZoneShapes.Should().BeEmpty();
    }

    [Fact]
    public async Task GetSiteConfigAsync_NoShapes_ReturnsConfigWithEmptyList()
    {
        var site = CreateTestSite("https://example.com/sat.jpg");
        _repositoryMock.Setup(r => r.GetSiteAsync()).ReturnsAsync(site);
        _repositoryMock.Setup(r => r.GetZoneSiteShapesAsync()).ReturnsAsync(new List<ZoneSiteShape>());

        var result = await CreateService().GetSiteConfigAsync();

        result.Should().NotBeNull();
        result.SatelliteImageUrl.Should().Be("https://example.com/sat.jpg");
        result.ZoneShapes.Should().BeEmpty();
    }

    // --- UpdateSiteConfigAsync ---

    [Fact]
    public async Task UpdateSiteConfigAsync_ValidData_UpdatesAndReturnsConfig()
    {
        var site = CreateTestSite();
        var dto = new UpdateSiteConfigDto { SatelliteImageUrl = "https://example.com/new-sat.jpg" };
        var shape = CreateTestShape();
        _repositoryMock.Setup(r => r.GetSiteAsync()).ReturnsAsync(site);
        _repositoryMock.Setup(r => r.UpdateSiteAsync(It.IsAny<Site>()))
            .Callback<Site>(s => s.SatelliteImageUrl = dto.SatelliteImageUrl)
            .Returns(Task.CompletedTask);
        _repositoryMock.Setup(r => r.GetZoneSiteShapesAsync()).ReturnsAsync(new List<ZoneSiteShape> { shape });

        var result = await CreateService().UpdateSiteConfigAsync(dto, Guid.NewGuid());

        result.Should().NotBeNull();
        result.SatelliteImageUrl.Should().Be("https://example.com/new-sat.jpg");
        result.ZoneShapes.Should().HaveCount(1);
    }

    [Fact]
    public async Task UpdateSiteConfigAsync_NoSite_ThrowsKeyNotFound()
    {
        _repositoryMock.Setup(r => r.GetSiteAsync()).ReturnsAsync((Site?)null);

        var act = () => CreateService().UpdateSiteConfigAsync(
            new UpdateSiteConfigDto { SatelliteImageUrl = "https://example.com/sat.jpg" },
            Guid.NewGuid());

        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage("*Site not found*");
    }

    [Fact]
    public async Task UpdateSiteConfigAsync_LogsActivity()
    {
        var site = CreateTestSite();
        var userId = Guid.NewGuid();
        var dto = new UpdateSiteConfigDto { SatelliteImageUrl = "https://example.com/sat.jpg" };
        _repositoryMock.Setup(r => r.GetSiteAsync()).ReturnsAsync(site);
        _repositoryMock.Setup(r => r.UpdateSiteAsync(It.IsAny<Site>())).Returns(Task.CompletedTask);
        _repositoryMock.Setup(r => r.GetZoneSiteShapesAsync()).ReturnsAsync(new List<ZoneSiteShape>());

        await CreateService().UpdateSiteConfigAsync(dto, userId);

        _activityLogServiceMock.Verify(a => a.TrackFacilityChangeAsync(
            "Updated", "SiteConfig", "site", "Satellite Image URL", null, userId), Times.Once);
    }

    // --- CreateZoneSiteShapeAsync ---

    [Fact]
    public async Task CreateZoneSiteShapeAsync_ValidData_ReturnsCreatedShape()
    {
        var zoneId = Guid.NewGuid();
        var points = "[{\"x\":150,\"y\":75},{\"x\":550,\"y\":75},{\"x\":550,\"y\":325},{\"x\":150,\"y\":325}]";
        var dto = new CreateZoneSiteShapeDto
        {
            ZoneId = zoneId,
            Points = points,
            Color = "#fce7f3",
            Label = "New Zone"
        };
        _repositoryMock.Setup(r => r.GetZoneByIdAsync(zoneId))
            .ReturnsAsync(new Zone { Id = zoneId, Code = "Z1", Name = "Zone One" });
        _repositoryMock.Setup(r => r.AddZoneSiteShapeAsync(It.IsAny<ZoneSiteShape>()))
            .ReturnsAsync((ZoneSiteShape s) => s);

        var result = await CreateService().CreateZoneSiteShapeAsync(dto, Guid.NewGuid());

        result.Should().NotBeNull();
        result.ZoneId.Should().Be(zoneId);
        result.Points.Should().Be(points);
        result.Color.Should().Be("#fce7f3");
        result.Label.Should().Be("New Zone");
    }

    [Fact]
    public async Task CreateZoneSiteShapeAsync_UsesDefaultColorWhenNull()
    {
        var zoneId = Guid.NewGuid();
        var dto = new CreateZoneSiteShapeDto
        {
            ZoneId = zoneId,
            Points = "[{\"x\":100,\"y\":50},{\"x\":300,\"y\":50},{\"x\":300,\"y\":200},{\"x\":100,\"y\":200}]",
            Color = null,
            Label = null
        };
        _repositoryMock.Setup(r => r.GetZoneByIdAsync(zoneId))
            .ReturnsAsync(new Zone { Id = zoneId, Code = "Z1", Name = "Zone One" });
        _repositoryMock.Setup(r => r.AddZoneSiteShapeAsync(It.IsAny<ZoneSiteShape>()))
            .ReturnsAsync((ZoneSiteShape s) => s);

        var result = await CreateService().CreateZoneSiteShapeAsync(dto, Guid.NewGuid());

        result.Color.Should().Be("#3b82f6");
    }

    [Fact]
    public async Task CreateZoneSiteShapeAsync_InvalidZoneId_ThrowsArgumentException()
    {
        var zoneId = Guid.NewGuid();
        var dto = new CreateZoneSiteShapeDto
        {
            ZoneId = zoneId,
            Points = "[{\"x\":100,\"y\":50},{\"x\":300,\"y\":50},{\"x\":300,\"y\":200},{\"x\":100,\"y\":200}]"
        };
        _repositoryMock.Setup(r => r.GetZoneByIdAsync(zoneId))
            .ReturnsAsync((Zone?)null);

        var act = () => CreateService().CreateZoneSiteShapeAsync(dto, Guid.NewGuid());

        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*not found*");
    }

    [Fact]
    public async Task CreateZoneSiteShapeAsync_SendsFacilitySiteShapeCreatedNotification()
    {
        var zoneId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var dto = new CreateZoneSiteShapeDto
        {
            ZoneId = zoneId,
            Points = "[{\"x\":100,\"y\":50},{\"x\":300,\"y\":50},{\"x\":300,\"y\":200},{\"x\":100,\"y\":200}]"
        };
        _repositoryMock.Setup(r => r.GetZoneByIdAsync(zoneId))
            .ReturnsAsync(new Zone { Id = zoneId, Code = "Z1", Name = "Zone One" });
        _repositoryMock.Setup(r => r.AddZoneSiteShapeAsync(It.IsAny<ZoneSiteShape>()))
            .ReturnsAsync((ZoneSiteShape s) => s);

        await CreateService().CreateZoneSiteShapeAsync(dto, userId);

        _notificationServiceMock.Verify(
            n => n.CreateNotificationAsync(
                It.Is<CreateNotificationDto>(d =>
                    d.EventType == NotificationEventType.FacilitySiteShapeCreated &&
                    d.Type == NotificationType.Info &&
                    d.TargetRole == UserRole.Supervisor),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    // --- UpdateZoneSiteShapeAsync ---

    [Fact]
    public async Task UpdateZoneSiteShapeAsync_ValidUpdate_ReturnsUpdatedShape()
    {
        var shapeId = Guid.NewGuid();
        var originalPoints = "[{\"x\":100,\"y\":50},{\"x\":400,\"y\":50},{\"x\":400,\"y\":250},{\"x\":100,\"y\":250}]";
        var newPoints = "[{\"x\":200,\"y\":100},{\"x\":700,\"y\":100},{\"x\":700,\"y\":400},{\"x\":200,\"y\":400}]";
        var existing = new ZoneSiteShape
        {
            Id = shapeId,
            ZoneId = Guid.NewGuid(),
            Points = originalPoints,
            Color = "#dbeafe",
            Label = "Old Label",
            CreatedAt = DateTime.UtcNow.AddDays(-1)
        };
        var dto = new UpdateZoneSiteShapeDto
        {
            Points = newPoints,
            Color = "#fce7f3",
            Label = "Updated Label"
        };

        _repositoryMock.Setup(r => r.GetZoneSiteShapeByIdAsync(shapeId)).ReturnsAsync(existing);
        _repositoryMock.Setup(r => r.UpdateZoneSiteShapeAsync(It.IsAny<ZoneSiteShape>()))
            .ReturnsAsync((ZoneSiteShape s) => s);

        var result = await CreateService().UpdateZoneSiteShapeAsync(shapeId, dto, Guid.NewGuid());

        result.Should().NotBeNull();
        result.Points.Should().Be(newPoints);
        result.Color.Should().Be("#fce7f3");
        result.Label.Should().Be("Updated Label");
    }

    [Fact]
    public async Task UpdateZoneSiteShapeAsync_PartialUpdate_KeepsUnchangedFields()
    {
        var shapeId = Guid.NewGuid();
        var originalPoints = "[{\"x\":100,\"y\":50},{\"x\":400,\"y\":50},{\"x\":400,\"y\":250},{\"x\":100,\"y\":250}]";
        var existing = new ZoneSiteShape
        {
            Id = shapeId,
            ZoneId = Guid.NewGuid(),
            Points = originalPoints,
            Color = "#dbeafe",
            Label = "Original Label",
            CreatedAt = DateTime.UtcNow.AddDays(-1)
        };
        var dto = new UpdateZoneSiteShapeDto
        {
            Color = "#ef4444",
            Label = "Updated Label"
        };

        _repositoryMock.Setup(r => r.GetZoneSiteShapeByIdAsync(shapeId)).ReturnsAsync(existing);
        _repositoryMock.Setup(r => r.UpdateZoneSiteShapeAsync(It.IsAny<ZoneSiteShape>()))
            .ReturnsAsync((ZoneSiteShape s) => s);

        var result = await CreateService().UpdateZoneSiteShapeAsync(shapeId, dto, Guid.NewGuid());

        result.Points.Should().Be(originalPoints);
        result.Color.Should().Be("#ef4444");
        result.Label.Should().Be("Updated Label");
    }

    [Fact]
    public async Task UpdateZoneSiteShapeAsync_MissingShape_ThrowsKeyNotFound()
    {
        var shapeId = Guid.NewGuid();
        _repositoryMock.Setup(r => r.GetZoneSiteShapeByIdAsync(shapeId))
            .ReturnsAsync((ZoneSiteShape?)null);

        var act = () => CreateService().UpdateZoneSiteShapeAsync(
            shapeId, new UpdateZoneSiteShapeDto(), Guid.NewGuid());

        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage($"*{shapeId}*");
    }

    [Fact]
    public async Task UpdateZoneSiteShapeAsync_SendsFacilitySiteShapeUpdatedNotification()
    {
        var shapeId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var existing = CreateTestShape();
        _repositoryMock.Setup(r => r.GetZoneSiteShapeByIdAsync(shapeId)).ReturnsAsync(existing);
        _repositoryMock.Setup(r => r.UpdateZoneSiteShapeAsync(It.IsAny<ZoneSiteShape>()))
            .ReturnsAsync(existing);

        await CreateService().UpdateZoneSiteShapeAsync(shapeId, new UpdateZoneSiteShapeDto { Points = "[{\"x\":200,\"y\":100}]" }, userId);

        _notificationServiceMock.Verify(
            n => n.CreateNotificationAsync(
                It.Is<CreateNotificationDto>(d =>
                    d.EventType == NotificationEventType.FacilitySiteShapeUpdated),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    // --- DeleteZoneSiteShapeAsync ---

    [Fact]
    public async Task DeleteZoneSiteShapeAsync_ExistingShape_DeletesSuccessfully()
    {
        var shapeId = Guid.NewGuid();
        var existing = CreateTestShape();
        _repositoryMock.Setup(r => r.GetZoneSiteShapeByIdAsync(shapeId)).ReturnsAsync(existing);
        _repositoryMock.Setup(r => r.DeleteZoneSiteShapeAsync(shapeId)).Returns(Task.CompletedTask);

        await CreateService().DeleteZoneSiteShapeAsync(shapeId, Guid.NewGuid());

        _repositoryMock.Verify(r => r.DeleteZoneSiteShapeAsync(shapeId), Times.Once);
    }

    [Fact]
    public async Task DeleteZoneSiteShapeAsync_MissingShape_ThrowsKeyNotFound()
    {
        var shapeId = Guid.NewGuid();
        _repositoryMock.Setup(r => r.GetZoneSiteShapeByIdAsync(shapeId))
            .ReturnsAsync((ZoneSiteShape?)null);

        var act = () => CreateService().DeleteZoneSiteShapeAsync(shapeId, Guid.NewGuid());

        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage($"*{shapeId}*");
    }

    [Fact]
    public async Task DeleteZoneSiteShapeAsync_SendsFacilitySiteShapeDeletedNotification()
    {
        var shapeId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var existing = CreateTestShape();
        _repositoryMock.Setup(r => r.GetZoneSiteShapeByIdAsync(shapeId)).ReturnsAsync(existing);
        _repositoryMock.Setup(r => r.DeleteZoneSiteShapeAsync(shapeId)).Returns(Task.CompletedTask);

        await CreateService().DeleteZoneSiteShapeAsync(shapeId, userId);

        _notificationServiceMock.Verify(
            n => n.CreateNotificationAsync(
                It.Is<CreateNotificationDto>(d =>
                    d.EventType == NotificationEventType.FacilitySiteShapeDeleted &&
                    d.Type == NotificationType.Warning &&
                    d.TargetRole == UserRole.Supervisor),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task DeleteZoneSiteShapeAsync_LogsActivity()
    {
        var shapeId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var existing = CreateTestShape();
        _repositoryMock.Setup(r => r.GetZoneSiteShapeByIdAsync(shapeId)).ReturnsAsync(existing);
        _repositoryMock.Setup(r => r.DeleteZoneSiteShapeAsync(shapeId)).Returns(Task.CompletedTask);

        await CreateService().DeleteZoneSiteShapeAsync(shapeId, userId);

        _activityLogServiceMock.Verify(a => a.TrackFacilityChangeAsync(
            "Deleted", "ZoneSiteShape", shapeId.ToString(), $"Shape {shapeId}", null, userId), Times.Once);
    }
}
