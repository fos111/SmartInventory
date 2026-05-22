using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SmartInventory.Application.Asset.DTOs;
using SmartInventory.Application.Asset.DTOs.Reports;
using SmartInventory.Application.Asset.Interfaces;
using SmartInventory.Application.Auth.Interfaces;
using SmartInventory.Application.Caching;
using SmartInventory.Application.Location.Interfaces;
using SmartInventory.Domain.Asset.Enums;
using SmartInventory.Domain.Auth.Enums;
using SmartInventory.Domain.Location.Entities;
using AssetEntity = SmartInventory.Domain.Asset.Entities.Asset;
using AssetHistoryEntity = SmartInventory.Domain.Asset.Entities.AssetHistory;

namespace SmartInventory.Application.Asset.Services;

public class ReportingService : IReportingService
{
    private readonly IAssetRepository _assetRepository;
    private readonly IAssetHistoryService _historyService;
    private readonly IActivityLogService _activityLogService;
    private readonly IAuthRepository _authRepository;
    private readonly ILocationRepository? _locationRepository;
    private readonly ICacheService? _cacheService;

    public ReportingService(
        IAssetRepository assetRepository,
        IAssetHistoryService historyService,
        IActivityLogService activityLogService,
        IAuthRepository authRepository,
        ILocationRepository? locationRepository = null,
        ICacheService? cacheService = null)
    {
        _assetRepository = assetRepository;
        _historyService = historyService;
        _activityLogService = activityLogService;
        _authRepository = authRepository;
        _locationRepository = locationRepository;
        _cacheService = cacheService;
    }

    public async Task<List<InventorySummaryDto>> GetInventorySummaryAsync(
        string groupBy, Guid? userId, UserRole role)
    {
        groupBy = groupBy?.ToLowerInvariant() ?? "category";

        // Non-Supervisor roles must exclude assets without a room code (in-transit/unassigned).
        // Aggregation methods don't support this role-based filter, so load filtered list.
        if (role != UserRole.Supervisor)
        {
            var filter = new Asset.Filters.AssetFilter();
            var assets = await _assetRepository.GetFilteredListAsync(filter);
            var valid = assets.Where(a => a.CurrentRoomCode != null);

            return groupBy switch
            {
                "category" => valid.GroupBy(a => a.Category)
                    .Select(g => new InventorySummaryDto
                        { GroupKey = g.Key, GroupLabel = g.Key, Count = g.Count() }).ToList(),
                "status" => valid.GroupBy(a => a.Status.ToString())
                    .Select(g => new InventorySummaryDto
                        { GroupKey = g.Key, GroupLabel = g.Key, Count = g.Count() }).ToList(),
                "location" => valid.GroupBy(a => a.CurrentRoomCode)
                    .Select(g => new InventorySummaryDto
                        { GroupKey = g.Key, GroupLabel = g.Key, Count = g.Count() }).ToList(),
                _ => throw new ArgumentException(
                    $"Invalid groupBy value: {groupBy}. Valid values: category, status, location")
            };
        }

        return await GetCachedOrFetchAsync($"stats:inventory-summary:{groupBy}", async () =>
        {
            var inventoryResult = groupBy switch
            {
                "category" => (await _assetRepository.GetCategoryCountsAsync())
                    .Select(c => new InventorySummaryDto
                        { GroupKey = c.Category, GroupLabel = c.Category, Count = c.Count }),
                "status" => (await _assetRepository.GetStatusCountsAsync())
                    .Select(s => new InventorySummaryDto
                        { GroupKey = s.Status.ToString(), GroupLabel = s.Status.ToString(), Count = s.Count }),
                "location" => (await _assetRepository.GetLocationCountsAsync())
                    .Select(l => new InventorySummaryDto
                        { GroupKey = l.RoomCode, GroupLabel = l.RoomCode, Count = l.Count }),
                _ => throw new ArgumentException(
                    $"Invalid groupBy value: {groupBy}. Valid values: category, status, location")
            };

            return inventoryResult.ToList();
        });
    }

