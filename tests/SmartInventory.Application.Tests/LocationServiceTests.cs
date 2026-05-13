using FluentAssertions;
using Moq;
using SmartInventory.Application.Asset.Interfaces;
using SmartInventory.Application.Location.DTOs;
using SmartInventory.Application.Location.Interfaces;
using SmartInventory.Application.Location.Services;
using SmartInventory.Domain.Location.Entities;
using Xunit;

namespace SmartInventory.Application.Tests;

public class LocationServiceTests : ApplicationTestBase
{
    private readonly Mock<ILocationRepository> _repositoryMock;
    private readonly Mock<IActivityLogService> _activityLogServiceMock;
    private readonly AutoMapper.Mapper _mapper;

    public LocationServiceTests()
    {
        _repositoryMock = new Mock<ILocationRepository>();
        _activityLogServiceMock = new Mock<IActivityLogService>();

        var config = new AutoMapper.MapperConfiguration(cfg =>
            cfg.AddProfile<SmartInventory.Application.Location.Mappings.LocationMappingProfile>());
        _mapper = new AutoMapper.Mapper(config);
    }

    private LocationService CreateService() => new LocationService(_repositoryMock.Object, _activityLogServiceMock.Object, _mapper);

    [Fact]
    public async Task GetHierarchyAsync_WithData_ReturnsHierarchy()
    {
        var site = CreateTestSite();
        _repositoryMock.Setup(r => r.GetFullHierarchyAsync())
            .ReturnsAsync(new List<Site> { site });

        var service = CreateService();
        var result = await service.GetHierarchyAsync();

        result.Should().NotBeNull();
        result.Site.Should().NotBeNull();
        result.Site.Code.Should().Be("ISETMA");
        result.Site.Name.Should().Be("ISETMA Mahdia");
        result.Site.Zones.Should().HaveCount(1);
    }

    [Fact]
    public async Task GetHierarchyAsync_NoData_ReturnsEmptySite()
    {
        _repositoryMock.Setup(r => r.GetFullHierarchyAsync())
            .ReturnsAsync(new List<Site>());

        var service = CreateService();
        var result = await service.GetHierarchyAsync();

        result.Should().NotBeNull();
        result.Site.Should().BeNull();
    }

    [Fact]
    public async Task GetRoomByCodeAsync_ExistingRoom_ReturnsRoom()
    {
        var room = CreateTestRoom();
        _repositoryMock.Setup(r => r.GetRoomByCodeAsync("LI1"))
            .ReturnsAsync(room);

        var service = CreateService();
        var result = await service.GetRoomByCodeAsync("LI1");

        result.Should().NotBeNull();
        result!.Code.Should().Be("LI1");
        result.Name.Should().Be("Laboratoire Informatique 1");
    }

    [Fact]
    public async Task GetRoomByCodeAsync_NonExistingRoom_ReturnsNull()
    {
        _repositoryMock.Setup(r => r.GetRoomByCodeAsync("INVALID"))
            .ReturnsAsync((Room?)null);

        var service = CreateService();
        var result = await service.GetRoomByCodeAsync("INVALID");

        result.Should().BeNull();
    }

