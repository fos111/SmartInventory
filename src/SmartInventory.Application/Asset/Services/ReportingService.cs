using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SmartInventory.Application.Asset.DTOs;
using SmartInventory.Application.Asset.DTOs.Reports;
using SmartInventory.Application.Asset.Interfaces;
using SmartInventory.Application.Auth.Interfaces;
using SmartInventory.Application.Location.Interfaces;
using SmartInventory.Domain.Auth.Enums;
using AssetEntity = SmartInventory.Domain.Asset.Entities.Asset;
using AssetHistoryEntity = SmartInventory.Domain.Asset.Entities.AssetHistory;

namespace SmartInventory.Application.Asset.Services;

public class ReportingService : IReportingService
{
    private readonly IAssetRepository _assetRepository;
    private readonly IAssetHistoryService _historyService;
    private readonly IActivityLogService _activityLogService;
    private readonly IAuthRepository _authRepository;
    private readonly ILocationRepository _locationRepository;

    public ReportingService(
        IAssetRepository assetRepository,
        IAssetHistoryService historyService,
        IActivityLogService activityLogService,
        IAuthRepository authRepository)
        : this(assetRepository, historyService, activityLogService, authRepository, null!)
    {
    }

    public ReportingService(
        IAssetRepository assetRepository,
        IAssetHistoryService historyService,
        IActivityLogService activityLogService,
        IAuthRepository authRepository,
        ILocationRepository locationRepository)
    {
        _assetRepository = assetRepository;
        _historyService = historyService;
        _activityLogService = activityLogService;
        _authRepository = authRepository;
        _locationRepository = locationRepository;
    }

    public async Task<IEnumerable<InventorySummaryDto>> GetInventorySummaryAsync(string groupBy, Guid? userId, UserRole role)
    {
        var filter = new Asset.Filters.AssetFilter();
        var (assets, _) = await _assetRepository.GetAssetsAsync(filter, 1, int.MaxValue);

        var query = role == UserRole.Supervisor
            ? assets
            : assets.Where(a => a.CurrentRoomCode != null);

        groupBy = groupBy?.ToLowerInvariant() ?? "category";
        return groupBy switch
        {
            "category" => query.GroupBy(a => a.Category)
                .Select(g => new InventorySummaryDto
                {
                    GroupKey = g.Key,
                    GroupLabel = g.Key,
                    Count = g.Count()
                }),
            "status" => query.GroupBy(a => a.Status.ToString())
                .Select(g => new InventorySummaryDto
                {
                    GroupKey = g.Key,
                    GroupLabel = g.Key,
                    Count = g.Count()
                }),
            "location" => query.GroupBy(a => a.CurrentRoomCode)
                .Select(g => new InventorySummaryDto
                {
                    GroupKey = g.Key,
                    GroupLabel = g.Key,
                    Count = g.Count()
                }),
            _ => throw new ArgumentException($"Invalid groupBy value: {groupBy}. Valid values: category, status, location")
        };
    }

