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
using SmartInventory.Application.Mobile.Products.DTOs;
using SmartInventory.Application.Mobile.Products.Interfaces;
using SmartInventory.Application.Mobile.Sync.DTOs;
using SmartInventory.Application.Mobile.Sync.Interfaces;
using Xunit;

namespace SmartInventory.Api.Tests.Mobile;

public class MobileAssetsControllerTests
{
    private readonly Mock<IMobileProductService> _productServiceMock;
    private readonly Mock<IMobileSyncService> _syncServiceMock;
    private readonly MobileAssetsController _controller;
    private readonly Guid _defaultUserId = Guid.NewGuid();

    public MobileAssetsControllerTests()
    {
        _productServiceMock = new Mock<IMobileProductService>();
        _syncServiceMock = new Mock<IMobileSyncService>();
        _controller = new MobileAssetsController(_productServiceMock.Object, _syncServiceMock.Object);

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
    public async Task GetAssets_ReturnsOkWithPagedResult()
    {
        var items = new List<AssetListItemDto>
        {
            new()
            {
                Id = Guid.NewGuid(),
                AssetTag = "AST-001",
                Name = "Laptop Dell",
                Status = "Active",
                CurrentRoomCode = "Room-A1-01",
                Category = "Electronics"
            },
            new()
            {
                Id = Guid.NewGuid(),
                AssetTag = "AST-002",
                Name = "Monitor LG",
                Status = "Active",
                CurrentRoomCode = "Room-A1-02",
                Category = "Electronics"
            }
        };

        var pagedResult = new MobilePagedResultDto<AssetListItemDto>
        {
            Items = items,
            TotalCount = 2,
            Page = 1,
            PageSize = 20,
            HasNextPage = false
        };

        _productServiceMock
            .Setup(s => s.GetProductsAsync(It.IsAny<MobileProductFilterDto>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(pagedResult);

        var filter = new MobileProductFilterDto();
        var result = await _controller.GetAssets(filter, CancellationToken.None);

        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var envelope = okResult.Value.Should().BeOfType<MobileEnvelope<MobilePagedResultDto<AssetListItemDto>>>().Subject;
        envelope.Success.Should().BeTrue();
        envelope.Data.Should().NotBeNull();
        envelope.Data!.Items.Should().HaveCount(2);
        envelope.Data.Items[0].AssetTag.Should().Be("AST-001");
    }

    [Fact]
    public async Task GetAssets_PassesFilterToService()
    {
        var since = DateTime.UtcNow;
        var filter = new MobileProductFilterDto { Since = since };

        _productServiceMock
            .Setup(s => s.GetProductsAsync(It.IsAny<MobileProductFilterDto>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new MobilePagedResultDto<AssetListItemDto>());

        await _controller.GetAssets(filter, CancellationToken.None);

        _productServiceMock.Verify(
            s => s.GetProductsAsync(It.Is<MobileProductFilterDto>(f => f.Since == since), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task PostBatch_ReturnsOkWithResults()
    {
        var request = new BatchOperationRequest
        {
            Operations = new List<BatchAssetOperationDto>
            {
                new()
            }
        };

        var results = new List<BatchOperationResult>
        {
            new()
            {
                Index = 0,
                Success = true,
                AssetTag = "AST-001",
                Error = null
            }
        };

        _syncServiceMock
            .Setup(s => s.ProcessBatchAsync(request, _defaultUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(results);

        var result = await _controller.Batch(request, CancellationToken.None);

        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var envelope = okResult.Value.Should().BeOfType<MobileEnvelope<List<BatchOperationResult>>>().Subject;
        envelope.Success.Should().BeTrue();
        envelope.Data.Should().NotBeNull();
        envelope.Data.Should().HaveCount(1);
        envelope.Data![0].Success.Should().BeTrue();
        envelope.Data[0].AssetTag.Should().Be("AST-001");
    }

    [Fact]
    public async Task PostBatch_ServiceCalledWithCorrectUserId()
    {
        var request = new BatchOperationRequest
        {
            Operations = new List<BatchAssetOperationDto>
            {
                new()
            }
        };

        _syncServiceMock
            .Setup(s => s.ProcessBatchAsync(It.IsAny<BatchOperationRequest>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<BatchOperationResult>());

        await _controller.Batch(request, CancellationToken.None);

        _syncServiceMock.Verify(
            s => s.ProcessBatchAsync(request, _defaultUserId, It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
