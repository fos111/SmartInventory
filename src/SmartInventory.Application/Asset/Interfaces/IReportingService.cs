using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SmartInventory.Application.Asset.DTOs;
using SmartInventory.Application.Asset.DTOs.Reports;
using SmartInventory.Domain.Auth.Enums;

namespace SmartInventory.Application.Asset.Interfaces;

public interface IReportingService
{
    Task<List<InventorySummaryDto>> GetInventorySummaryAsync(string groupBy, Guid? userId, UserRole role);
    Task<List<AssetHistoryDto>> GetAssetHistoryAsync(Guid assetId);
    Task<List<ActivityLogDto>> GetActivityLogAsync(DateTime? from, DateTime? to, Guid? userId);
    Task<List<LocationReportDto>> GetLocationReportAsync(Guid? locationId);

    // Maintenance & Status Reports
    Task<List<MaintenanceForecastDto>> GetMaintenanceForecastAsync(int days);
    Task<List<OverdueMaintenanceDto>> GetOverdueMaintenanceAsync();
    Task<List<CriticalIssueDto>> GetCriticalIssuesAsync();
    Task<List<StatusSummaryDto>> GetStatusSummaryAsync();

    // Location-Based Reports
    Task<List<ZoneInventoryDto>> GetZoneInventoryAsync();
    Task<List<BuildingStocktakeDto>> GetBuildingStocktakeAsync();
    Task<RoomAuditDto?> GetRoomAuditAsync(string roomCode);
    Task<List<EmptyRoomDto>> GetEmptyRoomsAsync(int threshold = 0);

    // Audit & Compliance Reports
    Task<List<LocationDiscrepancyDto>> GetLocationDiscrepanciesAsync();
    Task<List<CategoryStocktakeDto>> GetCategoryStocktakeAsync();

    // Executive Reports
    Task<List<DepartmentReportDto>> GetDepartmentReportsAsync();

    // Location-Based Comprehensive Report
    Task<LocationComprehensiveReportDto?> GetLocationReportAsync(
        string scope, string scopeId, Guid? userId, Domain.Auth.Enums.UserRole role);
}
