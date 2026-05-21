using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartInventory.Application.Mobile.Reports.Interfaces;

namespace SmartInventory.Api.Controllers.Mobile;

[ApiController]
[Route("api/mobile/reports")]
public class MobileReportsController : ControllerBase
{
    private readonly IMobileReportService _reportService;

    public MobileReportsController(IMobileReportService reportService)
    {
        _reportService = reportService;
    }

    [Authorize]
    [HttpGet("fiche/{roomCode}")]
    public async Task<IActionResult> GetRoomFiche(string roomCode)
    {
        var pdf = await _reportService.GetRoomFicheAsync(roomCode);
        if (pdf == null)
            return NotFound();

        return File(pdf, "application/pdf");
    }

    [Authorize]
    [HttpGet("journal/{roomCode}")]
    public async Task<IActionResult> GetRoomJournal(
        string roomCode,
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to)
    {
        var pdf = await _reportService.GetRoomJournalAsync(roomCode, from, to);
        if (pdf == null)
            return NotFound();

        return File(pdf, "application/pdf");
    }

    [AllowAnonymous]
    [HttpGet("qr/department/{deptId:guid}")]
    public async Task<IActionResult> GetDepartmentQr(Guid deptId)
    {
        var png = await _reportService.GetDepartmentQrAsync(deptId);
        if (png == null)
            return NotFound();

        return File(png, "image/png");
    }

    [AllowAnonymous]
    [HttpGet("qr/department/by-code/{code}")]
    public async Task<IActionResult> GetDepartmentQrByCode(string code)
    {
        var png = await _reportService.GetDepartmentQrByCodeAsync(code);
        return File(png, "image/png");
    }

    [AllowAnonymous]
    [HttpGet("qr/room/{roomCode}")]
    public async Task<IActionResult> GetRoomQr(string roomCode)
    {
        var png = await _reportService.GetRoomQrAsync(roomCode);
        if (png == null)
            return NotFound();

        return File(png, "image/png");
    }

    [AllowAnonymous]
    [HttpGet("qr/iset")]
    public async Task<IActionResult> GetIsetQr()
    {
        var png = await _reportService.GetIsetQrAsync();
        return File(png, "image/png");
    }
}
