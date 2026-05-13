using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartInventory.Application.Asset.DTOs;
using SmartInventory.Application.Asset.Interfaces;
using SmartInventory.Application.Asset.Services;
using SmartInventory.Domain.Auth.Enums;

namespace SmartInventory.API.Controllers
{
    [ApiController]
    [Route("api/reports")]
    [Authorize]
    public class ReportsController : ControllerBase
    {
        private readonly IReportingService _reportingService;
        private readonly IPdfReportService _pdfReportService;
        private readonly CsvExportService _csvExportService;
        private readonly PdfExportService _pdfExportService;

        public ReportsController(
            IReportingService reportingService,
            IPdfReportService pdfReportService,
            CsvExportService csvExportService,
            PdfExportService pdfExportService)
        {
            _reportingService = reportingService;
            _pdfReportService = pdfReportService;
            _csvExportService = csvExportService;
            _pdfExportService = pdfExportService;
        }

        private Guid GetUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return Guid.TryParse(userIdClaim, out var userId) ? userId : Guid.Empty;
        }

        private UserRole GetUserRole()
        {
            var roleClaim = User.FindFirst(ClaimTypes.Role)?.Value;
            return Enum.TryParse<UserRole>(roleClaim, out var role) ? role : UserRole.Technicien;
        }

