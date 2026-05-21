using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using SmartInventory.Api.Controllers.Mobile;
using SmartInventory.Api.Models;
using SmartInventory.Application.Mobile.Auth.DTOs;
using SmartInventory.Application.Mobile.Auth.Interfaces;
using SmartInventory.Application.PasswordReset.DTOs;
using SmartInventory.Application.PasswordReset.Interfaces;
using SmartInventory.Domain.Auth.Entities;
using SmartInventory.Domain.Auth.Enums;
using Xunit;

namespace SmartInventory.Api.Tests.Mobile;

public class MobileAuthControllerTests
{
    private readonly Mock<IMobileAuthService> _mobileAuthServiceMock;
    private readonly Mock<IRefreshTokenService> _refreshTokenServiceMock;
    private readonly Mock<IPasswordResetService> _passwordResetServiceMock;
    private readonly MobileAuthController _controller;

    public MobileAuthControllerTests()
    {
        _mobileAuthServiceMock = new Mock<IMobileAuthService>();
        _refreshTokenServiceMock = new Mock<IRefreshTokenService>();
        _passwordResetServiceMock = new Mock<IPasswordResetService>();
        _controller = new MobileAuthController(
            _mobileAuthServiceMock.Object,
            _refreshTokenServiceMock.Object,
            _passwordResetServiceMock.Object);
    }

