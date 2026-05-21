using AutoMapper;
using FluentAssertions;
using Moq;
using SmartInventory.Application.Auth.Interfaces;
using SmartInventory.Application.Mobile.Auth.DTOs;
using SmartInventory.Application.Mobile.Auth.Helpers;
using SmartInventory.Application.Mobile.Auth.Interfaces;
using SmartInventory.Application.Mobile.Auth.Services;
using SmartInventory.Domain.Auth.Entities;
using SmartInventory.Domain.Auth.Enums;
using Xunit;

namespace SmartInventory.Application.Tests.Mobile.Auth.Services;

public class MobileAuthServiceTests
{
    private readonly Mock<IAuthRepository> _authRepoMock;
    private readonly Mock<IPasswordHasher> _passwordHasherMock;
    private readonly Mock<IEmailVerificationService> _emailVerificationServiceMock;
    private readonly Mock<IRefreshTokenService> _refreshTokenServiceMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly MobileAuthService _service;

    public MobileAuthServiceTests()
    {
        _authRepoMock = new Mock<IAuthRepository>();
        _passwordHasherMock = new Mock<IPasswordHasher>();
        _emailVerificationServiceMock = new Mock<IEmailVerificationService>();
        _refreshTokenServiceMock = new Mock<IRefreshTokenService>();
        _mapperMock = new Mock<IMapper>();

        _service = new MobileAuthService(
            _authRepoMock.Object,
            _passwordHasherMock.Object,
            _emailVerificationServiceMock.Object,
            _refreshTokenServiceMock.Object,
            _mapperMock.Object);
    }