    public async Task<List<AssetHistoryDto>> GetAssetHistoryAsync(Guid assetId)
    {
        var asset = await _assetRepository.GetByIdAsync(assetId);
        if (asset == null)
            throw new ArgumentException($"Asset with ID {assetId} not found.");

        var history = await _historyService.GetAssetHistoryAsync(assetId);

        return history.Select(h => new AssetHistoryDto
        {
            Id = h.Id,
            AssetId = h.AssetId,
            AssetTag = asset.AssetTag,
            PropertyChanged = h.PropertyChanged,
            OldValue = h.OldValue,
            NewValue = h.NewValue,
            ChangedBy = h.ChangedBy,
            ChangedAt = h.ChangedAt
        }).ToList();
    }

    public async Task<List<ActivityLogDto>> GetActivityLogAsync(
        DateTime? from, DateTime? to, Guid? userId)
    {
        var filter = new Asset.Filters.AssetFilter();
        var assets = await _assetRepository.GetFilteredListAsync(filter);
        var assetDict = assets.ToDictionary(a => a.Id);

        var allHistory = await _historyService.GetAllHistoryAsync(from, to);
        var allActivity = await _activityLogService.GetAllActivityLogsAsync(from, to);

        var results = MapToActivityLogDtos(allHistory, allActivity, assetDict);

        if (userId.HasValue)
            results = results.Where(r => r.ChangedBy == userId.Value).ToList();

        results = results.OrderByDescending(r => r.ChangedAt).ToList();

        await ResolveUserNamesAsync(results);

        return results;
    }

    private static List<ActivityLogDto> MapToActivityLogDtos(
        IEnumerable<AssetHistoryEntity> assetHistory,
        IEnumerable<ActivityLogDto> facilityActivity,
        Dictionary<Guid, AssetEntity> assetDict)
    {
        var results = new List<ActivityLogDto>();

        foreach (var h in assetHistory)
        {
            assetDict.TryGetValue(h.AssetId, out var asset);
            results.Add(new ActivityLogDto
            {
                Id = h.Id,
                AssetId = h.AssetId,
                AssetTag = asset?.AssetTag ?? "Unknown",
                AssetName = asset?.Name ?? "Unknown",
                Action = h.PropertyChanged,
                OldValue = h.OldValue,
                NewValue = h.NewValue,
                ChangedBy = h.ChangedBy,
                ChangedAt = h.ChangedAt
            });
        }

        foreach (var a in facilityActivity)
        {
            results.Add(new ActivityLogDto
            {
                Id = a.Id,
                AssetId = null,
                AssetTag = a.AssetTag,
                AssetName = a.AssetName,
                Action = a.Action,
                OldValue = a.OldValue,
                NewValue = a.NewValue,
                ChangedBy = a.ChangedBy,
                ChangedAt = a.ChangedAt,
                EntityType = a.EntityType,
                EntityId = a.EntityId,
                Details = a.Details
            });
        }

        return results;
    }

    private async Task ResolveUserNamesAsync(List<ActivityLogDto> results)
    {
        var uniqueUserIds = results.Select(r => r.ChangedBy).Distinct().ToList();
        var userCache = new Dictionary<Guid, string>();

        foreach (var uid in uniqueUserIds)
        {
            var user = await _authRepository.GetByIdAsync(uid);
            userCache[uid] = user?.Username ?? "Unknown User";
        }

        foreach (var r in results)
        {
            r.ChangedByName = userCache.GetValueOrDefault(r.ChangedBy, "Unknown User");
        }
    }

