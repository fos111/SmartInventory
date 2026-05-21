using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartInventory.Api.Models;
using SmartInventory.Application.Auth.Interfaces;
using SmartInventory.Application.Mobile.Auth.DTOs;
using SmartInventory.Application.Mobile.Auth.Helpers;
using SmartInventory.Domain.Auth.Enums;

namespace SmartInventory.Api.Controllers.Mobile;

[ApiController]
[Route("api/mobile/auth")]
[Authorize(Roles = "Supervisor")]
public class MobileSupervisorController : ControllerBase
{
    private readonly IAuthRepository _authRepository;
    private readonly IMapper _mapper;

    public MobileSupervisorController(IAuthRepository authRepository, IMapper mapper)
    {
        _authRepository = authRepository;
        _mapper = mapper;
    }

    [HttpGet("users")]
    public async Task<ActionResult<MobileEnvelope<List<MobileUserDto>>>> GetUsers(CancellationToken ct)
    {
        var users = await _authRepository.GetAllAsync(ct);
        var userDtos = users.Select(u => _mapper.Map<MobileUserDto>(u)).ToList();

        return Ok(MobileEnvelope<List<MobileUserDto>>.SuccessResult(userDtos));
    }

    [HttpPatch("users/{userId:guid}/role")]
    public async Task<ActionResult<MobileEnvelope>> UpdateRole(
        Guid userId,
        [FromBody] UpdateRoleRequest request,
        CancellationToken ct)
    {
        if (!MobileRoleMapper.IsValidMobileRole(request.Role))
            return BadRequest(MobileEnvelope.FailureResult(
                "Invalid role. Allowed: technicien, magazinier, admin"));

        var user = await _authRepository.GetByIdAsync(userId, ct);
        if (user == null)
            return NotFound(MobileEnvelope.FailureResult("User not found"));

        user.Role = MobileRoleMapper.MapToDotNet(request.Role);
        await _authRepository.UpdateAsync(user, ct);

        return Ok(MobileEnvelope.SuccessResult("Role updated"));
    }

    [HttpPatch("users/{userId:guid}/status")]
    public async Task<ActionResult<MobileEnvelope>> UpdateStatus(
        Guid userId,
        [FromBody] UpdateStatusRequest request,
        CancellationToken ct)
    {
        var user = await _authRepository.GetByIdAsync(userId, ct);
        if (user == null)
            return NotFound(MobileEnvelope.FailureResult("User not found"));

        user.Status = request.IsActive ? AccountStatus.Active : AccountStatus.Rejected;
        await _authRepository.UpdateAsync(user, ct);

        return Ok(MobileEnvelope.SuccessResult("Status updated"));
    }

    [HttpDelete("users/{userId:guid}")]
    public async Task<ActionResult<MobileEnvelope>> DeleteUser(Guid userId, CancellationToken ct)
    {
        var user = await _authRepository.GetByIdAsync(userId, ct);
        if (user == null)
            return NotFound(MobileEnvelope.FailureResult("User not found"));

        user.Status = AccountStatus.Rejected;
        await _authRepository.UpdateAsync(user, ct);

        return Ok(MobileEnvelope.SuccessResult("User deleted"));
    }
}

public record UpdateRoleRequest(string Role);

public record UpdateStatusRequest(bool IsActive);
