using FluentAssertions;
using Moq;
using SmartInventory.Application.Auth.DTOs.Requests;
using SmartInventory.Application.Auth.Interfaces;
using SmartInventory.Application.Auth.Services;
using SmartInventory.Domain.Auth.Entities;
using SmartInventory.Domain.Auth.Enums;
using Xunit;

namespace SmartInventory.Application.Tests;

public class AuthServiceTests : ApplicationTestBase
{
    private readonly Mock<IAuthRepository> _authRepositoryMock;
    private readonly Mock<IPasswordHasher> _passwordHasherMock;
    private readonly Mock<ITokenService> _tokenServiceMock;
    private readonly Mock<IEmailVerificationService> _emailVerificationServiceMock;
    private readonly Mock<IEmailSender> _emailSenderMock;
    private readonly AuthService _authService;

    public AuthServiceTests()
    {
        _authRepositoryMock = new Mock<IAuthRepository>();
        _passwordHasherMock = new Mock<IPasswordHasher>();
        _tokenServiceMock = new Mock<ITokenService>();
        _emailVerificationServiceMock = new Mock<IEmailVerificationService>();
        _emailSenderMock = new Mock<IEmailSender>();

        _authService = new AuthService(
            _authRepositoryMock.Object,
            _passwordHasherMock.Object,
            _tokenServiceMock.Object,
            _emailVerificationServiceMock.Object,
            _emailSenderMock.Object
        );
    }