    public async Task<List<LocationReportDto>> GetLocationReportAsync(Guid? locationId)
    {
        var roomCounts = await _assetRepository.GetRoomAssetCountsAsync();
        var roomCategories = await _assetRepository.GetRoomCategoriesAsync();

        var categoriesByRoom = roomCategories
            .GroupBy(c => c.RoomCode)
            .ToDictionary(g => g.Key, g => g.Select(c => c.Category).Distinct().ToList());

        return roomCounts
            .GroupBy(r => r.RoomCode)
            .Select(g =>
            {
                var total = g.Sum(r => r.Count);
                return new LocationReportDto
                {
                    RoomCode = g.Key,
                    TotalAssets = total,
                    ActiveAssets = g.Where(r => r.Status == AssetStatus.Active).Sum(r => r.Count),
                    MaintenanceAssets = g.Where(r => r.Status == AssetStatus.Maintenance).Sum(r => r.Count),
                    RetiredAssets = g.Where(r => r.Status == AssetStatus.Retired).Sum(r => r.Count),
                    Categories = categoriesByRoom.GetValueOrDefault(g.Key, new List<string>())
                };
            }).ToList();
    }

    // ── Maintenance & Status Reports ──────────────────────────────────

    public async Task<List<MaintenanceForecastDto>> GetMaintenanceForecastAsync(int days)
    {
        var now = DateTime.UtcNow;
        var cutoff = now.AddDays(days);
        var assets = await _assetRepository.GetAssetsWithMaintenanceAsync(now, cutoff);

        return assets
            .Select(a => new MaintenanceForecastDto
            {
                Id = a.Id,
                AssetTag = a.AssetTag,
                Name = a.Name,
                Category = a.Category,
                Status = a.Status.ToString(),
                CurrentRoomCode = a.CurrentRoomCode,
                MaintenanceDueDate = a.MaintenanceDueDate,
                DaysUntilDue = (int)(a.MaintenanceDueDate!.Value - now).TotalDays
            })
            .OrderBy(a => a.DaysUntilDue)
            .ToList();
    }

    public async Task<List<OverdueMaintenanceDto>> GetOverdueMaintenanceAsync()
    {
        var now = DateTime.UtcNow;
        var assets = await _assetRepository.GetAssetsWithMaintenanceAsync(DateTime.MinValue, now);

        return assets
            .Select(a => new OverdueMaintenanceDto
            {
                Id = a.Id,
                AssetTag = a.AssetTag,
                Name = a.Name,
                Category = a.Category,
                Status = a.Status.ToString(),
                CurrentRoomCode = a.CurrentRoomCode,
                MaintenanceDueDate = a.MaintenanceDueDate,
                DaysOverdue = (int)(now - a.MaintenanceDueDate!.Value).TotalDays
            })
            .OrderByDescending(a => a.DaysOverdue)
            .ToList();
    }

    public async Task<List<CriticalIssueDto>> GetCriticalIssuesAsync()
    {
        var statuses = new[] { AssetStatus.CriticalIssue, AssetStatus.Lost };
        var assets = await _assetRepository.GetAssetsByStatusAsync(statuses);

        return assets
            .Select(a => new CriticalIssueDto
            {
                Id = a.Id,
                AssetTag = a.AssetTag,
                Name = a.Name,
                Category = a.Category,
                Status = a.Status.ToString(),
                CurrentRoomCode = a.CurrentRoomCode,
                LastSeen = a.LastSeen
            })
            .OrderBy(a => a.LastSeen)
            .ToList();
    }

    public async Task<List<StatusSummaryDto>> GetStatusSummaryAsync()
    {
        return await GetCachedOrFetchAsync("stats:status-summary", async () =>
        {
            var statusCounts = await _assetRepository.GetStatusCountsAsync();
            var total = statusCounts.Sum(s => s.Count);

            if (total == 0) return new List<StatusSummaryDto>();

            return statusCounts
                .Select(s => new StatusSummaryDto
                {
                    Status = s.Status.ToString(),
                    Count = s.Count,
                    Percentage = Math.Round((double)s.Count / total * 100, 1)
                })
                .OrderByDescending(s => s.Count)
                .ToList();
        });
    }