        [HttpGet("summary")]
        public async Task<ActionResult> GetSummary([FromQuery] string groupBy = "category")
        {
            if (string.IsNullOrEmpty(groupBy))
                groupBy = "category";
            
            var validGroupBy = new[] { "category", "status", "location" };
            if (!validGroupBy.Contains(groupBy.ToLowerInvariant()))
                return BadRequest(new { message = "Invalid groupBy value. Valid values: category, status, location" });

            try
            {
                var summary = await _reportingService.GetInventorySummaryAsync(groupBy, GetUserId(), GetUserRole());
                return Ok(summary);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("asset/{assetId:guid}/history")]
        public async Task<ActionResult> GetAssetHistory(Guid assetId)
        {
            try
            {
                var history = await _reportingService.GetAssetHistoryAsync(assetId);
                return Ok(history);
            }
            catch (ArgumentException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        [HttpGet("activity")]
        public async Task<ActionResult> GetActivity(
            [FromQuery] DateTime? from,
            [FromQuery] DateTime? to)
        {
            var activity = await _reportingService.GetActivityLogAsync(from, to, GetUserId());
            return Ok(activity);
        }

        [HttpGet("location")]
        public async Task<ActionResult> GetLocationReport()
        {
            var report = await _reportingService.GetLocationReportAsync(null);
            return Ok(report);
        }

        [HttpGet("export/summary")]
        public async Task<ActionResult> ExportSummary(
            [FromQuery] string format = "csv",
            [FromQuery] string groupBy = "category")
        {
            if (string.IsNullOrEmpty(groupBy))
                groupBy = "category";
            
            var validGroupBy = new[] { "category", "status", "location" };
            if (!validGroupBy.Contains(groupBy.ToLowerInvariant()))
                return BadRequest(new { message = "Invalid groupBy value. Valid values: category, status, location" });

            var validFormats = new[] { "csv", "pdf" };
            if (!validFormats.Contains(format.ToLowerInvariant()))
                return BadRequest(new { message = "Invalid format. Valid values: csv, pdf" });

            var data = await _reportingService.GetInventorySummaryAsync(groupBy, GetUserId(), GetUserRole());
            var dataList = data.ToList();

            if (!dataList.Any())
                return NotFound(new { message = "No data to export" });

            var content = format.ToLowerInvariant() == "pdf"
                ? await _pdfExportService.ExportToPdfAsync(dataList, $"Inventory Summary by {groupBy}", "summary")
                : await _csvExportService.ExportToCsvAsync(dataList, "summary");

            var mimeType = format.ToLowerInvariant() == "pdf" ? "application/pdf" : "text/csv";
            var extension = format.ToLowerInvariant() == "pdf" ? "pdf" : "csv";

            return File(content, mimeType, $"inventory-summary.{extension}");
        }

        [HttpGet("export/history/{assetId:guid}")]
        public async Task<ActionResult> ExportHistory(
            Guid assetId,
            [FromQuery] string format = "csv")
        {
            var validFormats = new[] { "csv", "pdf" };
            if (!validFormats.Contains(format.ToLowerInvariant()))
                return BadRequest(new { message = "Invalid format. Valid values: csv, pdf" });

            try
            {
                var data = await _reportingService.GetAssetHistoryAsync(assetId);
                var dataList = data.ToList();

                if (!dataList.Any())
                    return NotFound(new { message = "No history to export" });

                var content = format.ToLowerInvariant() == "pdf"
                    ? await _pdfExportService.ExportToPdfAsync(dataList, $"Asset History - {assetId}", "history")
                    : await _csvExportService.ExportToCsvAsync(dataList, "history");

                var mimeType = format.ToLowerInvariant() == "pdf" ? "application/pdf" : "text/csv";
                var extension = format.ToLowerInvariant() == "pdf" ? "pdf" : "csv";

                return File(content, mimeType, $"asset-history-{assetId}.{extension}");
            }
            catch (ArgumentException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        [HttpGet("export/activity")]
        public async Task<ActionResult> ExportActivity(
            [FromQuery] string format = "csv",
            [FromQuery] DateTime? from = null,
            [FromQuery] DateTime? to = null)
        {
            var validFormats = new[] { "csv", "pdf" };
            if (!validFormats.Contains(format.ToLowerInvariant()))
                return BadRequest(new { message = "Invalid format. Valid values: csv, pdf" });

            var data = await _reportingService.GetActivityLogAsync(from, to, GetUserId());
            var dataList = data.ToList();

            if (!dataList.Any())
                return NotFound(new { message = "No activity to export" });

            var content = format.ToLowerInvariant() == "pdf"
                ? await _pdfExportService.ExportToPdfAsync(dataList, "Activity Log", "activity")
                : await _csvExportService.ExportToCsvAsync(dataList, "activity");

            var mimeType = format.ToLowerInvariant() == "pdf" ? "application/pdf" : "text/csv";
            var extension = format.ToLowerInvariant() == "pdf" ? "pdf" : "csv";

            return File(content, mimeType, $"activity-log.{extension}");
        }

        // ── Maintenance & Status Reports ──────────────────────────────

        [HttpGet("maintenance-forecast")]
        public async Task<ActionResult> GetMaintenanceForecast([FromQuery] int days = 30)
        {
            var data = await _reportingService.GetMaintenanceForecastAsync(days);
            return Ok(data);
        }

        [HttpGet("export/maintenance-forecast")]
        public async Task<ActionResult> ExportMaintenanceForecast([FromQuery] int days = 30)
        {
            var data = await _reportingService.GetMaintenanceForecastAsync(days);
            if (!data.Any()) return NotFound(new { message = "No data to export" });
            var pdf = _pdfReportService.GenerateMaintenanceForecast(data, days);
            return File(pdf, "application/pdf", $"maintenance-forecast-{days}d.pdf");
        }

        [HttpGet("overdue-maintenance")]
        public async Task<ActionResult> GetOverdueMaintenance()
        {
            var data = await _reportingService.GetOverdueMaintenanceAsync();
            return Ok(data);
        }

        [HttpGet("export/overdue-maintenance")]
        public async Task<ActionResult> ExportOverdueMaintenance()
        {
            var data = await _reportingService.GetOverdueMaintenanceAsync();
            if (!data.Any()) return NotFound(new { message = "No data to export" });
            var pdf = _pdfReportService.GenerateOverdueMaintenance(data);
            return File(pdf, "application/pdf", "overdue-maintenance.pdf");
        }

        [HttpGet("critical-issues")]
        public async Task<ActionResult> GetCriticalIssues()
        {
            var data = await _reportingService.GetCriticalIssuesAsync();
            return Ok(data);
        }

        [HttpGet("export/critical-issues")]
        public async Task<ActionResult> ExportCriticalIssues()
        {
            var data = await _reportingService.GetCriticalIssuesAsync();
            if (!data.Any()) return NotFound(new { message = "No data to export" });
            var pdf = _pdfReportService.GenerateCriticalIssues(data);
            return File(pdf, "application/pdf", "critical-issues.pdf");
        }

        [HttpGet("status-summary")]
        public async Task<ActionResult> GetStatusSummary()
        {
            var data = await _reportingService.GetStatusSummaryAsync();
            return Ok(data);
        }

        [HttpGet("export/status-summary")]
        public async Task<ActionResult> ExportStatusSummary()
        {
            var data = await _reportingService.GetStatusSummaryAsync();
            if (!data.Any()) return NotFound(new { message = "No data to export" });
            var pdf = _pdfReportService.GenerateStatusSummary(data);
            return File(pdf, "application/pdf", "status-summary.pdf");
        }

        // ── Location-Based Reports ────────────────────────────────────

        [HttpGet("zone-inventory")]
        public async Task<ActionResult> GetZoneInventory()
        {
            var data = await _reportingService.GetZoneInventoryAsync();
            return Ok(data);
        }

        [HttpGet("export/zone-inventory")]
        public async Task<ActionResult> ExportZoneInventory()
        {
            var data = await _reportingService.GetZoneInventoryAsync();
            if (!data.Any()) return NotFound(new { message = "No data to export" });
            var pdf = _pdfReportService.GenerateZoneInventory(data);
            return File(pdf, "application/pdf", "zone-inventory.pdf");
        }

        [HttpGet("building-stocktake")]
        public async Task<ActionResult> GetBuildingStocktake()
        {
            var data = await _reportingService.GetBuildingStocktakeAsync();
            return Ok(data);
        }

        [HttpGet("export/building-stocktake")]
        public async Task<ActionResult> ExportBuildingStocktake()
        {
            var data = await _reportingService.GetBuildingStocktakeAsync();
            if (!data.Any()) return NotFound(new { message = "No data to export" });
            var pdf = _pdfReportService.GenerateBuildingStocktake(data);
            return File(pdf, "application/pdf", "building-stocktake.pdf");
        }

        [HttpGet("room-audit/{roomCode}")]
        public async Task<ActionResult> GetRoomAudit(string roomCode)
        {
            var data = await _reportingService.GetRoomAuditAsync(roomCode);
            if (data == null) return NotFound(new { message = "Room not found" });
            return Ok(data);
        }

        [HttpGet("export/room-audit/{roomCode}")]
        public async Task<ActionResult> ExportRoomAudit(string roomCode)
        {
            var data = await _reportingService.GetRoomAuditAsync(roomCode);
            if (data == null) return NotFound(new { message = "Room not found" });
            var pdf = _pdfReportService.GenerateRoomAudit(data);
            return File(pdf, "application/pdf", $"room-audit-{roomCode}.pdf");
        }

        [HttpGet("empty-rooms")]
        public async Task<ActionResult> GetEmptyRooms([FromQuery] int threshold = 0)
        {
            var data = await _reportingService.GetEmptyRoomsAsync(threshold);
            return Ok(data);
        }

        [HttpGet("export/empty-rooms")]
        public async Task<ActionResult> ExportEmptyRooms([FromQuery] int threshold = 0)
        {
            var data = await _reportingService.GetEmptyRoomsAsync(threshold);
            if (!data.Any()) return NotFound(new { message = "No data to export" });
            var pdf = _pdfReportService.GenerateEmptyRooms(data);
            return File(pdf, "application/pdf", "empty-rooms.pdf");
        }

        // ── Audit & Compliance Reports ────────────────────────────────

        [HttpGet("location-discrepancies")]
        public async Task<ActionResult> GetLocationDiscrepancies()
        {
            var data = await _reportingService.GetLocationDiscrepanciesAsync();
            return Ok(data);
        }

        [HttpGet("export/location-discrepancies")]
        public async Task<ActionResult> ExportLocationDiscrepancies()
        {
            var data = await _reportingService.GetLocationDiscrepanciesAsync();
            if (!data.Any()) return NotFound(new { message = "No data to export" });
            var pdf = _pdfReportService.GenerateLocationDiscrepancies(data);
            return File(pdf, "application/pdf", "location-discrepancies.pdf");
        }

        [HttpGet("category-stocktake")]
        public async Task<ActionResult> GetCategoryStocktake()
        {
            var data = await _reportingService.GetCategoryStocktakeAsync();
            return Ok(data);
        }

        [HttpGet("export/category-stocktake")]
        public async Task<ActionResult> ExportCategoryStocktake()
        {
            var data = await _reportingService.GetCategoryStocktakeAsync();
            if (!data.Any()) return NotFound(new { message = "No data to export" });
            var pdf = _pdfReportService.GenerateCategoryStocktake(data);
            return File(pdf, "application/pdf", "category-stocktake.pdf");
        }

        // ── Executive Reports ─────────────────────────────────────────

        [HttpGet("department-report")]
        public async Task<ActionResult> GetDepartmentReport()
        {
            var data = await _reportingService.GetDepartmentReportsAsync();
            return Ok(data);
        }

        [HttpGet("export/department-report")]
        public async Task<ActionResult> ExportDepartmentReport()
        {
            var data = await _reportingService.GetDepartmentReportsAsync();
            if (!data.Any()) return NotFound(new { message = "No data to export" });
            var pdf = _pdfReportService.GenerateDepartmentReport(data);
            return File(pdf, "application/pdf", "department-report.pdf");
        }
    }
}
