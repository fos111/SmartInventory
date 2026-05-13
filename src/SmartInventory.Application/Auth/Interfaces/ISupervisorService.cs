using SmartInventory.Application.Auth.DTOs.Responses;
using SmartInventory.Domain.Auth.Enums;

namespace SmartInventory.Application.Auth.Interfaces;

public interface ISupervisorService
{
    Task<List<UserListResponse>> GetPendingUsersAsync(CancellationToken ct = default);
    Task<bool> ApproveUserAsync(Guid userId, UserRole role, Guid supervisorId, CancellationToken ct = default);
    Task<bool> RejectUserAsync(Guid userId, Guid supervisorId, string? reason, CancellationToken ct = default);
}