    [Fact]
    public async Task CreateRoomAsync_ValidData_ReturnsCreatedRoom()
    {
        var floorId = Guid.NewGuid();
        var dto = new CreateRoomDto
        {
            Code = "TEST1",
            Name = "Test Room",
            Description = "Test description",
            FloorId = floorId
        };

        _repositoryMock.Setup(r => r.IsRoomCodeUniqueAsync("TEST1")).ReturnsAsync(true);
        _repositoryMock.Setup(r => r.GetFloorByIdAsync(floorId))
            .ReturnsAsync(new Floor { Id = floorId, Level = 1, BuildingId = Guid.NewGuid() });
        _repositoryMock.Setup(r => r.AddRoomAsync(It.IsAny<Room>()))
            .ReturnsAsync((Room r) => r);
        _repositoryMock.Setup(r => r.GetRoomByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync((Guid id) => new Room { Id = id, Code = "TEST1", Name = "Test Room", FloorId = floorId });

        var service = CreateService();
        var result = await service.CreateRoomAsync(dto, Guid.NewGuid());

        result.Should().NotBeNull();
        result.Code.Should().Be("TEST1");
        result.Name.Should().Be("Test Room");
    }

    [Fact]
    public async Task CreateRoomAsync_DuplicateCode_ThrowsException()
    {
        var dto = new CreateRoomDto
        {
            Code = "LI1",
            Name = "Test Room",
            FloorId = Guid.NewGuid()
        };

        _repositoryMock.Setup(r => r.IsRoomCodeUniqueAsync("LI1"))
            .ReturnsAsync(false);

        var service = CreateService();

        var act = () => service.CreateRoomAsync(dto, Guid.NewGuid());

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*already exists*");
    }

    [Fact]
    public async Task CreateRoomAsync_InvalidFloor_ThrowsException()
    {
        var floorId = Guid.NewGuid();
        var dto = new CreateRoomDto
        {
            Code = "TEST1",
            Name = "Test Room",
            FloorId = floorId
        };

        _repositoryMock.Setup(r => r.IsRoomCodeUniqueAsync("TEST1")).ReturnsAsync(true);
        _repositoryMock.Setup(r => r.GetFloorByIdAsync(floorId))
            .ReturnsAsync((Floor?)null);

        var service = CreateService();

        var act = () => service.CreateRoomAsync(dto, Guid.NewGuid());

        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*not found*");
    }

    private static Site CreateTestSite()
    {
        var siteId = Guid.NewGuid();
        var zoneId = Guid.NewGuid();
        var buildingId = Guid.NewGuid();
        var floorId = Guid.NewGuid();
        var roomId = Guid.NewGuid();

        var site = new Site
        {
            Id = siteId,
            Code = "ISETMA",
            Name = "ISETMA Mahdia",
            Description = "Main campus"
        };

        var zone = new Zone
        {
            Id = zoneId,
            Code = "MAIN",
            Name = "Main Zone",
            SiteId = siteId
        };

        var building = new Building
        {
            Id = buildingId,
            Code = "CENTRAL",
            Name = "Central Building",
            ZoneId = zoneId
        };

        var floor = new Floor
        {
            Id = floorId,
            Level = 1,
            BuildingId = buildingId
        };

        var room = new Room
        {
            Id = roomId,
            Code = "LI1",
            Name = "Laboratoire Informatique 1",
            FloorId = floorId
        };

        floor.Rooms.Add(room);
        building.Floors.Add(floor);
        zone.Buildings.Add(building);
        site.Zones.Add(zone);

        return site;
    }

    private static Room CreateTestRoom()
    {
        var floorId = Guid.NewGuid();
        return new Room
        {
            Id = Guid.NewGuid(),
            Code = "LI1",
            Name = "Laboratoire Informatique 1",
            Description = "Computer lab",
            FloorId = floorId
        };
    }

    [Fact]
    public async Task CreateBuildingAsync_ValidData_ReturnsCreatedBuilding()
    {
        var zoneId = Guid.NewGuid();
        var dto = new CreateBuildingDto
        {
            Code = "NEWBLD",
            Name = "New Building",
            Description = "Test building",
            ZoneId = zoneId
        };

        var zone = new Zone { Id = zoneId, Code = "TEST", Name = "Test Zone" };
        
        _repositoryMock.Setup(r => r.IsBuildingCodeUniqueAsync("NEWBLD")).ReturnsAsync(true);
        _repositoryMock.Setup(r => r.GetZoneByIdAsync(zoneId)).ReturnsAsync(zone);
        _repositoryMock.Setup(r => r.AddBuildingAsync(It.IsAny<Building>()))
            .ReturnsAsync((Building b) => b);

        var service = CreateService();
        var result = await service.CreateBuildingAsync(dto, Guid.NewGuid());

        result.Should().NotBeNull();
        result.Code.Should().Be("NEWBLD");
        result.Name.Should().Be("New Building");
    }

    [Fact]
    public async Task CreateBuildingAsync_DuplicateCode_ThrowsException()
    {
        var dto = new CreateBuildingDto
        {
            Code = "EXISTING",
            Name = "New Building",
            ZoneId = Guid.NewGuid()
        };

        _repositoryMock.Setup(r => r.IsBuildingCodeUniqueAsync("EXISTING"))
            .ReturnsAsync(false);

        var service = CreateService();

        var act = () => service.CreateBuildingAsync(dto, Guid.NewGuid());

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*already exists*");
    }

    [Fact]
    public async Task CreateBuildingAsync_InvalidZone_ThrowsException()
    {
        var zoneId = Guid.NewGuid();
        var dto = new CreateBuildingDto
        {
            Code = "NEWBLD",
            Name = "New Building",
            ZoneId = zoneId
        };

        _repositoryMock.Setup(r => r.IsBuildingCodeUniqueAsync("NEWBLD")).ReturnsAsync(true);
        _repositoryMock.Setup(r => r.GetZoneByIdAsync(zoneId))
            .ReturnsAsync((Zone?)null);

        var service = CreateService();

        var act = () => service.CreateBuildingAsync(dto, Guid.NewGuid());

        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*not found*");
    }

    [Fact]
    public async Task CreateFloorAsync_ValidData_ReturnsCreatedFloor()
    {
        var buildingId = Guid.NewGuid();
        var dto = new CreateFloorDto
        {
            Level = 2,
            Description = "Second floor",
            BuildingId = buildingId
        };

        var building = new Building { Id = buildingId, Code = "BLD1", Name = "Building 1" };
        
        _repositoryMock.Setup(r => r.GetBuildingByIdAsync(buildingId)).ReturnsAsync(building);
        _repositoryMock.Setup(r => r.AddFloorAsync(It.IsAny<Floor>()))
            .ReturnsAsync((Floor f) => f);

        var service = CreateService();
        var result = await service.CreateFloorAsync(dto, Guid.NewGuid());

        result.Should().NotBeNull();
        result.Level.Should().Be(2);
        result.BuildingId.Should().Be(buildingId);
    }

    [Fact]
    public async Task CreateFloorAsync_InvalidBuilding_ThrowsException()
    {
        var buildingId = Guid.NewGuid();
        var dto = new CreateFloorDto
        {
            Level = 1,
            BuildingId = buildingId
        };

        _repositoryMock.Setup(r => r.GetBuildingByIdAsync(buildingId))
            .ReturnsAsync((Building?)null);

        var service = CreateService();

        var act = () => service.CreateFloorAsync(dto, Guid.NewGuid());

        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*not found*");
    }
}

public class LocationCreationTrackingTests : ApplicationTestBase
{
    private readonly Mock<ILocationRepository> _repositoryMock;
    private readonly Mock<IActivityLogService> _activityLogServiceMock;
    private readonly AutoMapper.Mapper _mapper;

