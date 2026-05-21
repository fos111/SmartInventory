using FluentAssertions;
using Moq;
using SmartInventory.Application.Auth.Interfaces;
using SmartInventory.Application.Mobile.Auth.Interfaces;
using SmartInventory.Application.PasswordReset.Interfaces;
using SmartInventory.Application.PasswordReset.Services;
using SmartInventory.Domain.Auth.Entities;
using SmartInventory.Domain.Auth.Enums;
using SmartInventory.Domain.Mobile.Auth.Entities;
using Xunit;

namespace SmartInventory.Application.Tests.Mobile.Auth.Services;

public class PasswordResetServiceTests
{
    private readonly Mock<IPasswordResetTokenRepository> _tokenRepoMock;
    private readonly Mock<IAuthRepository> _authRepoMock;
    private readonly Mock<IPasswordHasher> _passwordHasherMock;
    private readonly Mock<IEmailSender> _emailSenderMock;
    private readonly PasswordResetService _service;

    public PasswordResetServiceTests()
    {
        _tokenRepoMock = new Mock<IPasswordResetTokenRepository>();
        _authRepoMock = new Mock<IAuthRepository>();
        _passwordHasherMock = new Mock<IPasswordHasher>();
        _emailSenderMock = new Mock<IEmailSender>();
        _service = new PasswordResetService(
            _tokenRepoMock.Object,
            _authRepoMock.Object,
            _passwordHasherMock.Object,
            _emailSenderMock.Object);
    }

    [Fact]
    public async Task RequestResetAsync_ValidEmail_GeneratesOtpAndSendsEmail()
    {
        var email = "test@test.com";
        var user = new User
        {
            Email = email,
            Username = "testuser",
            Status = AccountStatus.Active
        };

        _authRepoMock
            .Setup(r => r.GetByEmailAsync(email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _emailSenderMock
            .Setup(e => e.SendEmailAsync(email, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var result = await _service.RequestResetAsync(email);

        result.Should().BeTrue();
        _tokenRepoMock.Verify(
            r => r.AddAsync(It.IsAny<PasswordResetToken>(), It.IsAny<CancellationToken>()),
            Times.Once);
        _emailSenderMock.Verify(
            e => e.SendEmailAsync(email, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task RequestResetAsync_NonExistentEmail_ReturnsFalse()
    {
        _authRepoMock
            .Setup(r => r.GetByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        var result = await _service.RequestResetAsync("nonexistent@test.com");

        result.Should().BeFalse();
        _tokenRepoMock.Verify(
            r => r.AddAsync(It.IsAny<PasswordResetToken>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task RequestResetAsync_InvalidatesPreviousOtps()
    {
        var email = "test@test.com";
        var user = new User { Email = email, Username = "testuser", Status = AccountStatus.Active };

        _authRepoMock.Setup(r => r.GetByEmailAsync(email, It.IsAny<CancellationToken>())).ReturnsAsync(user);
        _emailSenderMock.Setup(e => e.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        await _service.RequestResetAsync(email);

        _tokenRepoMock.Verify(
            r => r.InvalidateByEmailAsync(email, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task ResetPasswordAsync_ValidOtp_UpdatesPassword()
    {
        var email = "test@test.com";
        var otp = "123456";
        var newPassword = "new-password";
        var hashedPassword = "hashed-new-password";
        var token = new PasswordResetToken
        {
            Email = email,
            Otp = otp,
            ExpiresAt = DateTime.UtcNow.AddMinutes(5)
        };
        var user = new User { Email = email, PasswordHash = "old-hash" };

        _tokenRepoMock
            .Setup(r => r.GetValidByEmailAndOtpAsync(email, otp, It.IsAny<CancellationToken>()))
            .ReturnsAsync(token);
        _authRepoMock
            .Setup(r => r.GetByEmailAsync(email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _passwordHasherMock
            .Setup(p => p.Hash(newPassword))
            .Returns(hashedPassword);

        var result = await _service.ResetPasswordAsync(email, otp, newPassword);

        result.Should().BeTrue();
        user.PasswordHash.Should().Be(hashedPassword);
        _authRepoMock.Verify(r => r.UpdateAsync(user, It.IsAny<CancellationToken>()), Times.Once);
        _tokenRepoMock.Verify(r => r.MarkAsUsedAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ResetPasswordAsync_InvalidOtp_ReturnsFalse()
    {
        _tokenRepoMock
            .Setup(r => r.GetValidByEmailAndOtpAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((PasswordResetToken?)null);

        var result = await _service.ResetPasswordAsync("test@test.com", "000000", "new-password");

        result.Should().BeFalse();
        _authRepoMock.Verify(r => r.UpdateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
