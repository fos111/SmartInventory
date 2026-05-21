using System;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartInventory.Api.Models;
using SmartInventory.Application.Mobile.Home.DTOs;
using SmartInventory.Application.Mobile.Home.Interfaces;

namespace SmartInventory.Api.Controllers.Mobile;

[ApiController]
[Route("api/mobile/home")]
[Authorize]
public class MobileHomeController : ControllerBase
{
    private readonly IMobileHomeService _homeService;

    public MobileHomeController(IMobileHomeService homeService)
    {
        _homeService = homeService;
    }

    [HttpGet]
    public async Task<ActionResult<MobileEnvelope<HomeSyncDto>>> GetHome(CancellationToken ct)
    {
        var userId = GetUserId();
        var result = await _homeService.GetHomeAsync(userId, ct);
        return Ok(MobileEnvelope<HomeSyncDto>.SuccessResult(result));
    }

    private Guid GetUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(userIdClaim, out var userId) ? userId : Guid.Empty;
    }
}
