using System.Threading;
using System.Threading.Tasks;
using SmartInventory.Application.Asset.DTOs.Reports;
using SmartInventory.Application.Mobile.Reports.DTOs;

namespace SmartInventory.Application.Asset.Interfaces;

public interface IPdfReportService
{
    Task<byte[]> GenerateMaintenanceForecastAsync(List<MaintenanceForecastDto> data, int days, CancellationToken ct = default);
    Task<byte[]> GenerateOverdueMaintenanceAsync(List<OverdueMaintenanceDto> data, CancellationToken ct = default);
    Task<byte[]> GenerateCriticalIssuesAsync(List<CriticalIssueDto> data, CancellationToken ct = default);
    Task<byte[]> GenerateStatusSummaryAsync(List<StatusSummaryDto> data, CancellationToken ct = default);
    Task<byte[]> GenerateZoneInventoryAsync(List<ZoneInventoryDto> data, CancellationToken ct = default);
    Task<byte[]> GenerateBuildingStocktakeAsync(List<BuildingStocktakeDto> data, CancellationToken ct = default);
    Task<byte[]> GenerateRoomAuditAsync(RoomAuditDto data, CancellationToken ct = default);
    Task<byte[]> GenerateEmptyRoomsAsync(List<EmptyRoomDto> data, CancellationToken ct = default);
    Task<byte[]> GenerateLocationDiscrepanciesAsync(List<LocationDiscrepancyDto> data, CancellationToken ct = default);
    Task<byte[]> GenerateCategoryStocktakeAsync(List<CategoryStocktakeDto> data, CancellationToken ct = default);
    Task<byte[]> GenerateDepartmentReportAsync(List<DepartmentReportDto> data, CancellationToken ct = default);

    Task<byte[]> GenerateRoomJournalAsync(RoomJournalDto data, CancellationToken ct = default);
    Task<byte[]> GenerateLocationReportAsync(LocationComprehensiveReportDto data, CancellationToken ct = default);
}
