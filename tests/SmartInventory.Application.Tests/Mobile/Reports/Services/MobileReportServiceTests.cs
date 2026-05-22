using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using SmartInventory.Application.Asset.DTOs;
using SmartInventory.Application.Asset.DTOs.Reports;
using SmartInventory.Application.Asset.Interfaces;
using SmartInventory.Application.Caching;
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
            .Setup(s => s.GenerateRoomAuditAsync(auditDto, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new byte[] { 0x25, 0x50, 0x44, 0x46, 0x00, 0x01 });

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
            .Setup(s => s.GenerateRoomJournalAsync(It.IsAny<RoomJournalDto>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new byte[] { 0x25, 0x50, 0x44, 0x46, 0x00, 0x01 });

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
            .Setup(s => s.GenerateRoomJournalAsync(It.IsAny<RoomJournalDto>(), It.IsAny<CancellationToken>()))
            .Callback<RoomJournalDto, CancellationToken>((j, _) => capturedJournal = j)
            .ReturnsAsync(new byte[] { 0x25, 0x50, 0x44, 0x46 });

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

public class MobileReportServiceCacheTests
{
    private readonly Mock<IReportingService> _reportingServiceMock;
    private readonly Mock<ILocationService> _locationServiceMock;
    private readonly Mock<ILocationRepository> _locationRepoMock;
    private readonly Mock<IPdfReportService> _pdfReportServiceMock;
    private readonly Mock<IBlobCacheService> _blobCacheMock;
    private readonly IMobileReportService _service;

    public MobileReportServiceCacheTests()
    {
        _reportingServiceMock = new Mock<IReportingService>();
        _locationServiceMock = new Mock<ILocationService>();
        _locationRepoMock = new Mock<ILocationRepository>();
        _pdfReportServiceMock = new Mock<IPdfReportService>();
        _blobCacheMock = new Mock<IBlobCacheService>();

        _service = new MobileReportService(
            _reportingServiceMock.Object,
            _locationServiceMock.Object,
            _locationRepoMock.Object,
            _pdfReportServiceMock.Object,
            _blobCacheMock.Object);
    }

    [Fact]
    public async Task GetDepartmentQrAsync_WhenCached_ReturnsCachedBytes()
    {
        var deptId = Guid.NewGuid();
        var zone = new Zone { Id = deptId, Code = "CS", Name = "Computer Science" };
        var cachedBytes = new byte[] { 0x89, 0x50, 0x4E, 0x47 };

        _locationRepoMock.Setup(r => r.GetZoneByIdAsync(deptId)).ReturnsAsync(zone);
        _blobCacheMock.Setup(b => b.GetAsync($"qrcodes/department-{deptId}.png", It.IsAny<CancellationToken>()))
            .ReturnsAsync(cachedBytes);

        var result = await _service.GetDepartmentQrAsync(deptId);

        result.Should().BeSameAs(cachedBytes);
        _blobCacheMock.Verify(b => b.SetAsync(It.IsAny<string>(), It.IsAny<byte[]>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task GetDepartmentQrAsync_WhenNotCached_GeneratesAndCaches()
    {
        var deptId = Guid.NewGuid();
        var zone = new Zone { Id = deptId, Code = "CS", Name = "Computer Science" };

        _locationRepoMock.Setup(r => r.GetZoneByIdAsync(deptId)).ReturnsAsync(zone);
        _blobCacheMock.Setup(b => b.GetAsync($"qrcodes/department-{deptId}.png", It.IsAny<CancellationToken>()))
            .ReturnsAsync((byte[]?)null);

        var result = await _service.GetDepartmentQrAsync(deptId);

        result.Should().NotBeNull();
        _blobCacheMock.Verify(b => b.SetAsync($"qrcodes/department-{deptId}.png", result!, "image/png", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetRoomQrAsync_WhenCached_ReturnsCachedBytes()
    {
        var roomCode = "A101";
        var room = new RoomDto { Id = Guid.NewGuid(), Code = roomCode, Name = "Room" };
        var cachedBytes = new byte[] { 0x89, 0x50, 0x4E, 0x47 };

        _locationServiceMock.Setup(s => s.GetRoomByCodeAsync(roomCode)).ReturnsAsync(room);
        _blobCacheMock.Setup(b => b.GetAsync($"qrcodes/room-{roomCode}.png", It.IsAny<CancellationToken>()))
            .ReturnsAsync(cachedBytes);

        var result = await _service.GetRoomQrAsync(roomCode);

        result.Should().BeSameAs(cachedBytes);
        _blobCacheMock.Verify(b => b.SetAsync(It.IsAny<string>(), It.IsAny<byte[]>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task GetIsetQrAsync_WhenCached_ReturnsCachedBytes()
    {
        var cachedBytes = new byte[] { 0x89, 0x50, 0x4E, 0x47 };

        var hierarchy = new HierarchyDto
        {
            Site = new SiteDto
            {
                Id = Guid.NewGuid(), Code = "MAIN", Name = "Main",
                Zones = new List<ZoneDto> { new() { Id = Guid.NewGuid(), Code = "CS", Name = "CS" } }
            }
        };

        _locationServiceMock.Setup(s => s.GetHierarchyAsync()).ReturnsAsync(hierarchy);
        _blobCacheMock.Setup(b => b.GetAsync("qrcodes/iset.png", It.IsAny<CancellationToken>()))
            .ReturnsAsync(cachedBytes);

        var result = await _service.GetIsetQrAsync();

        result.Should().BeSameAs(cachedBytes);
        _blobCacheMock.Verify(b => b.SetAsync(It.IsAny<string>(), It.IsAny<byte[]>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task GetDepartmentQrByCodeAsync_WhenCached_ReturnsCachedBytes()
    {
        var code = "CS";
        var cachedBytes = new byte[] { 0x89, 0x50, 0x4E, 0x47 };

        _blobCacheMock.Setup(b => b.GetAsync($"qrcodes/department-code-{code}.png", It.IsAny<CancellationToken>()))
            .ReturnsAsync(cachedBytes);

        var result = await _service.GetDepartmentQrByCodeAsync(code);

        result.Should().BeSameAs(cachedBytes);
        _blobCacheMock.Verify(b => b.SetAsync(It.IsAny<string>(), It.IsAny<byte[]>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task InvalidateIsetQrAsync_DeletesCachedBlob()
    {
        await _service.InvalidateIsetQrAsync();

        _blobCacheMock.Verify(b => b.DeleteAsync("qrcodes/iset.png", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetDepartmentQrAsync_WhenNoBlobCache_FallsThroughToGeneration()
    {
        var deptId = Guid.NewGuid();
        var zone = new Zone { Id = deptId, Code = "CS", Name = "CS" };
        var serviceNoCache = new MobileReportService(
            _reportingServiceMock.Object,
            _locationServiceMock.Object,
            _locationRepoMock.Object,
            _pdfReportServiceMock.Object);

        _locationRepoMock.Setup(r => r.GetZoneByIdAsync(deptId)).ReturnsAsync(zone);

        var result = await serviceNoCache.GetDepartmentQrAsync(deptId);

        result.Should().NotBeNull();
        result.Should().HaveCountGreaterThan(100);
    }
}