    // ── Location-Based Reports ────────────────────────────────────────

    private async Task<T> GetCachedOrFetchAsync<T>(string key, Func<Task<T>> factory, TimeSpan? expiry = null)
        where T : class
    {
        if (_cacheService != null)
        {
            var cached = await _cacheService.GetAsync<T>(key);
            if (cached != null) return cached;
        }

        var result = await factory();

        if (_cacheService != null)
            await _cacheService.SetAsync(key, result, expiry ?? TimeSpan.FromMinutes(5));

        return result;
    }

    private async Task<List<(Zone Zone, Building Building, Floor Floor, Room Room)>> FlattenHierarchyAsync()
    {
        var hierarchy = await _locationRepository!.GetFullHierarchyAsync();
        return hierarchy
            .SelectMany(s => s.Zones, (_, z) => z)
            .SelectMany(z => z.Buildings, (z, b) => (Zone: z, Building: b))
            .SelectMany(t => t.Building.Floors, (t, f) => (t.Zone, t.Building, Floor: f))
            .SelectMany(t => t.Floor.Rooms, (t, r) => (t.Zone, t.Building, t.Floor, Room: r))
            .ToList();
    }

    public async Task<List<ZoneInventoryDto>> GetZoneInventoryAsync()
    {
        var roomCounts = await _assetRepository.GetRoomAssetCountsAsync();
        var countsByRoom = roomCounts
            .GroupBy(r => r.RoomCode)
            .ToDictionary(g => g.Key, g => g.ToList());

        var rooms = await FlattenHierarchyAsync();
        var results = new List<ZoneInventoryDto>(rooms.Count);

        foreach (var (zone, building, floor, room) in rooms)
        {
            countsByRoom.TryGetValue(room.Code, out var roomStatuses);
            roomStatuses ??= new List<RoomAssetCountDto>();

            results.Add(new ZoneInventoryDto
            {
                ZoneName = zone.Name,
                BuildingName = building.Name,
                FloorLevel = floor.Level,
                RoomCode = room.Code,
                TotalAssets = roomStatuses.Sum(r => r.Count),
                ActiveCount = roomStatuses
                    .Where(r => r.Status == AssetStatus.Active).Sum(r => r.Count),
                MaintenanceCount = roomStatuses
                    .Where(r => r.Status == AssetStatus.Maintenance).Sum(r => r.Count),
                CriticalCount = roomStatuses
                    .Where(r => r.Status == AssetStatus.CriticalIssue).Sum(r => r.Count),
                InStockCount = roomStatuses
                    .Where(r => r.Status == AssetStatus.InStock).Sum(r => r.Count)
            });
        }

        return results;
    }

    public async Task<List<BuildingStocktakeDto>> GetBuildingStocktakeAsync()
    {
        var roomCounts = await _assetRepository.GetRoomAssetCountsAsync();
        var countsByRoom = roomCounts
            .GroupBy(r => r.RoomCode)
            .ToDictionary(g => g.Key, g => g.Sum(r => r.Count));

        var roomCategories = await _assetRepository.GetRoomCategoriesAsync();
        var categoriesByRoom = roomCategories
            .GroupBy(c => c.RoomCode)
            .ToDictionary(g => g.Key, g => g.Select(c => c.Category).Distinct().ToList());

        var hierarchy = await _locationRepository!.GetFullHierarchyAsync();
        var results = new List<BuildingStocktakeDto>();

        foreach (var site in hierarchy)
        {
            foreach (var zone in site.Zones)
            {
                foreach (var building in zone.Buildings)
                {
                    var roomCodes = building.Floors
                        .SelectMany(f => f.Rooms)
                        .Select(r => r.Code)
                        .ToHashSet();

                    var buildingCategories = roomCodes
                        .SelectMany(rc => categoriesByRoom.GetValueOrDefault(rc, new List<string>()))
                        .Distinct()
                        .OrderBy(c => c)
                        .ToList();

                    var buildingTotal = roomCodes
                        .Sum(rc => countsByRoom.GetValueOrDefault(rc, 0));

                    results.Add(new BuildingStocktakeDto
                    {
                        BuildingName = building.Name,
                        FloorCount = building.Floors.Count,
                        TotalAssets = buildingTotal,
                        Categories = buildingCategories
                    });
                }
            }
        }

        return results;
    }