    [Fact]
    public async Task RegisterAsync_ValidRequest_CreatesActiveUser()
    {
        var request = new MobileRegisterRequest(
            "John Doe",
            "john@test.com",
            "password123",
            "technicien");

        _authRepoMock
            .Setup(r => r.ExistsByEmailAsync(request.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _authRepoMock
            .Setup(r => r.ExistsAsync(request.Name, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _passwordHasherMock
            .Setup(p => p.Hash(request.Password))
            .Returns("hashed-password");
        _emailVerificationServiceMock
            .Setup(e => e.GenerateToken())
            .Returns("otp-code");
        _emailVerificationServiceMock
            .Setup(e => e.SendVerificationEmailAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _authRepoMock
            .Setup(r => r.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask)
            .Callback<User, CancellationToken>((user, _) =>
            {
                user.Id = Guid.NewGuid();
            });

        var result = await _service.RegisterAsync(request);

        result.Should().NotBeNull();
        result!.RequiresVerification.Should().BeTrue();

        _authRepoMock.Verify(r => r.AddAsync(It.Is<User>(u =>
            u.Username == request.Name &&
            u.Email == request.Email &&
            u.Role == UserRole.Technicien &&
            u.Status == AccountStatus.Active &&
            u.IsEmailVerified == false), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RegisterAsync_DuplicateEmail_ReturnsNull()
    {
        var request = new MobileRegisterRequest("John", "existing@test.com", "password123", null);

        _authRepoMock
            .Setup(r => r.ExistsByEmailAsync(request.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var result = await _service.RegisterAsync(request);

        result.Should().BeNull();
        _authRepoMock.Verify(r => r.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task RegisterAsync_DefaultRoleIsTechnicien()
    {
        var request = new MobileRegisterRequest("Jane", "jane@test.com", "password123", null);

        _authRepoMock
            .Setup(r => r.ExistsByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _authRepoMock
            .Setup(r => r.ExistsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _passwordHasherMock
            .Setup(p => p.Hash(It.IsAny<string>()))
            .Returns("hash");
        _emailVerificationServiceMock
            .Setup(e => e.GenerateToken())
            .Returns("otp");
        _emailVerificationServiceMock
            .Setup(e => e.SendVerificationEmailAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var result = await _service.RegisterAsync(request);

        result.Should().NotBeNull();
        _authRepoMock.Verify(r => r.AddAsync(It.Is<User>(u =>
            u.Role == UserRole.Technicien), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task LoginAsync_ValidCredentials_ReturnsUser()
    {
        var user = new User
        {
            Email = "test@test.com",
            PasswordHash = "hashed-password",
            Status = AccountStatus.Active,
            IsEmailVerified = true
        };
        var request = new MobileLoginRequest("test@test.com", "password123");

        _authRepoMock
            .Setup(r => r.GetByEmailAsync(request.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _passwordHasherMock
            .Setup(p => p.Verify(request.Password, user.PasswordHash))
            .Returns(true);

        var (resultUser, needsVerification) = await _service.LoginAsync(request);

        resultUser.Should().NotBeNull();
        needsVerification.Should().BeFalse();
    }

    [Fact]
    public async Task LoginAsync_InvalidPassword_ReturnsNull()
    {
        var user = new User { Email = "test@test.com", PasswordHash = "hashed" };
        var request = new MobileLoginRequest("test@test.com", "wrong");

        _authRepoMock
            .Setup(r => r.GetByEmailAsync(request.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _passwordHasherMock
            .Setup(p => p.Verify(request.Password, user.PasswordHash))
            .Returns(false);

        var (resultUser, _) = await _service.LoginAsync(request);

        resultUser.Should().BeNull();
    }

    [Fact]
    public async Task LoginAsync_EmailNotVerified_ReturnsNeedsVerification()
    {
        var user = new User
        {
            Email = "test@test.com",
            PasswordHash = "hashed",
            IsEmailVerified = false
        };
        var request = new MobileLoginRequest("test@test.com", "password123");

        _authRepoMock
            .Setup(r => r.GetByEmailAsync(request.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _passwordHasherMock
            .Setup(p => p.Verify(request.Password, user.PasswordHash))
            .Returns(true);

        var (resultUser, needsVerification) = await _service.LoginAsync(request);

        resultUser.Should().NotBeNull();
        needsVerification.Should().BeTrue();
    }

    [Fact]
    public async Task GetProfileAsync_ExistingUser_ReturnsMappedDto()
    {
        var userId = Guid.NewGuid();
        var user = new User
        {
            Id = userId,
            Username = "John Doe",
            Email = "john@test.com",
            Role = UserRole.Technicien
        };
        var expectedDto = new MobileUserDto { Id = userId, Name = "John Doe", Email = "john@test.com", Role = "technicien", Avatar = null };

        _authRepoMock
            .Setup(r => r.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _mapperMock
            .Setup(m => m.Map<MobileUserDto>(user))
            .Returns(expectedDto);

        var result = await _service.GetProfileAsync(userId);

        result.Should().NotBeNull();
        result!.Name.Should().Be("John Doe");
        result.Email.Should().Be("john@test.com");
        result.Role.Should().Be("technicien");
    }

    [Fact]
    public async Task GetProfileAsync_NonExistentUser_ReturnsNull()
    {
        var userId = Guid.NewGuid();

        _authRepoMock
            .Setup(r => r.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        var result = await _service.GetProfileAsync(userId);

        result.Should().BeNull();
    }

    [Fact]
    public async Task UpdateAvatarAsync_ExistingUser_UpdatesAvatar()
    {
        var userId = Guid.NewGuid();
        var user = new User { Id = userId, Username = "test" };
        var avatarUrl = "/uploads/avatars/test.jpg";

        _authRepoMock
            .Setup(r => r.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var result = await _service.UpdateAvatarAsync(userId, avatarUrl);

        result.Should().Be(avatarUrl);
        user.AvatarUrl.Should().Be(avatarUrl);
        _authRepoMock.Verify(r => r.UpdateAsync(user, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateAvatarAsync_NonExistentUser_ReturnsNull()
    {
        _authRepoMock
            .Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        var result = await _service.UpdateAvatarAsync(Guid.NewGuid(), "url");

        result.Should().BeNull();
    }
}