    public async Task<IEnumerable<AssetHistoryDto>> GetAssetHistoryAsync(Guid assetId)
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
        });
    }

    public async Task<IEnumerable<ActivityLogDto>> GetActivityLogAsync(DateTime? from, DateTime? to, Guid? userId)
    {
        var filter = new Asset.Filters.AssetFilter();
        var (assets, _) = await _assetRepository.GetAssetsAsync(filter, 1, int.MaxValue);
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
        Dictionary<Guid, Domain.Asset.Entities.Asset> assetDict)
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

    public async Task<IEnumerable<LocationReportDto>> GetLocationReportAsync(Guid? locationId)
    {
        var filter = new Asset.Filters.AssetFilter();
        var (assets, _) = await _assetRepository.GetAssetsAsync(filter, 1, int.MaxValue);

        return assets.GroupBy(a => a.CurrentRoomCode).Select(g => new LocationReportDto
        {
            RoomCode = g.Key,
            TotalAssets = g.Count(),
            ActiveAssets = g.Count(a => a.Status == Domain.Asset.Enums.AssetStatus.Active),
            MaintenanceAssets = g.Count(a => a.Status == Domain.Asset.Enums.AssetStatus.Maintenance),
            RetiredAssets = g.Count(a => a.Status == Domain.Asset.Enums.AssetStatus.Retired),
            Categories = g.Select(a => a.Category).Distinct().ToList()
        });
    }

    // ── Maintenance & Status Reports ──────────────────────────────────

    public async Task<List<MaintenanceForecastDto>> GetMaintenanceForecastAsync(int days)
    {
        var filter = new Asset.Filters.AssetFilter();
        var (assets, _) = await _assetRepository.GetAssetsAsync(filter, 1, int.MaxValue);
        var now = DateTime.UtcNow;
        var cutoff = now.AddDays(days);

        return assets
            .Where(a => a.MaintenanceDueDate.HasValue
                        && a.MaintenanceDueDate.Value >= now
                        && a.MaintenanceDueDate.Value <= cutoff
                        && a.Status != Domain.Asset.Enums.AssetStatus.Retired
                        && a.Status != Domain.Asset.Enums.AssetStatus.Lost)
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
        var filter = new Asset.Filters.AssetFilter();
        var (assets, _) = await _assetRepository.GetAssetsAsync(filter, 1, int.MaxValue);
        var now = DateTime.UtcNow;

        return assets
            .Where(a => a.MaintenanceDueDate.HasValue
                        && a.MaintenanceDueDate.Value < now
                        && a.Status != Domain.Asset.Enums.AssetStatus.Retired
                        && a.Status != Domain.Asset.Enums.AssetStatus.Lost)
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
        var filter = new Asset.Filters.AssetFilter();
        var (assets, _) = await _assetRepository.GetAssetsAsync(filter, 1, int.MaxValue);

        return assets
            .Where(a => a.Status == Domain.Asset.Enums.AssetStatus.CriticalIssue
                        || a.Status == Domain.Asset.Enums.AssetStatus.Lost)
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
        var filter = new Asset.Filters.AssetFilter();
        var (assets, _) = await _assetRepository.GetAssetsAsync(filter, 1, int.MaxValue);
        var total = assets.Count;

        if (total == 0) return new List<StatusSummaryDto>();

        var statuses = new[] { "Active", "Maintenance", "CriticalIssue", "InStock", "Lost", "Retired" };
        return statuses.Select(status =>
        {
            var count = assets.Count(a => a.Status.ToString() == status);
            return new StatusSummaryDto
            {
                Status = status,
                Count = count,
                Percentage = Math.Round((double)count / total * 100, 1)
            };
        }).ToList();
    }

    // ── Location-Based Reports ────────────────────────────────────────

    public async Task<List<ZoneInventoryDto>> GetZoneInventoryAsync()
    {
        var filter = new Asset.Filters.AssetFilter();
        var (assets, _) = await _assetRepository.GetAssetsAsync(filter, 1, int.MaxValue);

        var hierarchy = await _locationRepository.GetFullHierarchyAsync();
        var roomToAssetMap = assets.GroupBy(a => a.CurrentRoomCode)
            .ToDictionary(g => g.Key, g => g.ToList());

        var results = new List<ZoneInventoryDto>();

        foreach (var site in hierarchy)
        {
            foreach (var zone in site.Zones)
            {
                foreach (var building in zone.Buildings)
                {
                    foreach (var floor in building.Floors)
                    {
                        foreach (var room in floor.Rooms)
                        {
                            roomToAssetMap.TryGetValue(room.Code, out var roomAssets);
                            roomAssets ??= new List<AssetEntity>();

                            results.Add(new ZoneInventoryDto
                            {
                                ZoneName = zone.Name,
                                BuildingName = building.Name,
                                FloorLevel = floor.Level,
                                RoomCode = room.Code,
                                TotalAssets = roomAssets.Count,
                                ActiveCount = roomAssets.Count(a => a.Status == Domain.Asset.Enums.AssetStatus.Active),
                                MaintenanceCount = roomAssets.Count(a => a.Status == Domain.Asset.Enums.AssetStatus.Maintenance),
                                CriticalCount = roomAssets.Count(a => a.Status == Domain.Asset.Enums.AssetStatus.CriticalIssue),
                                InStockCount = roomAssets.Count(a => a.Status == Domain.Asset.Enums.AssetStatus.InStock)
                            });
                        }
                    }
                }
            }
        }

        return results;
    }

    public async Task<List<BuildingStocktakeDto>> GetBuildingStocktakeAsync()
    {
        var filter = new Asset.Filters.AssetFilter();
        var (assets, _) = await _assetRepository.GetAssetsAsync(filter, 1, int.MaxValue);

        var hierarchy = await _locationRepository.GetFullHierarchyAsync();
        var roomToAssetMap = assets.GroupBy(a => a.CurrentRoomCode)
            .ToDictionary(g => g.Key, g => g.ToList());

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

                    var buildingAssets = assets.Where(a => roomCodes.Contains(a.CurrentRoomCode)).ToList();

                    results.Add(new BuildingStocktakeDto
                    {
                        BuildingName = building.Name,
                        FloorCount = building.Floors.Count,
                        TotalAssets = buildingAssets.Count,
                        Categories = buildingAssets.Select(a => a.Category).Distinct().OrderBy(c => c).ToList()
                    });
                }
            }
        }

        return results;
    }

    public async Task<RoomAuditDto?> GetRoomAuditAsync(string roomCode)
    {
        var filter = new Asset.Filters.AssetFilter();
        var (assets, _) = await _assetRepository.GetAssetsAsync(filter, 1, int.MaxValue);

        var roomAssets = assets.Where(a =>
            a.CurrentRoomCode.Equals(roomCode, StringComparison.OrdinalIgnoreCase)).ToList();

        if (!roomAssets.Any()) return null;

        var hierarchy = await _locationRepository.GetFullHierarchyAsync();
        string? zoneName = null;
        string? buildingName = null;

        foreach (var site in hierarchy)
        {
            foreach (var zone in site.Zones)
            {
                foreach (var building in zone.Buildings)
                {
                    foreach (var floor in building.Floors)
                    {
                        if (floor.Rooms.Any(r => r.Code.Equals(roomCode, StringComparison.OrdinalIgnoreCase)))
                        {
                            zoneName = zone.Name;
                            buildingName = building.Name;
                            goto found;
                        }
                    }
                }
            }
        }
        found:

        return new RoomAuditDto
        {
            RoomCode = roomCode,
            ZoneName = zoneName,
            BuildingName = buildingName,
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
        var filter = new Asset.Filters.AssetFilter();
        var (assets, _) = await _assetRepository.GetAssetsAsync(filter, 1, int.MaxValue);

        var hierarchy = await _locationRepository.GetFullHierarchyAsync();
        var roomAssetCounts = assets.GroupBy(a => a.CurrentRoomCode)
            .ToDictionary(g => g.Key, g => g.Count());

        var results = new List<EmptyRoomDto>();

        foreach (var site in hierarchy)
        {
            foreach (var zone in site.Zones)
            {
                foreach (var building in zone.Buildings)
                {
                    foreach (var floor in building.Floors)
                    {
                        foreach (var room in floor.Rooms)
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
                    }
                }
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
        var filter = new Asset.Filters.AssetFilter();
        var (assets, _) = await _assetRepository.GetAssetsAsync(filter, 1, int.MaxValue);
        var total = assets.Count;

        if (total == 0) return new List<CategoryStocktakeDto>();

        return assets
            .GroupBy(a => a.Category)
            .Select(g => new CategoryStocktakeDto
            {
                Category = g.Key,
                Count = g.Count(),
                Percentage = Math.Round((double)g.Count() / total * 100, 1),
                StatusBreakdown = g.GroupBy(a => a.Status.ToString())
                    .Select(sg => new StatusCountItem
                    {
                        Status = sg.Key,
                        Count = sg.Count()
                    })
                    .OrderByDescending(s => s.Count)
                    .ToList()
            })
            .OrderByDescending(c => c.Count)
            .ToList();
    }

    // ── Executive Reports ─────────────────────────────────────────────

    public async Task<List<DepartmentReportDto>> GetDepartmentReportsAsync()
    {
        var filter = new Asset.Filters.AssetFilter();
        var (assets, _) = await _assetRepository.GetAssetsAsync(filter, 1, int.MaxValue);

        var hierarchy = await _locationRepository.GetFullHierarchyAsync();
        var roomCodeToZoneMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        foreach (var site in hierarchy)
        {
            foreach (var zone in site.Zones)
            {
                foreach (var building in zone.Buildings)
                {
                    foreach (var floor in building.Floors)
                    {
                        foreach (var room in floor.Rooms)
                        {
                            roomCodeToZoneMap[room.Code] = zone.Name;
                        }
                    }
                }
            }
        }

        var assetsWithZone = assets.Select(a => new
        {
            Asset = a,
            ZoneName = roomCodeToZoneMap.GetValueOrDefault(a.CurrentRoomCode, "Unknown")
        });

        return assetsWithZone
            .GroupBy(a => a.ZoneName)
            .Select(g =>
            {
                var total = g.Count();
                var active = g.Count(a => a.Asset.Status == Domain.Asset.Enums.AssetStatus.Active);
                return new DepartmentReportDto
                {
                    ZoneName = g.Key,
                    TotalAssets = total,
                    AvailabilityRate = total > 0 ? Math.Round((double)active / total * 100, 1) : 0,
                    CriticalCount = g.Count(a => a.Asset.Status == Domain.Asset.Enums.AssetStatus.CriticalIssue),
                    MaintenanceCount = g.Count(a => a.Asset.Status == Domain.Asset.Enums.AssetStatus.Maintenance),
                    InStockCount = g.Count(a => a.Asset.Status == Domain.Asset.Enums.AssetStatus.InStock)
                };
            })
            .OrderByDescending(d => d.TotalAssets)
            .ToList();
    }
}
