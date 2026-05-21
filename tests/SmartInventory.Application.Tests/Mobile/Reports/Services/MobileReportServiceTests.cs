using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using SmartInventory.Application.Asset.DTOs;
using SmartInventory.Application.Asset.DTOs.Reports;
using SmartInventory.Application.Asset.Interfaces;
using SmartInventory.Application.Location.DTOs;
using SmartInventory.Application.Location.Interfaces;
using SmartInventory.Application.Mobile.Reports.DTOs;
using SmartInventory.Application.Mobile.Reports.Interfaces;
using SmartInventory.Domain.Location.Entities;
using SmartInventory.Application.Mobile.Reports.Services;
using Xunit;

namespace SmartInventory.Application.Tests.Mobile.Reports.Services;

public class MobileReportServiceTests
{
    private readonly Mock<IReportingService> _reportingServiceMock;
    private readonly Mock<ILocationService> _locationServiceMock;
    private readonly Mock<ILocationRepository> _locationRepoMock;
    private readonly Mock<IPdfReportService> _pdfReportServiceMock;
    private readonly IMobileReportService _service;

    public MobileReportServiceTests()
    {
        _reportingServiceMock = new Mock<IReportingService>();
        _locationServiceMock = new Mock<ILocationService>();
        _locationRepoMock = new Mock<ILocationRepository>();
        _pdfReportServiceMock = new Mock<IPdfReportService>();

        _service = new MobileReportService(
            _reportingServiceMock.Object,
            _locationServiceMock.Object,
            _locationRepoMock.Object,
            _pdfReportServiceMock.Object);
    }

    #region GetRoomFicheAsync

    [Fact]
    public async Task GetRoomFicheAsync_ValidRoomCode_ReturnsPdfBytes()
    {
        var roomCode = "A101";
        var auditDto = new RoomAuditDto
        {
            RoomCode = roomCode,
            ZoneName = "Zone A",
            BuildingName = "Building A",
            Assets = new List<RoomAssetItem>
            {
                new() { AssetTag = "TAG-001", Name = "Laptop", Status = "InStock" }
            }
        };

        _reportingServiceMock
            .Setup(s => s.GetRoomAuditAsync(roomCode))
            .ReturnsAsync(auditDto);

        _pdfReportServiceMock
            .Setup(s => s.GenerateRoomAudit(auditDto))
            .Returns(new byte[] { 0x25, 0x50, 0x44, 0x46, 0x00, 0x01 });

        var result = await _service.GetRoomFicheAsync(roomCode);

        result.Should().NotBeNull();
        result.Should().HaveCountGreaterThan(0);
        result![..4].Should().BeEquivalentTo(new byte[] { 0x25, 0x50, 0x44, 0x46 }); // %PDF header
    }

    [Fact]
    public async Task GetRoomFicheAsync_InvalidRoomCode_ReturnsNull()
    {
        var roomCode = "INVALID";

        _reportingServiceMock
            .Setup(s => s.GetRoomAuditAsync(roomCode))
            .ReturnsAsync((RoomAuditDto?)null);

        var result = await _service.GetRoomFicheAsync(roomCode);

        result.Should().BeNull();
    }

    #endregion

    #region GetRoomJournalAsync

    [Fact]
    public async Task GetRoomJournalAsync_ValidRoomCode_ReturnsPdfBytes()
    {
        var roomCode = "A101";
        var from = new DateTime(2025, 1, 1);
        var to = new DateTime(2025, 12, 31);

        var room = new RoomDto { Id = Guid.NewGuid(), Code = roomCode, Name = "Server Room A101" };

        _locationServiceMock
            .Setup(s => s.GetRoomByCodeAsync(roomCode))
            .ReturnsAsync(room);

        var activityEntries = new List<ActivityLogDto>
        {
            new()
            {
                Id = Guid.NewGuid(),
                AssetTag = "TAG-001",
                AssetName = "Laptop",
                Action = "moved",
                OldValue = "B200",
                NewValue = roomCode,
                ChangedByName = "John",
                ChangedAt = new DateTime(2025, 6, 15)
            }
        };

        _reportingServiceMock
            .Setup(s => s.GetActivityLogAsync(from, to, null))
            .ReturnsAsync(activityEntries);

        _pdfReportServiceMock
            .Setup(s => s.GenerateRoomJournal(It.IsAny<RoomJournalDto>()))
            .Returns(new byte[] { 0x25, 0x50, 0x44, 0x46, 0x00, 0x01 });

        var result = await _service.GetRoomJournalAsync(roomCode, from, to);

        result.Should().NotBeNull();
        result.Should().HaveCountGreaterThan(0);
        result![..4].Should().BeEquivalentTo(new byte[] { 0x25, 0x50, 0x44, 0x46 }); // %PDF header
    }

