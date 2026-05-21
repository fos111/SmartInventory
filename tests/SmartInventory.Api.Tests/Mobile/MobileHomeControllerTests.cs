using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using SmartInventory.Api.Controllers.Mobile;
using SmartInventory.Api.Models;
using SmartInventory.Application.Mobile.Home.DTOs;
using SmartInventory.Application.Mobile.Home.Interfaces;
using SmartInventory.Application.Mobile.Auth.DTOs;
using Xunit;

namespace SmartInventory.Api.Tests.Mobile;

public class MobileHomeControllerTests
{
    private readonly Mock<IMobileHomeService> _homeServiceMock;
    private readonly MobileHomeController _controller;
    private readonly Guid _defaultUserId = Guid.NewGuid();

    public MobileHomeControllerTests()
    {
        _homeServiceMock = new Mock<IMobileHomeService>();
        _controller = new MobileHomeController(_homeServiceMock.Object);

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
    public async Task GetHome_ReturnsOkWithHomeSyncDto()
    {
        var homeSyncDto = new HomeSyncDto
        {
            User = new MobileUserDto { Id = Guid.NewGuid(), Name = "Test User", Email = "test@test.com", Role = "Technician", Avatar = null },
            Stats = new HomeInventoryStatsDto
            {
                InStock = 10,
                UnderMaintenance = 2,
                Critical = 1,
                Retired = 0
            },
            RecentActivity = new List<ActivityLogEntryDto>
            {
                new()
                {
                    Id = Guid.NewGuid(),
                    Action = "Check-in",
                    EntityName = "Laptop Dell",
                    ChangedAt = DateTime.UtcNow,
                    ChangedByName = "Admin"
                }
            },
            UnreadNotifications = 3,
            ServerTimestamp = DateTime.UtcNow
        };

        _homeServiceMock
            .Setup(s => s.GetHomeAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(homeSyncDto);

        var result = await _controller.GetHome(CancellationToken.None);

        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var envelope = okResult.Value.Should().BeOfType<MobileEnvelope<HomeSyncDto>>().Subject;
        envelope.Success.Should().BeTrue();
        envelope.Data.Should().NotBeNull();
        envelope.Data!.User.Should().NotBeNull();
        envelope.Data.UnreadNotifications.Should().Be(3);
    }

    [Fact]
    public async Task GetHome_WhenUserNull_ReturnsNullUser()
    {
        var homeSyncDto = new HomeSyncDto
        {
            User = null,
            Stats = null,
            RecentActivity = new List<ActivityLogEntryDto>(),
            UnreadNotifications = 0,
            ServerTimestamp = DateTime.UtcNow
        };

        _homeServiceMock
            .Setup(s => s.GetHomeAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(homeSyncDto);

        var result = await _controller.GetHome(CancellationToken.None);

        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var envelope = okResult.Value.Should().BeOfType<MobileEnvelope<HomeSyncDto>>().Subject;
        envelope.Data.Should().NotBeNull();
        envelope.Data!.User.Should().BeNull();
    }

    [Fact]
    public async Task GetHome_ServiceCalledWithCorrectUserId()
    {
        _homeServiceMock
            .Setup(s => s.GetHomeAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new HomeSyncDto());

        await _controller.GetHome(CancellationToken.None);

        _homeServiceMock.Verify(s => s.GetHomeAsync(_defaultUserId, It.IsAny<CancellationToken>()), Times.Once);
    }
}
