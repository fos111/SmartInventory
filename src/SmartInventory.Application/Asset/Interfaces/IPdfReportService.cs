using SmartInventory.Application.Asset.DTOs.Reports;

namespace SmartInventory.Application.Asset.Interfaces;

public interface IPdfReportService
{
    byte[] GenerateMaintenanceForecast(List<MaintenanceForecastDto> data, int days);
    byte[] GenerateOverdueMaintenance(List<OverdueMaintenanceDto> data);
    byte[] GenerateCriticalIssues(List<CriticalIssueDto> data);
    byte[] GenerateStatusSummary(List<StatusSummaryDto> data);
    byte[] GenerateZoneInventory(List<ZoneInventoryDto> data);
    byte[] GenerateBuildingStocktake(List<BuildingStocktakeDto> data);
    byte[] GenerateRoomAudit(RoomAuditDto data);
    byte[] GenerateEmptyRooms(List<EmptyRoomDto> data);
    byte[] GenerateLocationDiscrepancies(List<LocationDiscrepancyDto> data);
    byte[] GenerateCategoryStocktake(List<CategoryStocktakeDto> data);
    byte[] GenerateDepartmentReport(List<DepartmentReportDto> data);
}
