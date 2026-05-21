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
using SmartInventory.Application.Mobile.Sync.DTOs;
using SmartInventory.Application.Mobile.Sync.Interfaces;
using Xunit;

namespace SmartInventory.Api.Tests.Mobile;

public class MobileSyncControllerTests
{
    private readonly Mock<IMobileSyncService> _syncServiceMock;
    private readonly MobileSyncController _controller;
    private readonly Guid _defaultUserId = Guid.NewGuid();

    public MobileSyncControllerTests()
    {
        _syncServiceMock = new Mock<IMobileSyncService>();
        _controller = new MobileSyncController(_syncServiceMock.Object);

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
    public async Task PostQueue_ReturnsOkWithStatusDto()
    {
        var dto = new SyncBatchDto
        {
            DeviceId = "device-01",
            Operations = new List<SyncOperationDto>
            {
                new() { OperationType = "Create", AssetTag = "AST-001", PerformedAt = DateTime.UtcNow },
                new() { OperationType = "Update", AssetTag = "AST-002", PerformedAt = DateTime.UtcNow }
            },
            LastSyncTimestamp = DateTime.UtcNow
        };

        var statusDto = new SyncStatusDto
        {
            PendingOperations = 2,
            LastSyncTimestamp = DateTime.UtcNow
        };

        _syncServiceMock
            .Setup(s => s.ProcessQueueAsync(dto, _defaultUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(statusDto);

        var result = await _controller.QueueSync(dto, CancellationToken.None);

        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var envelope = okResult.Value.Should().BeOfType<MobileEnvelope<SyncStatusDto>>().Subject;
        envelope.Success.Should().BeTrue();
        envelope.Data.Should().NotBeNull();
        envelope.Data!.PendingOperations.Should().Be(2);
    }

    [Fact]
    public async Task PostQueue_ServiceCalledWithCorrectUserId()
    {
        var dto = new SyncBatchDto
        {
            DeviceId = "device-01",
            Operations = new List<SyncOperationDto>
            {
                new() { OperationType = "Create", AssetTag = "AST-001", PerformedAt = DateTime.UtcNow }
            },
            LastSyncTimestamp = DateTime.UtcNow
        };

        _syncServiceMock
            .Setup(s => s.ProcessQueueAsync(dto, _defaultUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new SyncStatusDto());

        await _controller.QueueSync(dto, CancellationToken.None);

        _syncServiceMock.Verify(
            s => s.ProcessQueueAsync(dto, _defaultUserId, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetStatus_ReturnsOkWithStatusDto()
    {
        var lastSync = DateTime.UtcNow;
        var statusDto = new SyncStatusDto
        {
            PendingOperations = 5,
            LastSyncTimestamp = lastSync
        };

        _syncServiceMock
            .Setup(s => s.GetStatusAsync(_defaultUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(statusDto);

        var result = await _controller.GetStatus(CancellationToken.None);

        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var envelope = okResult.Value.Should().BeOfType<MobileEnvelope<SyncStatusDto>>().Subject;
        envelope.Success.Should().BeTrue();
        envelope.Data.Should().NotBeNull();
        envelope.Data!.PendingOperations.Should().Be(5);
        envelope.Data.LastSyncTimestamp.Should().NotBeNull();
    }

    [Fact]
    public async Task GetStatus_ReturnsZero_WhenNoPending()
    {
        var statusDto = new SyncStatusDto
        {
            PendingOperations = 0,
            LastSyncTimestamp = null
        };

        _syncServiceMock
            .Setup(s => s.GetStatusAsync(_defaultUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(statusDto);

        var result = await _controller.GetStatus(CancellationToken.None);

        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var envelope = okResult.Value.Should().BeOfType<MobileEnvelope<SyncStatusDto>>().Subject;
        envelope.Success.Should().BeTrue();
        envelope.Data.Should().NotBeNull();
        envelope.Data!.PendingOperations.Should().Be(0);
        envelope.Data.LastSyncTimestamp.Should().BeNull();
    }
}
