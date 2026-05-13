using SmartInventory.Application.Auth.DTOs.Requests;
using SmartInventory.Application.Auth.DTOs.Responses;

namespace SmartInventory.Application.Auth.Interfaces;

public interface IAuthService
{
    Task<AuthResponse?> LoginAsync(LoginRequest request, CancellationToken ct = default);
    Task<AuthResponse?> RegisterAsync(RegisterRequest request, CancellationToken ct = default);
    Task<bool> VerifyEmailAsync(string token, CancellationToken ct = default);
    Task<bool> ResendVerificationEmailAsync(string email, CancellationToken ct = default);
    Task<AuthResponse?> GetLimitedLoginAsync(LoginRequest request, CancellationToken ct = default);
    Task<AuthResponse?> RequestReEvaluationAsync(Guid userId, CancellationToken ct = default);
}