using FluentAssertions;
using Moq;
using SmartInventory.Application.Auth.Interfaces;
using SmartInventory.Application.Mobile.Auth.DTOs;
using SmartInventory.Application.Mobile.Auth.Interfaces;
using SmartInventory.Application.Mobile.Auth.Services;
using SmartInventory.Domain.Auth.Entities;
using SmartInventory.Domain.Auth.Enums;
using SmartInventory.Domain.Mobile.Auth.Entities;
using Xunit;

namespace SmartInventory.Application.Tests.Mobile.Auth.Services;

public class RefreshTokenServiceTests
{
    private readonly Mock<IRefreshTokenRepository> _refreshTokenRepoMock;
    private readonly Mock<ITokenService> _tokenServiceMock;
    private readonly Mock<IAuthRepository> _authRepoMock;
    private readonly RefreshTokenService _service;

    public RefreshTokenServiceTests()
    {
        _refreshTokenRepoMock = new Mock<IRefreshTokenRepository>();
        _tokenServiceMock = new Mock<ITokenService>();
        _authRepoMock = new Mock<IAuthRepository>();
        _service = new RefreshTokenService(
            _refreshTokenRepoMock.Object,
            _tokenServiceMock.Object,
            _authRepoMock.Object);
    }

    private static User CreateTestUser()
    {
        return new User
        {
            Username = "testuser",
            Email = "test@test.com",
            PasswordHash = "hashed",
            Role = UserRole.Technicien,
            Status = AccountStatus.Active,
            IsEmailVerified = true
        };
    }

    [Fact]
    public async Task CreateTokenPairAsync_ValidUser_ReturnsTokenPair()
    {
        var user = CreateTestUser();
        _tokenServiceMock.Setup(t => t.GenerateToken(user)).Returns("jwt-token");

        var result = await _service.CreateTokenPairAsync(user);

        result.Should().NotBeNull();
        result.AccessToken.Should().Be("jwt-token");
        result.RefreshToken.Should().NotBeNullOrEmpty();
        result.ExpiresIn.Should().Be(3600);

        _refreshTokenRepoMock.Verify(
            r => r.AddAsync(It.IsAny<RefreshToken>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task RefreshAsync_ValidToken_ReturnsNewTokenPair()
    {
        var user = CreateTestUser();
        var oldRefreshToken = new RefreshToken
        {
            TokenHash = "hash-of-old-token",
            UserId = user.Id,
            ExpiresAt = DateTime.UtcNow.AddDays(30)
        };

        _refreshTokenRepoMock
            .Setup(r => r.GetByTokenHashAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(oldRefreshToken);
        _authRepoMock
            .Setup(r => r.GetByIdAsync(user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _tokenServiceMock.Setup(t => t.GenerateToken(user)).Returns("new-jwt-token");

        var result = await _service.RefreshAsync("valid-refresh-token");

        result.Should().NotBeNull();
        result!.AccessToken.Should().Be("new-jwt-token");
        result.RefreshToken.Should().NotBeNullOrEmpty();

        _refreshTokenRepoMock.Verify(
            r => r.RevokeAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task RefreshAsync_ExpiredToken_ReturnsNull()
    {
        var expiredToken = new RefreshToken
        {
            TokenHash = "hash-of-expired",
            UserId = Guid.NewGuid(),
            ExpiresAt = DateTime.UtcNow.AddDays(-1)
        };

        _refreshTokenRepoMock
            .Setup(r => r.GetByTokenHashAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expiredToken);

        var result = await _service.RefreshAsync("expired-refresh-token");

        result.Should().BeNull();
    }

    [Fact]
    public async Task RefreshAsync_RevokedToken_ReturnsNull()
    {
        var revokedToken = new RefreshToken
        {
            TokenHash = "hash-of-revoked",
            UserId = Guid.NewGuid(),
            ExpiresAt = DateTime.UtcNow.AddDays(30),
            RevokedAt = DateTime.UtcNow.AddDays(-1)
        };

        _refreshTokenRepoMock
            .Setup(r => r.GetByTokenHashAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(revokedToken);

        var result = await _service.RefreshAsync("revoked-refresh-token");

        result.Should().BeNull();
    }

    [Fact]
    public async Task RefreshAsync_NonExistentToken_ReturnsNull()
    {
        _refreshTokenRepoMock
            .Setup(r => r.GetByTokenHashAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((RefreshToken?)null);

        var result = await _service.RefreshAsync("non-existent-token");

        result.Should().BeNull();
    }

    [Fact]
    public async Task RevokeAsync_ValidToken_RevokesSuccessfully()
    {
        var token = new RefreshToken
        {
            TokenHash = "hash-of-token",
            UserId = Guid.NewGuid(),
            ExpiresAt = DateTime.UtcNow.AddDays(30)
        };

        _refreshTokenRepoMock
            .Setup(r => r.GetByTokenHashAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(token);

        await _service.RevokeAsync("valid-refresh-token");

        _refreshTokenRepoMock.Verify(
            r => r.RevokeAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task RevokeAllForUserAsync_RevokesAllTokens()
    {
        var userId = Guid.NewGuid();

        await _service.RevokeAllForUserAsync(userId);

        _refreshTokenRepoMock.Verify(
            r => r.RevokeAllForUserAsync(userId, It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
