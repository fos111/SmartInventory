using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using SmartInventory.Api.Controllers.Mobile;
using SmartInventory.Api.Models;
using SmartInventory.Application.Mobile.Lookup.DTOs;
using SmartInventory.Application.Mobile.Lookup.Interfaces;
using SmartInventory.Application.Asset.Interfaces;
using Xunit;

namespace SmartInventory.Api.Tests.Mobile;

public class MobileLookupControllerTests
{
    private readonly Mock<IMobileLookupService> _lookupServiceMock;
    private readonly Mock<IActivityLogService> _activityLogMock;
    private readonly MobileLookupController _controller;

    public MobileLookupControllerTests()
    {
        _lookupServiceMock = new Mock<IMobileLookupService>();
        _activityLogMock = new Mock<IActivityLogService>();
        _controller = new MobileLookupController(_lookupServiceMock.Object, _activityLogMock.Object);
    }

    [Fact]
    public async Task GetCategories_ReturnsOkWithEnvelope()
    {
        var categories = new List<MobileCategoryDto>
        {
            new() { Name = "Laptop", Group = "Electronics" }
        };

        _lookupServiceMock
            .Setup(s => s.GetCategoriesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(categories);

        var result = await _controller.GetCategories(CancellationToken.None);

        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var envelope = okResult.Value.Should().BeOfType<MobileEnvelope<List<MobileCategoryDto>>>().Subject;
        envelope.Success.Should().BeTrue();
        envelope.Data.Should().HaveCount(1);
        envelope.Data![0].Name.Should().Be("Laptop");
    }

    [Fact]
    public async Task GetDepartments_ReturnsOkWithEnvelope()
    {
        var departments = new List<MobileDepartmentDto>
        {
            new() { Id = Guid.NewGuid(), Code = "ZA", Name = "Zone A", RoomCount = 5 }
        };

        _lookupServiceMock
            .Setup(s => s.GetDepartmentsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(departments);

        var result = await _controller.GetDepartments(CancellationToken.None);

        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var envelope = okResult.Value.Should().BeOfType<MobileEnvelope<List<MobileDepartmentDto>>>().Subject;
        envelope.Success.Should().BeTrue();
        envelope.Data.Should().HaveCount(1);
        envelope.Data![0].Name.Should().Be("Zone A");
    }

    [Fact]
    public async Task GetRoomsByDepartment_ValidZoneId_ReturnsOkWithEnvelope()
    {
        var zoneId = Guid.NewGuid();
        var rooms = new List<MobileRoomDto>
        {
            new() { Id = Guid.NewGuid(), Code = "Room-01", Name = "Server Room", ZoneName = "Zone A" }
        };

        _lookupServiceMock
            .Setup(s => s.GetRoomsByDepartmentAsync(zoneId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(rooms);

        var result = await _controller.GetRoomsByDepartment(zoneId, CancellationToken.None);

        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var envelope = okResult.Value.Should().BeOfType<MobileEnvelope<List<MobileRoomDto>>>().Subject;
        envelope.Success.Should().BeTrue();
        envelope.Data.Should().HaveCount(1);
        envelope.Data![0].Code.Should().Be("Room-01");
    }

    [Fact]
    public async Task GetStats_ReturnsOkWithFlatStats()
    {
        var stats = new MobileInventoryStatsDto
        {
            InStock = 42,
            Maintenance = 3,
            Critical = 1,
            Retired = 5
        };

        _lookupServiceMock
            .Setup(s => s.GetStatsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(stats);

        var result = await _controller.GetStats(CancellationToken.None);

        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var envelope = okResult.Value.Should().BeOfType<MobileEnvelope<MobileInventoryStatsDto>>().Subject;
        envelope.Success.Should().BeTrue();
        envelope.Data!.InStock.Should().Be(42);
        envelope.Data.Maintenance.Should().Be(3);
        envelope.Data.Critical.Should().Be(1);
        envelope.Data.Retired.Should().Be(5);
    }

    [Fact]
    public async Task GetMoveLog_ReturnsOkWithEnvelope()
    {
        var entries = new List<MobileMoveLogEntryDto>
        {
            new()
            {
                Id = Guid.NewGuid(),
                AssetTag = "TAG-001",
                AssetName = "Laptop",
                ChangedByName = "John",
                ChangedAt = new DateTime(2026, 5, 14, 10, 0, 0, DateTimeKind.Utc)
            }
        };

        _lookupServiceMock
            .Setup(s => s.GetMoveLogAsync(It.IsAny<DateTime?>(), It.IsAny<DateTime?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(entries);

        var result = await _controller.GetMoveLog(null, null, CancellationToken.None);

        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var envelope = okResult.Value.Should().BeOfType<MobileEnvelope<List<MobileMoveLogEntryDto>>>().Subject;
        envelope.Success.Should().BeTrue();
        envelope.Data.Should().HaveCount(1);
        envelope.Data![0].AssetTag.Should().Be("TAG-001");
    }

    [Fact]
    public async Task CheckBarcode_Found_ReturnsOkWithExistsTrue()
    {
        var barcode = "TAG-001";
        var resultDto = new BarcodeCheckResultDto
        {
            Exists = true,
            AssetTag = barcode,
            Name = "Laptop 1"
        };

        _lookupServiceMock
            .Setup(s => s.CheckBarcodeAsync(barcode, It.IsAny<CancellationToken>()))
            .ReturnsAsync(resultDto);

        var result = await _controller.CheckBarcode(barcode, CancellationToken.None);

        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var envelope = okResult.Value.Should().BeOfType<MobileEnvelope<BarcodeCheckResultDto>>().Subject;
        envelope.Success.Should().BeTrue();
        envelope.Data!.Exists.Should().BeTrue();
        envelope.Data.AssetTag.Should().Be(barcode);
    }

    [Fact]
    public async Task CheckBarcode_NotFound_ReturnsOkWithExistsFalse()
    {
        var barcode = "NONEXISTENT";
        var resultDto = new BarcodeCheckResultDto
        {
            Exists = false,
            AssetTag = null,
            Name = null
        };

        _lookupServiceMock
            .Setup(s => s.CheckBarcodeAsync(barcode, It.IsAny<CancellationToken>()))
            .ReturnsAsync(resultDto);

        var result = await _controller.CheckBarcode(barcode, CancellationToken.None);

        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var envelope = okResult.Value.Should().BeOfType<MobileEnvelope<BarcodeCheckResultDto>>().Subject;
        envelope.Success.Should().BeTrue();
        envelope.Data!.Exists.Should().BeFalse();
        envelope.Data.AssetTag.Should().BeNull();
    }
}
