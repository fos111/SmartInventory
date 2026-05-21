using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartInventory.Api.Models;
using SmartInventory.Application.Asset.DTOs;
using SmartInventory.Application.Asset.Interfaces;
using SmartInventory.Application.Mobile.Products.DTOs;
using SmartInventory.Application.Mobile.Products.Interfaces;
using SmartInventory.Application.Storage.Interfaces;

namespace SmartInventory.Api.Controllers.Mobile;

[ApiController]
[Route("api/mobile/products")]
[Authorize]
public class MobileProductsController : ControllerBase
{
    private readonly IMobileProductService _productService;
    private readonly IMobileProductWriteService _productWriteService;
    private readonly IFileStorageService _fileStorage;
    private readonly IAssetService _assetService;

    public MobileProductsController(
        IMobileProductService productService,
        IMobileProductWriteService productWriteService,
        IFileStorageService fileStorage,
        IAssetService assetService)
    {
        _productService = productService;
        _productWriteService = productWriteService;
        _fileStorage = fileStorage;
        _assetService = assetService;
    }

    [HttpGet]
    public async Task<ActionResult<MobileEnvelope<MobilePagedResultDto<AssetListItemDto>>>> GetProducts(
        [FromQuery] MobileProductFilterDto filter,
        CancellationToken ct)
    {
        var result = await _productService.GetProductsAsync(filter, ct);
        return Ok(MobileEnvelope<MobilePagedResultDto<AssetListItemDto>>.SuccessResult(result));
    }

