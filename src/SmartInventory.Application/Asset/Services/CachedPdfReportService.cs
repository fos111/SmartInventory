using System;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using SmartInventory.Application.Asset.DTOs.Reports;
using SmartInventory.Application.Asset.Interfaces;
using SmartInventory.Application.Caching;
using SmartInventory.Application.Mobile.Reports.DTOs;

namespace SmartInventory.Application.Asset.Services;

public class CachedPdfReportService : IPdfReportService
{
    private readonly IPdfReportService _inner;
    private readonly IBlobCacheService? _cacheService;

    public CachedPdfReportService(IPdfReportService inner, IBlobCacheService? cacheService = null)
    {
        _inner = inner;
        _cacheService = cacheService;
    }

    public async Task<byte[]> GenerateMaintenanceForecastAsync(List<MaintenanceForecastDto> data, int days, CancellationToken ct = default)
    {
        return await CacheAsync($"reports/maintenance/forecast-{days}",
            () => _inner.GenerateMaintenanceForecastAsync(data, days, ct), ct);
    }

    public async Task<byte[]> GenerateOverdueMaintenanceAsync(List<OverdueMaintenanceDto> data, CancellationToken ct = default)
    {
        return await CacheAsync("reports/maintenance/overdue",
            () => _inner.GenerateOverdueMaintenanceAsync(data, ct), ct);
    }

    public async Task<byte[]> GenerateCriticalIssuesAsync(List<CriticalIssueDto> data, CancellationToken ct = default)
    {
        return await CacheAsync("reports/critical-issues",
            () => _inner.GenerateCriticalIssuesAsync(data, ct), ct);
    }

    public async Task<byte[]> GenerateStatusSummaryAsync(List<StatusSummaryDto> data, CancellationToken ct = default)
    {
        return await CacheAsync("reports/status-summary",
            () => _inner.GenerateStatusSummaryAsync(data, ct), ct);
    }

    public async Task<byte[]> GenerateZoneInventoryAsync(List<ZoneInventoryDto> data, CancellationToken ct = default)
    {
        return await CacheAsync("reports/zone-inventory",
            () => _inner.GenerateZoneInventoryAsync(data, ct), ct);
    }

    public async Task<byte[]> GenerateBuildingStocktakeAsync(List<BuildingStocktakeDto> data, CancellationToken ct = default)
    {
        return await CacheAsync("reports/building-stocktake",
            () => _inner.GenerateBuildingStocktakeAsync(data, ct), ct);
    }

    public async Task<byte[]> GenerateRoomAuditAsync(RoomAuditDto data, CancellationToken ct = default)
    {
        return await CacheAsync($"reports/fiche/{data.RoomCode}",
            () => _inner.GenerateRoomAuditAsync(data, ct), ct);
    }

    public async Task<byte[]> GenerateEmptyRoomsAsync(List<EmptyRoomDto> data, CancellationToken ct = default)
    {
        return await CacheAsync("reports/empty-rooms",
            () => _inner.GenerateEmptyRoomsAsync(data, ct), ct);
    }

    public async Task<byte[]> GenerateLocationDiscrepanciesAsync(List<LocationDiscrepancyDto> data, CancellationToken ct = default)
    {
        return await CacheAsync("reports/location-discrepancies",
            () => _inner.GenerateLocationDiscrepanciesAsync(data, ct), ct);
    }

    public async Task<byte[]> GenerateCategoryStocktakeAsync(List<CategoryStocktakeDto> data, CancellationToken ct = default)
    {
        return await CacheAsync("reports/category-stocktake",
            () => _inner.GenerateCategoryStocktakeAsync(data, ct), ct);
    }

    public async Task<byte[]> GenerateDepartmentReportAsync(List<DepartmentReportDto> data, CancellationToken ct = default)
    {
        return await CacheAsync("reports/department-report",
            () => _inner.GenerateDepartmentReportAsync(data, ct), ct);
    }

    public async Task<byte[]> GenerateRoomJournalAsync(RoomJournalDto data, CancellationToken ct = default)
    {
        return await CacheAsync($"reports/journal/{data.RoomCode}",
            () => _inner.GenerateRoomJournalAsync(data, ct), ct);
    }

    public async Task<byte[]> GenerateLocationReportAsync(LocationComprehensiveReportDto data, CancellationToken ct = default)
    {
        return await CacheAsync($"reports/location/{data.Scope}/{data.ScopeId}",
            () => _inner.GenerateLocationReportAsync(data, ct), ct);
    }

    private async Task<byte[]> CacheAsync(string key, Func<Task<byte[]>> generator, CancellationToken ct)
    {
        if (_cacheService == null)
            return await generator();

        var cached = await _cacheService.GetAsync(key, ct);
        if (cached != null) return cached;

        var bytes = await generator();
        await _cacheService.SetAsync(key, bytes, "application/pdf", ct);
        return bytes;
    }
}
