using SmartInventory.Domain.Auth.Entities;

namespace SmartInventory.Application.Auth.Interfaces;

public interface IEmailVerificationService
{
    string GenerateToken();
    Task SendVerificationEmailAsync(User user, CancellationToken ct = default);
    Task<(bool Success, string Error)> ValidateTokenAsync(string token, CancellationToken ct = default);
    Task MarkTokenAsUsedAsync(string token, CancellationToken ct = default);
}