    [Fact]
    public async Task LoginAsync_ValidCredentials_ReturnsToken()
    {
        var user = CreateTestUser(status: AccountStatus.Active, isEmailVerified: true);
        var request = new LoginRequest { Email = "test@example.com", Password = "password123" };

        _authRepositoryMock.Setup(r => r.GetByEmailAsync("test@example.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _passwordHasherMock.Setup(p => p.Verify("password123", user.PasswordHash)).Returns(true);
        _tokenServiceMock.Setup(t => t.GenerateToken(user)).Returns("jwt-token");

        var result = await _authService.LoginAsync(request);

        result.Should().NotBeNull();
        result!.Token.Should().Be("jwt-token");
        result.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task LoginAsync_InvalidPassword_ReturnsNull()
    {
        var user = CreateTestUser(status: AccountStatus.Active, isEmailVerified: true);
        var request = new LoginRequest { Email = "test@example.com", Password = "wrongpassword" };

        _authRepositoryMock.Setup(r => r.GetByEmailAsync("test@example.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _passwordHasherMock.Setup(p => p.Verify("wrongpassword", user.PasswordHash)).Returns(false);

        var result = await _authService.LoginAsync(request);

        result.Should().BeNull();
    }

    [Fact]
    public async Task LoginAsync_EmailNotVerified_ReturnsRestrictedResponse()
    {
        var user = CreateTestUser(status: AccountStatus.Pending, isEmailVerified: false);
        var request = new LoginRequest { Email = "test@example.com", Password = "password123" };

        _authRepositoryMock.Setup(r => r.GetByEmailAsync("test@example.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _passwordHasherMock.Setup(p => p.Verify("password123", user.PasswordHash)).Returns(true);

        var result = await _authService.LoginAsync(request);

        result.Should().NotBeNull();
        result!.IsActive.Should().BeFalse();
    }

    [Fact]
    public async Task LoginAsync_AccountNotActive_ReturnsRestrictedResponse()
    {
        var user = CreateTestUser(status: AccountStatus.Pending, isEmailVerified: true);
        var request = new LoginRequest { Email = "test@example.com", Password = "password123" };

        _authRepositoryMock.Setup(r => r.GetByEmailAsync("test@example.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _passwordHasherMock.Setup(p => p.Verify("password123", user.PasswordHash)).Returns(true);

        var result = await _authService.LoginAsync(request);

        result.Should().NotBeNull();
        result!.IsActive.Should().BeFalse();
    }

    [Fact]
    public async Task RegisterAsync_ValidRequest_CreatesUserAndSendsEmail()
    {
        var request = new RegisterRequest
        {
            Username = "newuser",
            Password = "password123",
            Email = "newuser@test.com"
        };

        _authRepositoryMock.Setup(r => r.ExistsAsync("newuser", It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _authRepositoryMock.Setup(r => r.ExistsByEmailAsync("newuser@test.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _passwordHasherMock.Setup(p => p.Hash("password123")).Returns("hashed-password");
        _emailVerificationServiceMock.Setup(e => e.SendVerificationEmailAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var result = await _authService.RegisterAsync(request);

        result.Should().NotBeNull();
        result!.Username.Should().Be("newuser");
        _authRepositoryMock.Verify(r => r.AddAsync(It.Is<User>(u => 
            u.Username == "newuser" && 
            u.Email == "newuser@test.com" &&
            u.Role == null &&
            u.Status == AccountStatus.Pending &&
            !u.IsEmailVerified
        ), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RegisterAsync_DuplicateUsername_ReturnsNull()
    {
        var request = new RegisterRequest
        {
            Username = "existinguser",
            Password = "password123",
            Email = "new@test.com"
        };

        _authRepositoryMock.Setup(r => r.ExistsAsync("existinguser", It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var result = await _authService.RegisterAsync(request);

        result.Should().BeNull();
    }

    [Fact]
    public async Task RegisterAsync_DuplicateEmail_ReturnsNull()
    {
        var request = new RegisterRequest
        {
            Username = "newuser",
            Password = "password123",
            Email = "existing@test.com"
        };

        _authRepositoryMock.Setup(r => r.ExistsAsync("newuser", It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _authRepositoryMock.Setup(r => r.ExistsByEmailAsync("existing@test.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var result = await _authService.RegisterAsync(request);

        result.Should().BeNull();
    }

    [Fact]
    public async Task VerifyEmailAsync_ValidToken_MarksUserVerified()
    {
        var user = CreateTestUser(isEmailVerified: false);
        var token = "valid-token";

        _emailVerificationServiceMock.Setup(e => e.ValidateTokenAsync(token, It.IsAny<CancellationToken>()))
            .ReturnsAsync((true, string.Empty));
        _authRepositoryMock.Setup(r => r.GetUserByVerificationTokenAsync(token, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _emailVerificationServiceMock.Setup(e => e.MarkTokenAsUsedAsync(token, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var result = await _authService.VerifyEmailAsync(token);

        result.Should().BeTrue();
        user.IsEmailVerified.Should().BeTrue();
        _authRepositoryMock.Verify(r => r.UpdateAsync(user, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task VerifyEmailAsync_InvalidToken_ReturnsFalse()
    {
        var token = "invalid-token";

        _emailVerificationServiceMock.Setup(e => e.ValidateTokenAsync(token, It.IsAny<CancellationToken>()))
            .ReturnsAsync((false, "Invalid token"));

        var result = await _authService.VerifyEmailAsync(token);

        result.Should().BeFalse();
    }

    [Fact]
    public async Task ResendVerificationEmailAsync_ValidEmail_SendsEmail()
    {
        var user = CreateTestUser(isEmailVerified: false);

        _authRepositoryMock.Setup(r => r.GetByEmailAsync("test@test.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _emailVerificationServiceMock.Setup(e => e.SendVerificationEmailAsync(user, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var result = await _authService.ResendVerificationEmailAsync("test@test.com");

        result.Should().BeTrue();
        _emailVerificationServiceMock.Verify(e => e.SendVerificationEmailAsync(user, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ResendVerificationEmailAsync_AlreadyVerified_ReturnsFalse()
    {
        var user = CreateTestUser(isEmailVerified: true);

        _authRepositoryMock.Setup(r => r.GetByEmailAsync("test@test.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var result = await _authService.ResendVerificationEmailAsync("test@test.com");

        result.Should().BeFalse();
    }

    [Fact]
    public async Task RequestReEvaluationAsync_RejectedUser_SetsPending()
    {
        var user = CreateTestUser(status: AccountStatus.Rejected);

        _authRepositoryMock.Setup(r => r.GetByIdAsync(user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var result = await _authService.RequestReEvaluationAsync(user.Id);

        result.Should().NotBeNull();
        result!.Status.Should().Be(AccountStatus.Pending);
        user.Status.Should().Be(AccountStatus.Pending);
        _authRepositoryMock.Verify(r => r.UpdateAsync(user, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RequestReEvaluationAsync_NonRejectedUser_ReturnsNull()
    {
        var user = CreateTestUser(status: AccountStatus.Active);

        _authRepositoryMock.Setup(r => r.GetByIdAsync(user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var result = await _authService.RequestReEvaluationAsync(user.Id);

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetLimitedLoginAsync_PendingUser_ReturnsLimitedToken()
    {
        var user = CreateTestUser(status: AccountStatus.Pending);
        var request = new LoginRequest { Email = "test@example.com", Password = "password123" };

        _authRepositoryMock.Setup(r => r.GetByEmailAsync("test@example.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _passwordHasherMock.Setup(p => p.Verify("password123", user.PasswordHash)).Returns(true);
        _tokenServiceMock.Setup(t => t.GenerateLimitedToken(user)).Returns("limited-token");

        var result = await _authService.GetLimitedLoginAsync(request);

        result.Should().NotBeNull();
        result!.Token.Should().Be("limited-token");
    }

    private static User CreateTestUser(AccountStatus status = AccountStatus.Pending, bool isEmailVerified = false)
    {
        return new User
        {
            Username = "testuser",
            Email = "test@test.com",
            PasswordHash = "hashed-password",
            Role = UserRole.Technicien,
            Status = status,
            IsEmailVerified = isEmailVerified
        };
    }
}