    public async Task<RoomAuditDto?> GetRoomAuditAsync(string roomCode)
    {
        var filter = new Asset.Filters.AssetFilter { RoomCode = roomCode };
        var roomAssets = await _assetRepository.GetFilteredListAsync(filter);

        if (roomAssets.Count == 0) return null;

        var (zone, building, _, _) = (await FlattenHierarchyAsync())
            .FirstOrDefault(r => r.Room.Code.Equals(roomCode, StringComparison.OrdinalIgnoreCase));

        return new RoomAuditDto
        {
            RoomCode = roomCode,
            ZoneName = zone?.Name,
            BuildingName = building?.Name,
            Assets = roomAssets.Select(a => new RoomAssetItem
            {
                AssetTag = a.AssetTag,
                Name = a.Name,
                Category = a.Category,
                Status = a.Status.ToString(),
                RfidTagId = a.RfidTagId,
                LastSeen = a.LastSeen
            }).ToList()
        };
    }

    public async Task<List<EmptyRoomDto>> GetEmptyRoomsAsync(int threshold = 0)
    {
        var roomCounts = await _assetRepository.GetRoomAssetCountsAsync();
        var roomAssetCounts = roomCounts
            .GroupBy(r => r.RoomCode)
            .ToDictionary(g => g.Key, g => g.Sum(r => r.Count));

        var rooms = await FlattenHierarchyAsync();
        var results = new List<EmptyRoomDto>(rooms.Count);

        foreach (var (zone, building, _, room) in rooms)
        {
            roomAssetCounts.TryGetValue(room.Code, out var count);
            if (count <= threshold)
            {
                results.Add(new EmptyRoomDto
                {
                    RoomCode = room.Code,
                    ZoneName = zone.Name,
                    BuildingName = building.Name,
                    AssetCount = count
                });
            }
        }

        return results.OrderBy(r => r.AssetCount).ThenBy(r => r.RoomCode).ToList();
    }

    // ── Audit & Compliance Reports ────────────────────────────────────

    public async Task<List<LocationDiscrepancyDto>> GetLocationDiscrepanciesAsync()
    {
        var discrepant = await _assetRepository.GetDiscrepantAssetsAsync();

        return discrepant
            .Where(a => !string.IsNullOrEmpty(a.DetectedRoomCode)
                        && !a.CurrentRoomCode.Equals(a.DetectedRoomCode, StringComparison.OrdinalIgnoreCase))
            .Select(a => new LocationDiscrepancyDto
            {
                Id = a.Id,
                AssetTag = a.AssetTag,
                Name = a.Name,
                Category = a.Category,
                CurrentRoomCode = a.CurrentRoomCode,
                DetectedRoomCode = a.DetectedRoomCode
            })
            .ToList();
    }

    public async Task<List<CategoryStocktakeDto>> GetCategoryStocktakeAsync()
    {
        return await GetCachedOrFetchAsync("stats:category-stocktake", async () =>
        {
            var breakdown = await _assetRepository.GetCategoryStatusBreakdownAsync();
            var total = breakdown.Sum(b => b.Count);

            if (total == 0) return new List<CategoryStocktakeDto>();

            return breakdown
                .GroupBy(b => b.Category)
                .Select(g => new CategoryStocktakeDto
                {
                    Category = g.Key,
                    Count = g.Sum(b => b.Count),
                    Percentage = Math.Round((double)g.Sum(b => b.Count) / total * 100, 1),
                    StatusBreakdown = g
                        .Select(b => new StatusCountItem
                        {
                            Status = b.Status.ToString(),
                            Count = b.Count
                        })
                        .OrderByDescending(s => s.Count)
                        .ToList()
                })
                .OrderByDescending(c => c.Count)
                .ToList();
        });
    }

