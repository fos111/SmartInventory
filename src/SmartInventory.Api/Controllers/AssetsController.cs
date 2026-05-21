using System;
using System.IO;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartInventory.Application.Asset.Common;
using SmartInventory.Application.Asset.DTOs;
using SmartInventory.Application.Asset.Filters;
using SmartInventory.Application.Asset.Interfaces;
using SmartInventory.Domain.Auth.Enums;

namespace SmartInventory.API.Controllers
{
    [ApiController]
    [Route("api/assets")]
    [Authorize]
    public class AssetsController : ControllerBase
    {
        private readonly IAssetService _assetService;

        public AssetsController(IAssetService assetService)
        {
            _assetService = assetService;
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

        [HttpGet]
        public async Task<ActionResult<PagedResult<AssetDto>>> GetAssets([FromQuery] AssetFilter filter)
        {
            var result = await _assetService.GetAssetsAsync(filter);
            return Ok(result);
        }

        [HttpGet("{id:guid}")]
        public async Task<ActionResult<AssetDto>> GetAsset(Guid id)
        {
            var asset = await _assetService.GetAssetByIdAsync(id);
            if (asset == null)
                return NotFound($"Asset with ID '{id}' not found.");
            return Ok(asset);
        }

        [HttpGet("tag/{assetTag}")]
        public async Task<ActionResult<AssetDto>> GetAssetByTag(string assetTag)
        {
            var asset = await _assetService.GetAssetByTagAsync(assetTag);
            if (asset == null)
                return NotFound($"Asset with tag '{assetTag}' not found.");
            return Ok(asset);
        }

        [HttpPost]
        [Authorize(Roles = "Supervisor,Gestionnaire")]
        public async Task<ActionResult<AssetDto>> CreateAsset([FromBody] CreateAssetDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var asset = await _assetService.CreateAssetAsync(dto, GetUserId());
                return CreatedAtAction(nameof(GetAsset), new { id = asset.Id }, asset);
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("{id:guid}")]
        [Authorize(Roles = "Supervisor")]
        public async Task<ActionResult<AssetDto>> UpdateAsset(Guid id, [FromBody] UpdateAssetDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var asset = await _assetService.UpdateAssetAsync(id, dto, GetUserId());
                return Ok(asset);
            }
            catch (ArgumentException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { message = ex.Message });
            }
        }

        [HttpPut("{id:guid}/move")]
        [Authorize(Roles = "Supervisor")]
        public async Task<ActionResult<AssetDto>> MoveAsset(Guid id, [FromBody] MoveAssetDto dto)
        {
            try
            {
                var asset = await _assetService.MoveAssetAsync(id, dto.NewRoomCode, GetUserId());
                return Ok(asset);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("{id:guid}/rfid")]
        [Authorize(Roles = "Supervisor")]
        public async Task<ActionResult<AssetDto>> UpdateRfid(Guid id, [FromBody] UpdateRfidDto dto)
        {
            try
            {
                var asset = await _assetService.UpdateRfidAsync(id, dto.RfidTagId, GetUserId());
                return Ok(asset);
            }
            catch (ArgumentException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { message = ex.Message });
            }
        }

        [HttpPut("{id:guid}/status")]
        [Authorize(Roles = "Supervisor")]
        public async Task<ActionResult<AssetDto>> UpdateStatus(Guid id, [FromBody] UpdateAssetStatusDto dto)
        {
            try
            {
                var asset = await _assetService.UpdateStatusAsync(id, dto.Status, GetUserId(), GetUserRole(), dto.Note);
                return Ok(asset);
            }
            catch (ArgumentException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(403, new { message = ex.Message });
            }
        }

        [HttpPut("{id}/maintenance-due")]
        [Authorize(Roles = "Supervisor,Technicien")]
        public async Task<ActionResult<AssetDto>> SetMaintenanceDueDate(string id, [FromBody] SetMaintenanceDueDateDto dto)
        {
            if (!Guid.TryParse(id, out var guid))
            {
                var assetByTag = await _assetService.GetAssetByTagAsync(id);
                if (assetByTag == null)
                    return NotFound(new { message = $"Asset with tag '{id}' not found." });
                guid = assetByTag.Id;
            }

            try
            {
                var asset = await _assetService.SetMaintenanceDueDateAsync(guid, dto.DueDate, GetUserId());
                return Ok(asset);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Supervisor,Gestionnaire")]
        public async Task<IActionResult> DeleteAsset(string id)
        {
            try
            {
                await _assetService.DeleteAssetAsync(id, GetUserId());
                return NoContent();
            }
            catch (ArgumentException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        [HttpGet("{id:guid}/qr")]
        public async Task<IActionResult> GetQrCode(Guid id)
        {
            try
            {
                var qrBytes = await _assetService.GenerateQrCodeAsync(id);
                return File(qrBytes, "image/png");
            }
            catch (ArgumentException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        [HttpGet("{id:guid}/barcode")]
        public async Task<IActionResult> GetBarcode(Guid id, [FromQuery] int width = 300, [FromQuery] int height = 80)
        {
            try
            {
                var barcodeBytes = await _assetService.GenerateBarcodeAsync(id, width, height);
                return File(barcodeBytes, "image/png");
            }
            catch (ArgumentException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        [HttpGet("reconciliation")]
        public async Task<ActionResult> GetReconciliation()
        {
            var assets = await _assetService.GetReconciliationAsync();
            return Ok(assets);
        }

        [HttpPost("import")]
        [Authorize(Roles = "Supervisor,Gestionnaire")]
        public async Task<ActionResult<BulkImportResponse>> ImportAssets(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest(new { message = "Please upload a CSV file." });

            if (!file.FileName.EndsWith(".csv", StringComparison.OrdinalIgnoreCase))
                return BadRequest(new { message = "Only CSV files are allowed." });

            using var stream = file.OpenReadStream();
            var result = await _assetService.ImportAssetsAsync(stream, GetUserId());
            return Accepted(result);
        }
    }
}