    [HttpGet("scan")]
    public async Task<ActionResult<MobileEnvelope<AssetScanDto>>> ScanByTag(
        [FromQuery] string assetTag,
        CancellationToken ct)
    {
        var userId = GetUserId();
        var result = await _productService.ScanByTagAsync(assetTag, userId, ct);

        if (result == null)
            return Ok(MobileEnvelope<AssetScanDto>.FailureResult($"Asset with tag '{assetTag}' not found."));

        return Ok(MobileEnvelope<AssetScanDto>.SuccessResult(result));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<MobileEnvelope<AssetScanDto>>> GetProduct(
        Guid id,
        CancellationToken ct)
    {
        var result = await _productService.GetProductByIdAsync(id, ct);

        if (result == null)
            return Ok(MobileEnvelope<AssetScanDto>.FailureResult($"Asset with ID '{id}' not found."));

        return Ok(MobileEnvelope<AssetScanDto>.SuccessResult(result));
    }

    [HttpGet("scan-history")]
    public async Task<ActionResult<MobileEnvelope<List<ScanHistoryEntryDto>>>> GetScanHistory(
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        CancellationToken ct)
    {
        var userId = GetUserId();
        var history = await _productService.GetScanHistoryAsync(from, to, userId, ct);
        return Ok(MobileEnvelope<List<ScanHistoryEntryDto>>.SuccessResult(history.ToList()));
    }

    // New multipart POST — replaces [FromBody] version as primary
    [HttpPost]
    public async Task<ActionResult<MobileEnvelope<AssetScanDto>>> CreateProduct(
        [FromForm] MobileProductCreateMultipartRequest request,
        CancellationToken ct)
    {
        var userId = GetUserId();

        // Save photo if provided
        string? photoUrl = null;
        if (request.Photo != null && request.Photo.Length > 0)
        {
            if (!IsValidImage(request.Photo))
                return BadRequest(MobileEnvelope<AssetScanDto>.FailureResult("Only JPEG and PNG files are allowed (max 5MB)"));

            var extension = System.IO.Path.GetExtension(request.Photo.FileName).ToLowerInvariant();
            var fileName = $"{Guid.NewGuid():N}{extension}";
            await using var stream = request.Photo.OpenReadStream();
            photoUrl = await _fileStorage.SaveFileAsync("products", fileName, stream, request.Photo.ContentType, ct);
        }

        var dto = MapToMobileCreateDto(request, photoUrl);
        var result = await _productWriteService.CreateProductFromMobileAsync(dto, userId, ct);

        if (result == null)
            return Ok(MobileEnvelope<AssetScanDto>.FailureResult("Failed to create product"));

        return Ok(MobileEnvelope<AssetScanDto>.SuccessResult(result));
    }

    // Keep [FromBody] version available for backward compat via different route
    [HttpPost("json")]
    public async Task<ActionResult<MobileEnvelope<AssetScanDto>>> CreateProductJson(
        [FromBody] CreateProductRequestDto dto,
        CancellationToken ct)
    {
        var userId = GetUserId();
        var result = await _productWriteService.CreateProductAsync(dto, userId, ct);

        if (result == null)
            return Ok(MobileEnvelope<AssetScanDto>.FailureResult("Failed to create product"));

        return Ok(MobileEnvelope<AssetScanDto>.SuccessResult(result));
    }

    // New full-update PUT with multipart support
    [HttpPut("{id:guid}")]
    public async Task<ActionResult<MobileEnvelope<AssetScanDto>>> UpdateProduct(
        Guid id,
        [FromForm] MobileProductCreateMultipartRequest request,
        CancellationToken ct)
    {
        var userId = GetUserId();

        string? photoUrl = null;
        if (request.Photo != null && request.Photo.Length > 0)
        {
            if (!IsValidImage(request.Photo))
                return BadRequest(MobileEnvelope<AssetScanDto>.FailureResult("Only JPEG and PNG files are allowed (max 5MB)"));

            var extension = System.IO.Path.GetExtension(request.Photo.FileName).ToLowerInvariant();
            var fileName = $"{Guid.NewGuid():N}{extension}";
            await using var stream = request.Photo.OpenReadStream();
            photoUrl = await _fileStorage.SaveFileAsync("products", fileName, stream, request.Photo.ContentType, ct);
        }

        var dto = MapToMobileCreateDto(request, photoUrl);
        var result = await _productWriteService.UpdateProductAsync(id, dto, userId, ct);

        if (result == null)
            return Ok(MobileEnvelope<AssetScanDto>.FailureResult($"Product with ID '{id}' not found."));

        return Ok(MobileEnvelope<AssetScanDto>.SuccessResult(result));
    }

    [HttpPut("{id:guid}/status")]
    public async Task<ActionResult<MobileEnvelope<AssetScanDto>>> UpdateProductStatus(
        Guid id,
        [FromBody] UpdateStatusRequestDto dto,
        CancellationToken ct)
    {
        var userId = GetUserId();
        var result = await _productWriteService.UpdateProductStatusAsync(id, dto.Status, userId, ct, dto.Note);

        if (result == null)
            return Ok(MobileEnvelope<AssetScanDto>.FailureResult($"Product with ID '{id}' not found."));

        return Ok(MobileEnvelope<AssetScanDto>.SuccessResult(result));
    }

    [HttpPut("{id:guid}/ble-id")]
    public async Task<ActionResult<MobileEnvelope<AssetScanDto>>> UpdateProductBleId(
        Guid id,
        [FromBody] UpdateBleIdDto dto,
        CancellationToken ct)
    {
        var userId = GetUserId();
        var result = await _productWriteService.UpdateProductBleIdAsync(id, dto.BleId, userId, ct);

        if (result == null)
            return Ok(MobileEnvelope<AssetScanDto>.FailureResult($"Product with ID '{id}' not found, or BLE ID already in use."));

        return Ok(MobileEnvelope<AssetScanDto>.SuccessResult(result));
    }

    [HttpPut("{id:guid}/price")]
    public async Task<ActionResult<MobileEnvelope<AssetScanDto>>> UpdateProductPrice(
        Guid id,
        [FromBody] UpdatePriceDto dto,
        CancellationToken ct)
    {
        var userId = GetUserId();
        var result = await _productWriteService.UpdateProductPriceAsync(id, dto.Price, userId, ct);

        if (result == null)
            return Ok(MobileEnvelope<AssetScanDto>.FailureResult($"Product with ID '{id}' not found."));

        return Ok(MobileEnvelope<AssetScanDto>.SuccessResult(result));
    }

    [HttpPut("{id:guid}/location")]
    public async Task<ActionResult<MobileEnvelope<AssetScanDto>>> MoveProduct(
        Guid id,
        [FromBody] MoveProductRequestDto dto,
        CancellationToken ct)
    {
        var userId = GetUserId();
        var (result, message) = await _productWriteService.MoveProductAsync(id, dto.RoomId, userId, ct);

        if (result == null)
            return Ok(MobileEnvelope<AssetScanDto>.FailureResult(message ?? $"Product with ID '{id}' not found."));

        return Ok(MobileEnvelope<AssetScanDto>.SuccessResult(result));
    }

    [HttpDelete("{id:guid}")]
    public async Task<ActionResult<MobileEnvelope>> DeleteProduct(
        Guid id,
        CancellationToken ct)
    {
        var userId = GetUserId();
        var (success, message) = await _productWriteService.DeleteProductAsync(id, userId, ct);

        if (!success)
            return Ok(MobileEnvelope.FailureResult(message ?? $"Product with ID '{id}' not found."));

        return Ok(MobileEnvelope.SuccessResult("Product deleted"));
    }

    // Scan history POST — lightweight audit trail
    [HttpPost("scan-history")]
    public async Task<ActionResult<MobileEnvelope>> RecordScan(
        [FromBody] MobileProductScanHistoryRequest request,
        CancellationToken ct)
    {
        var userId = GetUserId();
        var result = await _productService.ScanByTagAsync(request.AssetTag, userId, ct);

        if (result == null)
            return Ok(MobileEnvelope.FailureResult($"Asset with tag '{request.AssetTag}' not found."));

        return Ok(MobileEnvelope.SuccessResult("Scan recorded"));
    }

    private static MobileProductCreateDto MapToMobileCreateDto(MobileProductCreateMultipartRequest request, string? photoUrl)
    {
        return new MobileProductCreateDto
        {
            Name = request.Name,
            Sku = request.Sku,
            Type = request.Type,
            Description = request.Description,
            RoomId = request.RoomId,
            PhotoPath = photoUrl,
            Tags = request.Tags,
            Price = request.Price,
            Specifications = request.Specifications,
            BleId = request.BleId
        };
    }

    [HttpGet("{id:guid}/barcode")]
    public async Task<ActionResult<MobileEnvelope<BarcodeImageDto>>> GetProductBarcode(Guid id)
    {
        try
        {
            var barcodeBytes = await _assetService.GenerateBarcodeAsync(id, 300, 80);
            var base64 = Convert.ToBase64String(barcodeBytes);
            return Ok(MobileEnvelope<BarcodeImageDto>.SuccessResult(
                new BarcodeImageDto { BarcodeBase64 = base64 }));
        }
        catch (ArgumentException ex)
        {
            return NotFound(MobileEnvelope<string>.FailureResult(ex.Message));
        }
    }

    private Guid GetUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(userIdClaim, out var userId) ? userId : Guid.Empty;
    }

    private static bool IsValidImage(IFormFile file)
    {
        if (file.Length > 5 * 1024 * 1024)
            return false;

        var contentType = file.ContentType.ToLowerInvariant();
        return contentType is "image/jpeg" or "image/png";
    }
}