    [Fact]
    public async Task GetRoomJournalAsync_InvalidRoomCode_ReturnsNull()
    {
        var roomCode = "INVALID";

        _locationServiceMock
            .Setup(s => s.GetRoomByCodeAsync(roomCode))
            .ReturnsAsync((RoomDto?)null);

        var result = await _service.GetRoomJournalAsync(roomCode, null, null);

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetRoomJournalAsync_FiltersActivityByRoomCode()
    {
        var roomCode = "A101";

        var room = new RoomDto { Id = Guid.NewGuid(), Code = roomCode, Name = "Server Room" };

        _locationServiceMock
            .Setup(s => s.GetRoomByCodeAsync(roomCode))
            .ReturnsAsync(room);

        var activityEntries = new List<ActivityLogDto>
        {
            // This one matches (NewValue == roomCode)
            new()
            {
                Id = Guid.NewGuid(), AssetTag = "TAG-001", AssetName = "Laptop",
                Action = "moved", OldValue = "B200", NewValue = roomCode,
                ChangedByName = "John", ChangedAt = new DateTime(2025, 6, 15)
            },
            // This one doesn't match
            new()
            {
                Id = Guid.NewGuid(), AssetTag = "TAG-002", AssetName = "Monitor",
                Action = "moved", OldValue = "C300", NewValue = "D400",
                ChangedByName = "Jane", ChangedAt = new DateTime(2025, 6, 16)
            },
            // This one matches (OldValue == roomCode — moved OUT of the room)
            new()
            {
                Id = Guid.NewGuid(), AssetTag = "TAG-003", AssetName = "Server",
                Action = "moved", OldValue = roomCode, NewValue = "E500",
                ChangedByName = "Bob", ChangedAt = new DateTime(2025, 6, 17)
            }
        };

        _reportingServiceMock
            .Setup(s => s.GetActivityLogAsync(null, null, null))
            .ReturnsAsync(activityEntries);

        RoomJournalDto? capturedJournal = null;
        _pdfReportServiceMock
            .Setup(s => s.GenerateRoomJournal(It.IsAny<RoomJournalDto>()))
            .Callback<RoomJournalDto>(j => capturedJournal = j)
            .Returns(new byte[] { 0x25, 0x50, 0x44, 0x46 });

        await _service.GetRoomJournalAsync(roomCode, null, null);

        capturedJournal.Should().NotBeNull();
        capturedJournal!.RoomCode.Should().Be(roomCode);
        capturedJournal.Entries.Should().HaveCount(2);
        capturedJournal.Entries.Should().Contain(e => e.AssetTag == "TAG-001");
        capturedJournal.Entries.Should().Contain(e => e.AssetTag == "TAG-003");
        capturedJournal.Entries.Should().NotContain(e => e.AssetTag == "TAG-002");
    }

    #endregion

    #region GetDepartmentQrAsync

    [Fact]
    public async Task GetDepartmentQrAsync_ValidDeptId_ReturnsPngBytes()
    {
        var deptId = Guid.NewGuid();
        var zone = new Zone { Id = deptId, Code = "CS", Name = "Computer Science" };

        _locationRepoMock
            .Setup(r => r.GetZoneByIdAsync(deptId))
            .ReturnsAsync(zone);

        var result = await _service.GetDepartmentQrAsync(deptId);

        result.Should().NotBeNull();
        result.Should().HaveCountGreaterThan(100); // valid PNG is > 100 bytes
        result![..8].Should().BeEquivalentTo(new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A }); // PNG magic number
    }

    [Fact]
    public async Task GetDepartmentQrAsync_InvalidDeptId_ReturnsNull()
    {
        var deptId = Guid.NewGuid();

        _locationRepoMock
            .Setup(r => r.GetZoneByIdAsync(deptId))
            .ReturnsAsync((Zone?)null);

        var result = await _service.GetDepartmentQrAsync(deptId);

        result.Should().BeNull();
    }

    #endregion

    #region GetDepartmentQrByCodeAsync

    [Fact]
    public async Task GetDepartmentQrByCodeAsync_ReturnsPngWithDeptCode()
    {
        var result = await _service.GetDepartmentQrByCodeAsync("CS");

        result.Should().NotBeNull();
        result.Should().HaveCountGreaterThan(100);
        result![..8].Should().BeEquivalentTo(new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A }); // PNG magic number
    }

    #endregion

    #region GetRoomQrAsync

    [Fact]
    public async Task GetRoomQrAsync_ValidRoomCode_ReturnsPngBytes()
    {
        var roomCode = "A101";
        var room = new RoomDto { Id = Guid.NewGuid(), Code = roomCode, Name = "Server Room" };

        _locationServiceMock
            .Setup(s => s.GetRoomByCodeAsync(roomCode))
            .ReturnsAsync(room);

        var result = await _service.GetRoomQrAsync(roomCode);

        result.Should().NotBeNull();
        result.Should().HaveCountGreaterThan(100);
        result![..8].Should().BeEquivalentTo(new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A }); // PNG magic number
    }

    [Fact]
    public async Task GetRoomQrAsync_InvalidRoomCode_ReturnsNull()
    {
        var roomCode = "INVALID";

        _locationServiceMock
            .Setup(s => s.GetRoomByCodeAsync(roomCode))
            .ReturnsAsync((RoomDto?)null);

        var result = await _service.GetRoomQrAsync(roomCode);

        result.Should().BeNull();
    }

    #endregion

    #region GetIsetQrAsync

    [Fact]
    public async Task GetIsetQrAsync_ReturnsPngWithDepartmentCodes()
    {
        var hierarchy = new HierarchyDto
        {
            Site = new SiteDto
            {
                Id = Guid.NewGuid(),
                Code = "MAIN",
                Name = "Main Site",
                Zones = new List<ZoneDto>
                {
                    new() { Id = Guid.NewGuid(), Code = "CS", Name = "Computer Science" },
                    new() { Id = Guid.NewGuid(), Code = "EE", Name = "Electrical Engineering" }
                }
            }
        };

        _locationServiceMock
            .Setup(s => s.GetHierarchyAsync())
            .ReturnsAsync(hierarchy);

        var result = await _service.GetIsetQrAsync();

        result.Should().NotBeNull();
        result.Should().HaveCountGreaterThan(100);
        result![..8].Should().BeEquivalentTo(new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A }); // PNG magic number
    }

    #endregion
}
