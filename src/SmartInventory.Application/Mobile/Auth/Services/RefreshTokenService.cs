using System.Security.Cryptography;
using SmartInventory.Application.Auth.Interfaces;
using SmartInventory.Application.Mobile.Auth.DTOs;
using SmartInventory.Application.Mobile.Auth.Interfaces;
using SmartInventory.Domain.Mobile.Auth.Entities;

namespace SmartInventory.Application.Mobile.Auth.Services;

public class RefreshTokenService : IRefreshTokenService
{
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly ITokenService _tokenService;
    private readonly IAuthRepository _authRepository;

    private const int RefreshTokenExpiryDays = 30;
    private const int AccessTokenExpirySeconds = 3600;

    public RefreshTokenService(
        IRefreshTokenRepository refreshTokenRepository,
        ITokenService tokenService,
        IAuthRepository authRepository)
    {
        _refreshTokenRepository = refreshTokenRepository;
        _tokenService = tokenService;
        _authRepository = authRepository;
    }

    public async Task<TokenPairDto> CreateTokenPairAsync(Domain.Auth.Entities.User user, CancellationToken ct = default)
    {
        var accessToken = _tokenService.GenerateToken(user);
        var (rawToken, tokenHash) = GenerateRefreshToken();

        var refreshToken = new RefreshToken
        {
            TokenHash = tokenHash,
            UserId = user.Id,
            ExpiresAt = DateTime.UtcNow.AddDays(RefreshTokenExpiryDays)
        };

        await _refreshTokenRepository.AddAsync(refreshToken, ct);

        return new TokenPairDto(accessToken, rawToken, AccessTokenExpirySeconds);
    }

    public async Task<TokenPairDto?> RefreshAsync(string refreshToken, CancellationToken ct = default)
    {
        var tokenHash = HashToken(refreshToken);
        var storedToken = await _refreshTokenRepository.GetByTokenHashAsync(tokenHash, ct);

        if (storedToken == null || !storedToken.IsActive)
            return null;

        var user = await _authRepository.GetByIdAsync(storedToken.UserId, ct);
        if (user == null)
            return null;

        await _refreshTokenRepository.RevokeAsync(storedToken.Id, ct);

        return await CreateTokenPairAsync(user, ct);
    }

    public async Task RevokeAsync(string refreshToken, CancellationToken ct = default)
    {
        var tokenHash = HashToken(refreshToken);
        var storedToken = await _refreshTokenRepository.GetByTokenHashAsync(tokenHash, ct);

        if (storedToken != null)
        {
            await _refreshTokenRepository.RevokeAsync(storedToken.Id, ct);
        }
    }

    public async Task RevokeAllForUserAsync(Guid userId, CancellationToken ct = default)
    {
        await _refreshTokenRepository.RevokeAllForUserAsync(userId, ct);
    }

    private static (string rawToken, string hash) GenerateRefreshToken()
    {
        var randomBytes = new byte[16];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomBytes);
        var rawToken = Convert.ToBase64String(randomBytes);
        var hash = HashToken(rawToken);
        return (rawToken, hash);
    }

    private static string HashToken(string token)
    {
        var bytes = System.Text.Encoding.UTF8.GetBytes(token);
        var hashBytes = SHA256.HashData(bytes);
        return Convert.ToBase64String(hashBytes);
    }
}