    // ── Executive Reports ─────────────────────────────────────────────

    public async Task<List<DepartmentReportDto>> GetDepartmentReportsAsync()
    {
        var roomCounts = await _assetRepository.GetRoomAssetCountsAsync();
        var countsByRoom = roomCounts
            .GroupBy(r => r.RoomCode)
            .ToDictionary(g => g.Key, g => g.ToList());

        var hierarchy = await _locationRepository!.GetFullHierarchyAsync();
        var results = new List<DepartmentReportDto>();

        foreach (var site in hierarchy)
        {
            foreach (var zone in site.Zones)
            {
                var roomCodes = zone.Buildings
                    .SelectMany(b => b.Floors)
                    .SelectMany(f => f.Rooms)
                    .Select(r => r.Code)
                    .ToHashSet();

                var zoneStatuses = roomCodes
                    .SelectMany(rc => countsByRoom.GetValueOrDefault(rc, new List<RoomAssetCountDto>()));

                var total = zoneStatuses.Sum(z => z.Count);

                if (total == 0) continue;

                results.Add(new DepartmentReportDto
                {
                    ZoneName = zone.Name,
                    TotalAssets = total,
                    AvailabilityRate = Math.Round(
                        (double)zoneStatuses.Where(z => z.Status == AssetStatus.Active).Sum(z => z.Count)
                        / total * 100, 1),
                    CriticalCount = zoneStatuses
                        .Where(z => z.Status == AssetStatus.CriticalIssue).Sum(z => z.Count),
                    MaintenanceCount = zoneStatuses
                        .Where(z => z.Status == AssetStatus.Maintenance).Sum(z => z.Count),
                    InStockCount = zoneStatuses
                        .Where(z => z.Status == AssetStatus.InStock).Sum(z => z.Count)
                });
            }
        }

        return results.OrderByDescending(d => d.TotalAssets).ToList();
    }

    public async Task<LocationComprehensiveReportDto?> GetLocationReportAsync(
        string scope, string scopeId, Guid? userId, UserRole role)
    {
        var hierarchy = await _locationRepository!.GetFullHierarchyAsync();
        var (roomCodes, info) = ResolveLocationScope(scope, scopeId, hierarchy);
        if (roomCodes == null) return null;

        var allAssets = await _assetRepository.GetFilteredListAsync(new Asset.Filters.AssetFilter());
        var currentAssets = allAssets
            .Where(a => roomCodes.Contains(a.CurrentRoomCode, StringComparer.OrdinalIgnoreCase))
            .ToList();

        var assetIds = currentAssets.Select(a => a.Id).ToHashSet();
        var assetHistory = await _historyService.GetByAssetIdsAsync(assetIds);
        var locationHistory = await _locationRepository!.GetLocationHistoryByRoomCodesAsync(roomCodes);
        var allActivity = await _activityLogService.GetAllActivityLogsAsync();
        var userCache = new Dictionary<Guid, string>();

        var historyEntries = await ComposeHistoryAsync(
            currentAssets, assetHistory, locationHistory, allActivity, roomCodes, userCache);

        return new LocationComprehensiveReportDto
        {
            Scope = scope,
            ScopeId = scopeId,
            ScopeName = scope.ToLowerInvariant() switch
            {
                "zone" => info.ZoneName ?? scopeId,
                "building" => info.BuildingName ?? scopeId,
                "room" => info.RoomName ?? info.RoomCode ?? scopeId,
                _ => scopeId
            },
            Hierarchy = info,
            TotalAssets = currentAssets.Count,
            CurrentAssets = currentAssets.Select(MapAssetItem).ToList(),
            History = historyEntries,
            GeneratedAt = DateTime.UtcNow
        };
    }

