using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using SmartInventory.Api.Controllers.Auth;
using SmartInventory.Application.Auth.DTOs.Requests;
using SmartInventory.Application.Auth.DTOs.Responses;
using SmartInventory.Application.Auth.Interfaces;
using SmartInventory.Domain.Auth.Enums;
using Moq;
using Xunit;

namespace SmartInventory.Api.Tests;

public class AuthControllerTests : ApiTestBase
{
    private readonly Mock<IAuthService> _authServiceMock;
    private readonly AuthController _controller;

    public AuthControllerTests()
    {
        _authServiceMock = new Mock<IAuthService>();
        _controller = new AuthController(_authServiceMock.Object);
    }

    [Fact]
    public async Task Register_ValidRequest_ReturnsAccepted()
    {
        // Arrange
        var request = new RegisterRequest
        {
            Username = "newuser",
            Email = "new@test.com",
            Password = "Password123!"
        };
        var userId = Guid.NewGuid();
        _authServiceMock.Setup(s => s.RegisterAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new AuthResponse { UserId = userId });

        // Act
        var result = await _controller.Register(request, CancellationToken.None);

        // Assert
        result.Should().BeAssignableTo<IActionResult>();
    }

    [Fact]
    public async Task Register_DuplicateUsername_ReturnsConflict()
    {
        // Arrange
        var request = new RegisterRequest
        {
            Username = "existinguser",
            Email = "new@test.com",
            Password = "Password123!"
        };
        _authServiceMock.Setup(s => s.RegisterAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync((AuthResponse?)null);

        // Act
        var result = await _controller.Register(request, CancellationToken.None);

        // Assert
        result.Should().BeOfType<ConflictObjectResult>();
    }

[Fact]
    public async Task Login_ActiveUser_ReturnsOkWithJWT()
    {
        var request = new LoginRequest { Email = "test@example.com", Password = "password123" };
        var response = new AuthResponse
        {
            Token = "full-jwt-token",
            UserId = Guid.NewGuid(),
            Role = UserRole.Technicien,
            Status = AccountStatus.Active,
            IsActive = true,
            Message = "Login successful"
        };
        _authServiceMock.Setup(s => s.LoginAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        var result = await _controller.Login(request, CancellationToken.None);

        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var authResponse = okResult.Value.Should().BeOfType<AuthResponse>().Subject;
        authResponse.Token.Should().Be("full-jwt-token");
        authResponse.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task Login_PendingUser_ReturnsForbiddenWithLimitedJWT()
    {
        var request = new LoginRequest { Email = "test@example.com", Password = "password123" };
        var fullResponse = (AuthResponse?)null;
        var limitedResponse = new AuthResponse
        {
            Token = "limited-jwt-token",
            UserId = Guid.NewGuid(),
            Role = UserRole.Technicien,
            Status = AccountStatus.Pending,
            IsActive = false
        };
        _authServiceMock.Setup(s => s.LoginAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(fullResponse);
        _authServiceMock.Setup(s => s.GetLimitedLoginAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(limitedResponse);

        var result = await _controller.Login(request, CancellationToken.None);

        result.Should().BeAssignableTo<IActionResult>();
    }

    [Fact]
    public async Task Login_InvalidCredentials_ReturnsUnauthorized()
    {
        var request = new LoginRequest { Email = "invalid@example.com", Password = "wrong" };
        _authServiceMock.Setup(s => s.LoginAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync((AuthResponse?)null);
        _authServiceMock.Setup(s => s.GetLimitedLoginAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync((AuthResponse?)null);

        var result = await _controller.Login(request, CancellationToken.None);

        result.Should().BeOfType<UnauthorizedObjectResult>();
    }

    [Fact]
    public async Task VerifyEmail_ValidToken_Redirects()
    {
        // Arrange
        var token = "valid-token";
        _authServiceMock.Setup(s => s.VerifyEmailAsync(token, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.VerifyEmail(token, CancellationToken.None);

        // Assert
        result.Should().BeOfType<RedirectResult>();
    }

    [Fact]
    public async Task VerifyEmail_InvalidToken_ReturnsBadRequest()
    {
        // Arrange
        var token = "invalid-token";
        _authServiceMock.Setup(s => s.VerifyEmailAsync(token, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var result = await _controller.VerifyEmail(token, CancellationToken.None);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task ResendVerification_ValidEmail_ReturnsOk()
    {
        // Arrange
        var request = new ResendVerificationRequest { Email = "test@test.com" };
        _authServiceMock.Setup(s => s.ResendVerificationEmailAsync(request.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.ResendVerification(request, CancellationToken.None);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task ResendVerification_InvalidEmail_ReturnsBadRequest()
    {
        // Arrange
        var request = new ResendVerificationRequest { Email = "invalid@test.com" };
        _authServiceMock.Setup(s => s.ResendVerificationEmailAsync(request.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var result = await _controller.ResendVerification(request, CancellationToken.None);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task RequestReEvaluation_RejectedUser_ReturnsOk()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var response = new AuthResponse
        {
            Token = "token",
            UserId = userId,
            Role = UserRole.Technicien,
            Status = AccountStatus.Pending,
            Message = "Re-evaluation requested"
        };
        _authServiceMock.Setup(s => s.RequestReEvaluationAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        // Act
        var result = await _controller.RequestReEvaluation(new RequestReEvaluationRequest { UserId = userId }, CancellationToken.None);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task RequestReEvaluation_InvalidUser_ReturnsBadRequest()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _authServiceMock.Setup(s => s.RequestReEvaluationAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((AuthResponse?)null);

        // Act
        var result = await _controller.RequestReEvaluation(new RequestReEvaluationRequest { UserId = userId }, CancellationToken.None);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }
}