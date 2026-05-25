using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartInventory.Application.Location.DTOs;
using SmartInventory.Application.Location.Interfaces;

namespace SmartInventory.API.Controllers
{
    [ApiController]
    [Route("api/locations")]
    [Authorize]
    public class LocationsController : ControllerBase
    {
        private readonly ILocationService _locationService;

        public LocationsController(ILocationService locationService)
        {
            _locationService = locationService;
        }

        private Guid GetUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return Guid.TryParse(userIdClaim, out var userId) ? userId : Guid.Empty;
        }

        [HttpGet("hierarchy")]
        public async Task<ActionResult<HierarchyDto>> GetHierarchy()
        {
            var hierarchy = await _locationService.GetHierarchyAsync();
            return Ok(hierarchy);
        }

        [HttpGet("rooms/{code}")]
        public async Task<ActionResult<RoomDto>> GetRoomByCode([FromRoute] string code)
        {
            if (string.IsNullOrWhiteSpace(code))
                return BadRequest(new { message = "Room code is required." });

            var trimmedCode = code.Trim();
            var room = await _locationService.GetRoomByCodeAsync(trimmedCode);
            if (room == null)
                return NotFound($"Room with code '{trimmedCode}' not found.");
            return Ok(room);
        }

        [HttpPost("rooms")]
        [Authorize(Roles = "Supervisor")]
        public async Task<ActionResult<RoomDto>> CreateRoom([FromBody] CreateRoomDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var room = await _locationService.CreateRoomAsync(dto, GetUserId());
                return CreatedAtAction(nameof(GetRoomByCode), new { code = room.Code }, room);
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { message = ex.Message });
            }
            catch (System.ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("buildings")]
        [Authorize(Roles = "Supervisor")]
        public async Task<ActionResult<BuildingDto>> CreateBuilding([FromBody] CreateBuildingDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var building = await _locationService.CreateBuildingAsync(dto, GetUserId());
                return CreatedAtAction(nameof(GetHierarchy), building);
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { message = ex.Message });
            }
            catch (System.ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("floors")]
        [Authorize(Roles = "Supervisor")]
        public async Task<ActionResult<FloorDto>> CreateFloor([FromBody] CreateFloorDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var floor = await _locationService.CreateFloorAsync(dto, GetUserId());
                return CreatedAtAction(nameof(GetHierarchy), floor);
            }
            catch (System.ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("rooms/batch")]
        [Authorize(Roles = "Supervisor")]
        public async Task<ActionResult<List<RoomGeometryDto>>> BatchUpdateRooms([FromBody] BatchUpdateRoomGeometriesDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var results = await _locationService.BatchUpdateRoomGeometriesAsync(dto, GetUserId());
                return Ok(results);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        [HttpPut("rooms/{id:guid}")]
        [Authorize(Roles = "Supervisor")]
        public async Task<ActionResult<RoomGeometryDto>> UpdateRoomGeometry([FromRoute] Guid id, [FromBody] UpdateRoomGeometryDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var result = await _locationService.UpdateRoomGeometryAsync(id, dto, GetUserId());
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        [HttpDelete("rooms/{id:guid}")]
        [Authorize(Roles = "Supervisor")]
        public async Task<ActionResult> DeleteRoom([FromRoute] Guid id)
        {
            try
            {
                await _locationService.DeleteRoomAsync(id, GetUserId());
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }
        [HttpGet("site-config")]
        public async Task<ActionResult<SiteConfigDto>> GetSiteConfig()
        {
            var config = await _locationService.GetSiteConfigAsync();
            return Ok(config);
        }

        [HttpPut("site-config")]
        [Authorize(Roles = "Supervisor")]
        public async Task<ActionResult<SiteConfigDto>> UpdateSiteConfig([FromBody] UpdateSiteConfigDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var config = await _locationService.UpdateSiteConfigAsync(dto, GetUserId());
                return Ok(config);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        [HttpPost("site-config/shapes")]
        [Authorize(Roles = "Supervisor")]
        public async Task<ActionResult<ZoneSiteShapeDto>> CreateZoneSiteShape([FromBody] CreateZoneSiteShapeDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var shape = await _locationService.CreateZoneSiteShapeAsync(dto, GetUserId());
                return CreatedAtAction(nameof(GetSiteConfig), shape);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("site-config/shapes/{id:guid}")]
        [Authorize(Roles = "Supervisor")]
        public async Task<ActionResult<ZoneSiteShapeDto>> UpdateZoneSiteShape([FromRoute] Guid id, [FromBody] UpdateZoneSiteShapeDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var shape = await _locationService.UpdateZoneSiteShapeAsync(id, dto, GetUserId());
                return Ok(shape);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        [HttpDelete("site-config/shapes/{id:guid}")]
        [Authorize(Roles = "Supervisor")]
        public async Task<ActionResult> DeleteZoneSiteShape([FromRoute] Guid id)
        {
            try
            {
                await _locationService.DeleteZoneSiteShapeAsync(id, GetUserId());
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }
    }
}