    [Fact]
    public async Task Register_ValidRequest_ReturnsOkWithEnvelope()
    {
        var request = new MobileRegisterRequest("John", "john@test.com", "password123", null);
        var resultDto = new RegisterResultDto(Guid.NewGuid(), true, "john@test.com");

        _mobileAuthServiceMock
            .Setup(s => s.RegisterAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(resultDto);

        var result = await _controller.Register(request, CancellationToken.None);

        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var envelope = okResult.Value.Should().BeOfType<MobileEnvelope<RegisterResultDto>>().Subject;
        envelope.Success.Should().BeTrue();
        envelope.Data.Should().Be(resultDto);
    }

    [Fact]
    public async Task Register_Duplicate_ReturnsConflict()
    {
        var request = new MobileRegisterRequest("John", "existing@test.com", "password123", null);

        _mobileAuthServiceMock
            .Setup(s => s.RegisterAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync((RegisterResultDto?)null);

        var result = await _controller.Register(request, CancellationToken.None);

        var conflictResult = result.Result.Should().BeOfType<ConflictObjectResult>().Subject;
        var envelope = conflictResult.Value.Should().BeOfType<MobileEnvelope<RegisterResultDto>>().Subject;
        envelope.Success.Should().BeFalse();
    }

    [Fact]
    public async Task Login_ValidCredentials_ReturnsTokenPair()
    {
        var request = new MobileLoginRequest("john@test.com", "password123");
        var user = new User { Id = Guid.NewGuid(), Status = AccountStatus.Active, IsEmailVerified = true };
        var tokenPair = new TokenPairDto("access-token", "refresh-token", 3600);

        _mobileAuthServiceMock
            .Setup(s => s.LoginAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync((user, false));
        _refreshTokenServiceMock
            .Setup(s => s.CreateTokenPairAsync(user, It.IsAny<CancellationToken>()))
            .ReturnsAsync(tokenPair);

        var result = await _controller.Login(request, CancellationToken.None);

        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var envelope = okResult.Value.Should().BeOfType<MobileEnvelope<TokenPairDto>>().Subject;
        envelope.Success.Should().BeTrue();
        envelope.Data!.AccessToken.Should().Be("access-token");
    }

    [Fact]
    public async Task Login_InvalidCredentials_ReturnsFailure()
    {
        var request = new MobileLoginRequest("john@test.com", "wrong");

        _mobileAuthServiceMock
            .Setup(s => s.LoginAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(((User?)null, false));

        var result = await _controller.Login(request, CancellationToken.None);

        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var envelope = okResult.Value.Should().BeOfType<MobileEnvelope<TokenPairDto>>().Subject;
        envelope.Success.Should().BeFalse();
        envelope.Message.Should().Be("Invalid credentials");
    }

    [Fact]
    public async Task Login_EmailNotVerified_ReturnsNeedsVerification()
    {
        var request = new MobileLoginRequest("john@test.com", "password123");
        var user = new User { Email = "john@test.com", IsEmailVerified = false };

        _mobileAuthServiceMock
            .Setup(s => s.LoginAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync((user, true));

        var result = await _controller.Login(request, CancellationToken.None);

        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var envelope = okResult.Value.Should().BeOfType<MobileEnvelope<TokenPairDto>>().Subject;
        envelope.Success.Should().BeFalse();
        envelope.NeedsVerification.Should().BeTrue();
        envelope.Email.Should().Be("john@test.com");
    }

    [Fact]
    public async Task VerifyEmail_ValidOtp_ReturnsTokenPair()
    {
        var request = new VerifyEmailRequest("john@test.com", "123456");
        var tokenPair = new TokenPairDto("access", "refresh", 3600);

        _mobileAuthServiceMock
            .Setup(s => s.VerifyEmailAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(tokenPair);

        var result = await _controller.VerifyEmail(request, CancellationToken.None);

        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var envelope = okResult.Value.Should().BeOfType<MobileEnvelope<TokenPairDto>>().Subject;
        envelope.Success.Should().BeTrue();
        envelope.Data.Should().Be(tokenPair);
    }

    [Fact]
    public async Task VerifyEmail_InvalidOtp_ReturnsBadRequest()
    {
        var request = new VerifyEmailRequest("john@test.com", "000000");

        _mobileAuthServiceMock
            .Setup(s => s.VerifyEmailAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync((TokenPairDto?)null);

        var result = await _controller.VerifyEmail(request, CancellationToken.None);

        var badRequest = result.Result.Should().BeOfType<BadRequestObjectResult>().Subject;
        var envelope = badRequest.Value.Should().BeOfType<MobileEnvelope<TokenPairDto>>().Subject;
        envelope.Success.Should().BeFalse();
    }

    [Fact]
    public async Task Refresh_ValidToken_ReturnsNewPair()
    {
        var request = new RefreshTokenRequest("valid-refresh-token");
        var tokenPair = new TokenPairDto("new-access", "new-refresh", 3600);

        _refreshTokenServiceMock
            .Setup(s => s.RefreshAsync(request.RefreshToken, It.IsAny<CancellationToken>()))
            .ReturnsAsync(tokenPair);

        var result = await _controller.Refresh(request, CancellationToken.None);

        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var envelope = okResult.Value.Should().BeOfType<MobileEnvelope<TokenPairDto>>().Subject;
        envelope.Success.Should().BeTrue();
    }

    [Fact]
    public async Task Refresh_InvalidToken_ReturnsUnauthorized()
    {
        var request = new RefreshTokenRequest("invalid-token");

        _refreshTokenServiceMock
            .Setup(s => s.RefreshAsync(request.RefreshToken, It.IsAny<CancellationToken>()))
            .ReturnsAsync((TokenPairDto?)null);

        var result = await _controller.Refresh(request, CancellationToken.None);

        var unauthorized = result.Result.Should().BeOfType<UnauthorizedObjectResult>().Subject;
        var envelope = unauthorized.Value.Should().BeOfType<MobileEnvelope<TokenPairDto>>().Subject;
        envelope.Success.Should().BeFalse();
    }

    [Fact]
    public async Task Logout_ReturnsOk()
    {
        var request = new LogoutRequest("some-token");
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
        };

        var result = await _controller.Logout(request, CancellationToken.None);

        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var envelope = okResult.Value.Should().BeOfType<MobileEnvelope>().Subject;
        envelope.Success.Should().BeTrue();
    }

    [Fact]
    public async Task ForgotPassword_ValidEmail_ReturnsOk()
    {
        var request = new ForgotPasswordRequest("john@test.com");

        _passwordResetServiceMock
            .Setup(s => s.RequestResetAsync(request.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var result = await _controller.ForgotPassword(request, CancellationToken.None);

        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var envelope = okResult.Value.Should().BeOfType<MobileEnvelope>().Subject;
        envelope.Success.Should().BeTrue();
    }

    [Fact]
    public async Task ForgotPassword_NonExistentEmail_ReturnsBadRequest()
    {
        var request = new ForgotPasswordRequest("unknown@test.com");

        _passwordResetServiceMock
            .Setup(s => s.RequestResetAsync(request.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var result = await _controller.ForgotPassword(request, CancellationToken.None);

        var badRequest = result.Result.Should().BeOfType<BadRequestObjectResult>().Subject;
        var envelope = badRequest.Value.Should().BeOfType<MobileEnvelope>().Subject;
        envelope.Success.Should().BeFalse();
    }

    [Fact]
    public async Task ResetPassword_ValidOtp_ReturnsOk()
    {
        var request = new ResetPasswordRequest("john@test.com", "123456", "new-password");

        _passwordResetServiceMock
            .Setup(s => s.ResetPasswordAsync(request.Email, request.Otp, request.NewPassword, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var result = await _controller.ResetPassword(request, CancellationToken.None);

        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var envelope = okResult.Value.Should().BeOfType<MobileEnvelope>().Subject;
        envelope.Success.Should().BeTrue();
    }

    [Fact]
    public async Task ResetPassword_InvalidOtp_ReturnsBadRequest()
    {
        var request = new ResetPasswordRequest("john@test.com", "000000", "new-password");

        _passwordResetServiceMock
            .Setup(s => s.ResetPasswordAsync(request.Email, request.Otp, request.NewPassword, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var result = await _controller.ResetPassword(request, CancellationToken.None);

        var badRequest = result.Result.Should().BeOfType<BadRequestObjectResult>().Subject;
        var envelope = badRequest.Value.Should().BeOfType<MobileEnvelope>().Subject;
        envelope.Success.Should().BeFalse();
    }
}
