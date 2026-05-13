using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartInventory.Application.Auth.DTOs.Requests;
using SmartInventory.Application.Auth.Interfaces;

namespace SmartInventory.Api.Controllers.Auth;

[ApiController]
[Route("api/supervisor")]
[Authorize(Roles = "Supervisor")]
public class SupervisorController : ControllerBase
{
    private readonly ISupervisorService _supervisorService;

    public SupervisorController(ISupervisorService supervisorService)
    {
        _supervisorService = supervisorService;
    }

    [HttpGet("users/pending")]
    public async Task<IActionResult> GetPendingUsers(CancellationToken ct)
    {
        var users = await _supervisorService.GetPendingUsersAsync(ct);
        return Ok(users);
    }

    [HttpPost("users/{id}/approve")]
    public async Task<IActionResult> ApproveUser(Guid id, [FromBody] ApproveUserRequest request, CancellationToken ct)
    {
        var supervisorIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (!Guid.TryParse(supervisorIdClaim, out var supervisorId))
            return Unauthorized();

        var success = await _supervisorService.ApproveUserAsync(id, request.Role, supervisorId, ct);
        if (!success)
            return BadRequest(new { message = "User not found or not eligible for approval" });

        return Ok(new { message = "User approved" });
    }

    [HttpPost("users/{id}/reject")]
    public async Task<IActionResult> RejectUser(Guid id, [FromBody] RejectUserRequest request, CancellationToken ct)
    {
        var supervisorIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (!Guid.TryParse(supervisorIdClaim, out var supervisorId))
            return Unauthorized();

        var success = await _supervisorService.RejectUserAsync(id, supervisorId, request.Reason, ct);
        if (!success)
            return BadRequest(new { message = "User not found or not eligible for rejection" });

        return Ok(new { message = "User rejected" });
    }
}
