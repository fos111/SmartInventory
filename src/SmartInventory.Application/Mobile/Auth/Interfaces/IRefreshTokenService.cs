using SmartInventory.Application.Mobile.Auth.DTOs;

namespace SmartInventory.Application.Mobile.Auth.Interfaces;

public interface IRefreshTokenService
{
    Task<TokenPairDto> CreateTokenPairAsync(Domain.Auth.Entities.User user, CancellationToken ct = default);
    Task<TokenPairDto?> RefreshAsync(string refreshToken, CancellationToken ct = default);
    Task RevokeAsync(string refreshToken, CancellationToken ct = default);
    Task RevokeAllForUserAsync(Guid userId, CancellationToken ct = default);
}
