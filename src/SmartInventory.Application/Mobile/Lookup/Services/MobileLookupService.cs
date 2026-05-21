using SmartInventory.Application.Asset.Interfaces;
using SmartInventory.Application.Asset.Services;
using SmartInventory.Application.Location.Interfaces;
using SmartInventory.Application.Mobile.Lookup.DTOs;
using SmartInventory.Application.Mobile.Lookup.Interfaces;

namespace SmartInventory.Application.Mobile.Lookup.Services;

public class MobileLookupService : IMobileLookupService
{
    private readonly ILocationService _locationService;
    private readonly ILocationRepository _locationRepository;
    private readonly IReportingService _reportingService;
    private readonly IAssetService _assetService;
    private readonly CategoryService _categoryService;

    public MobileLookupService(
        ILocationService locationService,
        ILocationRepository locationRepository,
        IReportingService reportingService,
        IAssetService assetService,
        CategoryService categoryService)
    {
        _locationService = locationService;
        _locationRepository = locationRepository;
        _reportingService = reportingService;
        _assetService = assetService;
        _categoryService = categoryService;
    }

    public Task<IEnumerable<MobileCategoryDto>> GetCategoriesAsync(CancellationToken ct = default)
    {
        var categories = _categoryService.GetAllCategories();
        var result = categories.Select(c => new MobileCategoryDto
        {
            Name = c.Name,
            Group = c.Group
        });

        return Task.FromResult(result);
    }

    public async Task<IEnumerable<MobileDepartmentDto>> GetDepartmentsAsync(CancellationToken ct = default)
    {
        var hierarchy = await _locationService.GetHierarchyAsync();

        if (hierarchy.Site.Zones.Count == 0)
            return Enumerable.Empty<MobileDepartmentDto>();

        var departments = hierarchy.Site.Zones.Select(zone =>
        {
            var roomCount = zone.Buildings
                .Sum(building => building.Floors
                    .Sum(floor => floor.Rooms.Count));

            return new MobileDepartmentDto
            {
                Id = zone.Id,
                Code = zone.Code,
                Name = zone.Name,
                RoomCount = roomCount
            };
        });

        return departments;
    }

    public async Task<IEnumerable<MobileRoomDto>> GetRoomsByDepartmentAsync(Guid zoneId, CancellationToken ct = default)
    {
        var hierarchy = await _locationService.GetHierarchyAsync();

        var zone = hierarchy.Site.Zones
            .FirstOrDefault(z => z.Id == zoneId);

        if (zone == null)
            return Enumerable.Empty<MobileRoomDto>();

        var rooms = zone.Buildings
            .SelectMany(building => building.Floors
                .SelectMany(floor => floor.Rooms
                    .Select(room => new MobileRoomDto
                    {
                        Id = room.Id,
                        Code = room.Code,
                        Name = room.Name,
                        FloorLevel = room.FloorLevel,
                        BuildingName = building.Name,
                        ZoneName = zone.Name
                    })));

        return rooms;
    }

    public async Task<IEnumerable<MobileRoomDto>> GetRoomsByDepartmentCodeAsync(string code, CancellationToken ct = default)
    {
        var zone = await _locationRepository.GetZoneByCodeAsync(code);

        if (zone == null)
            return Enumerable.Empty<MobileRoomDto>();

        var rooms = zone.Buildings
            .SelectMany(building => building.Floors
                .SelectMany(floor => floor.Rooms
                    .Select(room => new MobileRoomDto
                    {
                        Id = room.Id,
                        Code = room.Code,
                        Name = room.Name,
                        FloorLevel = floor.Level,
                        BuildingName = building.Name,
                        ZoneName = zone.Name
                    })));

        return rooms;
    }

    public async Task<MobileInventoryStatsDto> GetStatsAsync(CancellationToken ct = default)
    {
        var statusSummary = await _reportingService.GetStatusSummaryAsync();

        return new MobileInventoryStatsDto
        {
            InStock = statusSummary.FirstOrDefault(s => s.Status == "Active")?.Count ?? 0,
            Maintenance = statusSummary.FirstOrDefault(s => s.Status == "Maintenance")?.Count ?? 0,
            Critical = statusSummary.FirstOrDefault(s => s.Status == "CriticalIssue")?.Count ?? 0,
            Lost = statusSummary.FirstOrDefault(s => s.Status == "Lost")?.Count ?? 0,
            Retired = statusSummary.FirstOrDefault(s => s.Status == "Retired")?.Count ?? 0
        };
    }

    public async Task<IEnumerable<MobileMoveLogEntryDto>> GetMoveLogAsync(
        DateTime? from = null, DateTime? to = null, CancellationToken ct = default)
    {
        var activity = await _reportingService.GetActivityLogAsync(from, to, null);

        var result = activity.Select(a => new MobileMoveLogEntryDto
        {
            Id = a.Id,
            AssetTag = a.AssetTag,
            AssetName = a.AssetName,
            OldValue = a.OldValue,
            NewValue = a.NewValue,
            ChangedByName = a.ChangedByName,
            ChangedAt = a.ChangedAt
        });

        return result;
    }

    public async Task<BarcodeCheckResultDto> CheckBarcodeAsync(string barcode, CancellationToken ct = default)
    {
        if (string.IsNullOrEmpty(barcode))
        {
            return new BarcodeCheckResultDto
            {
                Exists = false,
                AssetTag = null,
                Name = null
            };
        }

        var asset = await _assetService.GetAssetByTagAsync(barcode);

        if (asset == null)
        {
            return new BarcodeCheckResultDto
            {
                Exists = false,
                AssetTag = null,
                Name = null
            };
        }

        return new BarcodeCheckResultDto
        {
            Exists = true,
            AssetTag = asset.AssetTag,
            Name = asset.Name
        };
    }
}
