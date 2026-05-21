using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartInventory.Api.Models;
using SmartInventory.Application.Mobile.Products.DTOs;
using SmartInventory.Application.Mobile.Products.Interfaces;
using SmartInventory.Application.Mobile.Sync.DTOs;
using SmartInventory.Application.Mobile.Sync.Interfaces;

namespace SmartInventory.Api.Controllers.Mobile;

[ApiController]
[Route("api/mobile/assets")]
[Authorize]
public class MobileAssetsController : ControllerBase
{
    private readonly IMobileProductService _productService;
    private readonly IMobileSyncService _syncService;

    public MobileAssetsController(
        IMobileProductService productService,
        IMobileSyncService syncService)
    {
        _productService = productService;
        _syncService = syncService;
    }

    [HttpGet]
    public async Task<ActionResult<MobileEnvelope<MobilePagedResultDto<AssetListItemDto>>>> GetAssets(
        [FromQuery] MobileProductFilterDto filter,
        CancellationToken ct)
    {
        var result = await _productService.GetProductsAsync(filter, ct);
        return Ok(MobileEnvelope<MobilePagedResultDto<AssetListItemDto>>.SuccessResult(result));
    }

    [HttpPost("batch")]
    public async Task<ActionResult<MobileEnvelope<List<BatchOperationResult>>>> Batch(
        [FromBody] BatchOperationRequest request,
        CancellationToken ct)
    {
        var userId = GetUserId();
        var result = await _syncService.ProcessBatchAsync(request, userId, ct);
        return Ok(MobileEnvelope<List<BatchOperationResult>>.SuccessResult(result));
    }

    private Guid GetUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(userIdClaim, out var userId) ? userId : Guid.Empty;
    }
}