    private static (List<string>? RoomCodes, LocationHierarchyInfo Info) ResolveLocationScope(
        string scope, string scopeId, List<Site> hierarchy)
    {
        var info = new LocationHierarchyInfo();

        switch (scope.ToLowerInvariant())
        {
            case "room":
            {
                var found = FindRoomInHierarchy(hierarchy, scopeId);
                if (found == null) return (null, info);

                var (room, floor, building, zone, site) = found.Value;
                info.SiteName = site?.Name;
                info.ZoneName = zone?.Name;
                info.BuildingName = building?.Name;
                info.FloorLevel = floor?.Level;
                info.RoomCode = room.Code;
                info.RoomName = room.Name;
                info.RoomDescription = room.Description;

                return (new List<string> { room.Code }, info);
            }

            case "building":
            {
                var found = FindBuildingInHierarchy(hierarchy, scopeId);
                if (found == null) return (null, info);

                var (building, zone, site) = found.Value;
                info.SiteName = site?.Name;
                info.ZoneName = zone?.Name;
                info.BuildingName = building.Name;

                var roomCodes = building.Floors
                    .SelectMany(f => f.Rooms)
                    .Select(r => r.Code)
                    .Distinct()
                    .ToList();
                return (roomCodes, info);
            }

            case "zone":
            {
                var found = FindZoneInHierarchy(hierarchy, scopeId);
                if (found == null) return (null, info);

                var (zone, site) = found.Value;
                info.SiteName = site?.Name;
                info.ZoneName = zone.Name;

                var roomCodes = zone.Buildings
                    .SelectMany(b => b.Floors)
                    .SelectMany(f => f.Rooms)
                    .Select(r => r.Code)
                    .Distinct()
                    .ToList();
                return (roomCodes, info);
            }

            default:
                throw new ArgumentException($"Invalid scope: {scope}. Valid values: zone, building, room");
        }
    }

    private static (Room Room, Floor? Floor, Building? Building, Zone? Zone, Site? Site)? FindRoomInHierarchy(
        List<Site> hierarchy, string roomCode)
    {
        var rooms = hierarchy
            .SelectMany(s => s.Zones)
            .SelectMany(z => z.Buildings)
            .SelectMany(b => b.Floors)
            .SelectMany(f => f.Rooms);

        var room = rooms.FirstOrDefault(r => r.Code.Equals(roomCode, StringComparison.OrdinalIgnoreCase));
        if (room == null) return null;

        var floor = hierarchy
            .SelectMany(s => s.Zones)
            .SelectMany(z => z.Buildings)
            .SelectMany(b => b.Floors)
            .FirstOrDefault(f => f.Rooms.Any(r => r.Id == room.Id));

        var building = hierarchy
            .SelectMany(s => s.Zones)
            .SelectMany(z => z.Buildings)
            .FirstOrDefault(b => b.Floors.Any(f => f.Rooms.Any(r => r.Id == room.Id)));

        var zone = hierarchy
            .SelectMany(s => s.Zones)
            .FirstOrDefault(z => z.Buildings.Any(b => b.Floors.Any(f => f.Rooms.Any(r => r.Id == room.Id))));

        var site = hierarchy.FirstOrDefault(s => s.Zones.Any(z => z.Id == zone?.Id));

        return (room, floor, building, zone, site);
    }

    private static (Building Building, Zone? Zone, Site? Site)? FindBuildingInHierarchy(
        List<Site> hierarchy, string buildingId)
    {
        var building = hierarchy
            .SelectMany(s => s.Zones)
            .SelectMany(z => z.Buildings)
            .FirstOrDefault(b => b.Id.ToString() == buildingId);

        if (building == null) return null;

        var zone = hierarchy
            .SelectMany(s => s.Zones)
            .FirstOrDefault(z => z.Buildings.Any(b => b.Id == building.Id));

        var site = hierarchy.FirstOrDefault(s => s.Zones.Any(z => z.Id == zone?.Id));

        return (building, zone, site);
    }

