using SmartInventory.Application.Mobile.Auth.DTOs;

namespace SmartInventory.Application.Mobile.Auth.Interfaces;

public interface IMobileAuthService
{
    Task<RegisterResultDto?> RegisterAsync(MobileRegisterRequest request, CancellationToken ct = default);
    Task<(Domain.Auth.Entities.User? User, bool NeedsVerification)> LoginAsync(MobileLoginRequest request, CancellationToken ct = default);
    Task<TokenPairDto?> VerifyEmailAsync(VerifyEmailRequest request, CancellationToken ct = default);
    Task<bool> ResendVerificationAsync(ResendVerificationRequest request, CancellationToken ct = default);
    Task<MobileUserDto?> GetProfileAsync(Guid userId, CancellationToken ct = default);
    Task<string?> UpdateAvatarAsync(Guid userId, string avatarUrl, CancellationToken ct = default);
}
