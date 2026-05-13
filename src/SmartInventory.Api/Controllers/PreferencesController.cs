using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartInventory.Application.UserPreferences.DTOs;
using SmartInventory.Application.UserPreferences.Interfaces;
using SmartInventory.Domain.Auth.Enums;

namespace SmartInventory.API.Controllers
{
    [ApiController]
    [Route("api/users/preferences")]
    [Authorize]
    public class PreferencesController : ControllerBase
    {
        private readonly IUserPreferenceService _userPreferenceService;

        public PreferencesController(IUserPreferenceService userPreferenceService)
        {
            _userPreferenceService = userPreferenceService;
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
        public async Task<ActionResult<UserPreferencesResponse>> GetPreferences()
        {
            var userId = GetUserId();
            if (userId == Guid.Empty)
                return Unauthorized();

            var result = await _userPreferenceService.GetPreferencesAsync(userId, GetUserRole());
            return Ok(result);
        }

        [HttpPut]
        public async Task<ActionResult<UserPreferencesResponse>> UpdatePreferences([FromBody] Dictionary<string, string> preferences)
        {
            var userId = GetUserId();
            if (userId == Guid.Empty)
                return Unauthorized();

            if (preferences == null || preferences.Count == 0)
                return BadRequest(new { message = "At least one preference is required" });

            var result = await _userPreferenceService.UpdatePreferencesAsync(userId, GetUserRole(), preferences);
            return Ok(result);
        }

        [HttpGet("role-defaults")]
        public async Task<ActionResult<RoleDefaultsResponse>> GetRoleDefaults()
        {
            var result = await _userPreferenceService.GetRoleDefaultsAsync(GetUserRole());
            return Ok(result);
        }
    }
}
