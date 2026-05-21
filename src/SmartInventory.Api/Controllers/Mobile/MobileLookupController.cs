using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartInventory.Api.Models;
using SmartInventory.Application.Mobile.Lookup.DTOs;
using SmartInventory.Application.Mobile.Lookup.Interfaces;
using SmartInventory.Application.Asset.Interfaces;
using System.Security.Claims;

namespace SmartInventory.Api.Controllers.Mobile;

[ApiController]
[Route("api/mobile/lookups")]
[Authorize]
public class MobileLookupController : ControllerBase
{
    private readonly IMobileLookupService _lookupService;
    private readonly IActivityLogService _activityLogService;

    public MobileLookupController(
        IMobileLookupService lookupService,
        IActivityLogService activityLogService)
    {
        _lookupService = lookupService;
        _activityLogService = activityLogService;
    }

    [HttpGet("categories")]
    public async Task<ActionResult<MobileEnvelope<List<MobileCategoryDto>>>> GetCategories(
        CancellationToken ct)
    {
        var categories = await _lookupService.GetCategoriesAsync(ct);
        return Ok(MobileEnvelope<List<MobileCategoryDto>>.SuccessResult(categories.ToList()));
    }

    [HttpGet("departments")]
    public async Task<ActionResult<MobileEnvelope<List<MobileDepartmentDto>>>> GetDepartments(
        CancellationToken ct)
    {
        var departments = await _lookupService.GetDepartmentsAsync(ct);
        return Ok(MobileEnvelope<List<MobileDepartmentDto>>.SuccessResult(departments.ToList()));
    }

    [HttpGet("departments/{zoneId:guid}/rooms")]
    public async Task<ActionResult<MobileEnvelope<List<MobileRoomDto>>>> GetRoomsByDepartment(
        Guid zoneId,
        CancellationToken ct)
    {
        var rooms = await _lookupService.GetRoomsByDepartmentAsync(zoneId, ct);
        return Ok(MobileEnvelope<List<MobileRoomDto>>.SuccessResult(rooms.ToList()));
    }

    [HttpGet("departments/by-code/{code}/rooms")]
    public async Task<ActionResult<MobileEnvelope<List<MobileRoomDto>>>> GetRoomsByDepartmentCode(
        string code,
        CancellationToken ct)
    {
        var rooms = await _lookupService.GetRoomsByDepartmentCodeAsync(code, ct);
        return Ok(MobileEnvelope<List<MobileRoomDto>>.SuccessResult(rooms.ToList()));
    }

    [HttpGet("stats")]
    public async Task<ActionResult<MobileEnvelope<MobileInventoryStatsDto>>> GetStats(
        CancellationToken ct)
    {
        var stats = await _lookupService.GetStatsAsync(ct);
        return Ok(MobileEnvelope<MobileInventoryStatsDto>.SuccessResult(stats));
    }

    [HttpGet("move-log")]
    public async Task<ActionResult<MobileEnvelope<List<MobileMoveLogEntryDto>>>> GetMoveLog(
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        CancellationToken ct)
    {
        var entries = await _lookupService.GetMoveLogAsync(from, to, ct);
        return Ok(MobileEnvelope<List<MobileMoveLogEntryDto>>.SuccessResult(entries.ToList()));
    }

    [HttpGet("barcode-check")]
    public async Task<ActionResult<MobileEnvelope<BarcodeCheckResultDto>>> CheckBarcode(
        [FromQuery] string barcode,
        CancellationToken ct)
    {
        var result = await _lookupService.CheckBarcodeAsync(barcode, ct);
        return Ok(MobileEnvelope<BarcodeCheckResultDto>.SuccessResult(result));
    }

    // Scan history POST — logs department QR scan
    [HttpPost("scan-history")]
    public async Task<ActionResult<MobileEnvelope>> RecordDepartmentScan(
        [FromBody] MobileLookupScanHistoryRequest request,
        CancellationToken ct)
    {
        var userId = GetUserId();
        await _activityLogService.TrackFacilityChangeAsync(
            "Scanned",
            "Department",
            request.DepartmentCode,
            request.DepartmentName,
            null,
            userId);

        return Ok(MobileEnvelope.SuccessResult("Scan recorded"));
    }

    private Guid GetUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(userIdClaim, out var userId) ? userId : Guid.Empty;
    }
}