    public LocationCreationTrackingTests()
    {
        _repositoryMock = new Mock<ILocationRepository>();
        _activityLogServiceMock = new Mock<IActivityLogService>();

        var config = new AutoMapper.MapperConfiguration(cfg =>
            cfg.AddProfile<SmartInventory.Application.Location.Mappings.LocationMappingProfile>());
        _mapper = new AutoMapper.Mapper(config);
    }

    private LocationService CreateService()
        => new LocationService(_repositoryMock.Object, _activityLogServiceMock.Object, _mapper);

    [Fact]
    public async Task CreateRoomAsync_ShouldLogCreationActivity()
    {
        var floorId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var dto = new CreateRoomDto
        {
            Code = "RM-TRACK",
            Name = "Tracking Room",
            FloorId = floorId
        };

        _repositoryMock.Setup(r => r.IsRoomCodeUniqueAsync(dto.Code)).ReturnsAsync(true);
        _repositoryMock.Setup(r => r.GetFloorByIdAsync(floorId))
            .ReturnsAsync(new Floor { Id = floorId, Level = 1 });
        _repositoryMock.Setup(r => r.AddRoomAsync(It.IsAny<Room>()))
            .ReturnsAsync((Room r) => r);
        _repositoryMock.Setup(r => r.GetRoomByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync((Guid id) => new Room { Id = id, Code = "RM-TRACK", Name = "Tracking Room", FloorId = floorId });

        var service = CreateService();
        var result = await service.CreateRoomAsync(dto, userId);

        result.Should().NotBeNull();
        result.Code.Should().Be("RM-TRACK");
        _activityLogServiceMock.Verify(a => a.TrackFacilityChangeAsync(
            "Created", "Room", "RM-TRACK", "Tracking Room", null, userId), Times.Once);
    }

    [Fact]
    public async Task CreateBuildingAsync_ShouldLogCreationActivity()
    {
        var zoneId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var dto = new CreateBuildingDto
        {
            Code = "BLD-TRACK",
            Name = "Tracking Building",
            ZoneId = zoneId
        };

        _repositoryMock.Setup(r => r.IsBuildingCodeUniqueAsync(dto.Code)).ReturnsAsync(true);
        _repositoryMock.Setup(r => r.GetZoneByIdAsync(zoneId))
            .ReturnsAsync(new Zone { Id = zoneId, Code = "ZONE" });
        _repositoryMock.Setup(r => r.AddBuildingAsync(It.IsAny<Building>()))
            .ReturnsAsync((Building b) => b);

        var service = CreateService();
        var result = await service.CreateBuildingAsync(dto, userId);

        result.Should().NotBeNull();
        result.Code.Should().Be("BLD-TRACK");
        _activityLogServiceMock.Verify(a => a.TrackFacilityChangeAsync(
            "Created", "Building", "BLD-TRACK", "Tracking Building", null, userId), Times.Once);
    }

    [Fact]
    public async Task CreateFloorAsync_ShouldLogCreationActivity()
    {
        var buildingId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var dto = new CreateFloorDto
        {
            Level = 3,
            Description = "Third floor tracking",
            BuildingId = buildingId
        };

        _repositoryMock.Setup(r => r.GetBuildingByIdAsync(buildingId))
            .ReturnsAsync(new Building { Id = buildingId, Code = "BLD1" });
        _repositoryMock.Setup(r => r.AddFloorAsync(It.IsAny<Floor>()))
            .ReturnsAsync((Floor f) => f);

        var service = CreateService();
        var result = await service.CreateFloorAsync(dto, userId);

        result.Should().NotBeNull();
        result.Level.Should().Be(3);
        _activityLogServiceMock.Verify(a => a.TrackFacilityChangeAsync(
            "Created", "Floor", buildingId.ToString(), $"Level 3", null, userId), Times.Once);
    }
}