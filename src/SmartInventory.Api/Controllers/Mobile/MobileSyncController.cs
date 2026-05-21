using System;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartInventory.Api.Models;
using SmartInventory.Application.Mobile.Sync.DTOs;
using SmartInventory.Application.Mobile.Sync.Interfaces;

namespace SmartInventory.Api.Controllers.Mobile;

[ApiController]
[Route("api/mobile/sync")]
[Authorize]
public class MobileSyncController : ControllerBase
{
    private readonly IMobileSyncService _syncService;

    public MobileSyncController(IMobileSyncService syncService)
    {
        _syncService = syncService;
    }

    [HttpPost("queue")]
    public async Task<ActionResult<MobileEnvelope<SyncStatusDto>>> QueueSync(
        [FromBody] SyncBatchDto dto,
        CancellationToken ct)
    {
        var userId = GetUserId();
        var result = await _syncService.ProcessQueueAsync(dto, userId, ct);
        return Ok(MobileEnvelope<SyncStatusDto>.SuccessResult(result));
    }

    [HttpGet("status")]
    public async Task<ActionResult<MobileEnvelope<SyncStatusDto>>> GetStatus(
        CancellationToken ct)
    {
        var userId = GetUserId();
        var result = await _syncService.GetStatusAsync(userId, ct);
        return Ok(MobileEnvelope<SyncStatusDto>.SuccessResult(result));
    }

    private Guid GetUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(userIdClaim, out var userId) ? userId : Guid.Empty;
    }
}