    private static (Zone Zone, Site? Site)? FindZoneInHierarchy(
        List<Site> hierarchy, string zoneId)
    {
        var zone = hierarchy
            .SelectMany(s => s.Zones)
            .FirstOrDefault(z => z.Id.ToString() == zoneId);

        if (zone == null) return null;

        var site = hierarchy.FirstOrDefault(s => s.Zones.Any(z => z.Id == zone.Id));

        return (zone, site);
    }

    private static LocationRoomAssetItem MapAssetItem(AssetEntity a)
    {
        return new LocationRoomAssetItem
        {
            AssetTag = a.AssetTag,
            Name = a.Name,
            Category = a.Category,
            Status = a.Status,
            SerialNumber = a.SerialNumber,
            RfidTagId = a.RfidTagId,
            BleId = a.BleId,
            Manufacturer = a.Manufacturer,
            Model = a.Model,
            LastSeen = a.LastSeen,
            InstallDate = a.InstallDate,
            MaintenanceDueDate = a.MaintenanceDueDate
        };
    }

    private async Task<List<LocationHistoryEntry>> ComposeHistoryAsync(
        List<AssetEntity> currentAssets,
        IEnumerable<AssetHistoryEntity> assetHistory,
        List<AssetLocationHistory> locationHistory,
        IEnumerable<ActivityLogDto> allActivity,
        List<string> roomCodes,
        Dictionary<Guid, string> userCache)
    {
        var entries = new List<LocationHistoryEntry>();

        var assetDict = currentAssets.ToDictionary(a => a.Id);

        foreach (var h in assetHistory)
        {
            assetDict.TryGetValue(h.AssetId, out var asset);
            entries.Add(new LocationHistoryEntry
            {
                Timestamp = h.ChangedAt,
                EventType = h.PropertyChanged,
                AssetTag = asset?.AssetTag ?? "Unknown",
                AssetName = asset?.Name ?? "Unknown",
                OldValue = h.OldValue,
                NewValue = h.NewValue,
                ChangedBy = await ResolveUserNameAsync(h.ChangedBy, userCache),
                Source = "user"
            });
        }

        foreach (var h in locationHistory)
        {
            assetDict.TryGetValue(h.AssetId, out var asset);
            entries.Add(new LocationHistoryEntry
            {
                Timestamp = h.CreatedAt,
                EventType = h.PreviousRoomCode == null ? "asset_assigned" : "asset_moved",
                AssetTag = asset?.AssetTag ?? "Unknown",
                AssetName = asset?.Name ?? "Unknown",
                OldValue = h.PreviousRoomCode,
                NewValue = h.NewRoomCode,
                ChangedBy = h.Source == "iot" ? "IoT System" : "User",
                Source = h.Source
            });
        }

        foreach (var a in allActivity.Where(a =>
            roomCodes.Contains(a.OldValue, StringComparer.OrdinalIgnoreCase) ||
            roomCodes.Contains(a.NewValue, StringComparer.OrdinalIgnoreCase)))
        {
            entries.Add(new LocationHistoryEntry
            {
                Timestamp = a.ChangedAt,
                EventType = a.Action,
                AssetTag = a.AssetTag ?? "",
                AssetName = a.AssetName ?? "",
                OldValue = a.OldValue,
                NewValue = a.NewValue,
                ChangedBy = a.ChangedByName ?? await ResolveUserNameAsync(a.ChangedBy, userCache),
                Source = a.EntityType ?? "user"
            });
        }

        return entries.OrderByDescending(e => e.Timestamp).ToList();
    }

    private async Task<string> ResolveUserNameAsync(Guid userId, Dictionary<Guid, string> cache)
    {
        if (cache.TryGetValue(userId, out var name))
            return name;

        var user = await _authRepository.GetByIdAsync(userId);
        name = user?.Username ?? "Unknown User";
        cache[userId] = name;
        return name;
    }
}
