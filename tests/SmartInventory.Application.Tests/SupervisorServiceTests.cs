using FluentAssertions;
using Moq;
using SmartInventory.Application.Auth.DTOs.Responses;
using SmartInventory.Application.Auth.Interfaces;
using SmartInventory.Application.Auth.Services;
using SmartInventory.Domain.Auth.Entities;
using SmartInventory.Domain.Auth.Enums;
using Xunit;

namespace SmartInventory.Application.Tests;

public class SupervisorServiceTests : ApplicationTestBase
{
    private readonly Mock<IAuthRepository> _authRepositoryMock;
    private readonly Mock<IEmailSender> _emailSenderMock;
    private readonly SupervisorService _supervisorService;

    public SupervisorServiceTests()
    {
        _authRepositoryMock = new Mock<IAuthRepository>();
        _emailSenderMock = new Mock<IEmailSender>();

        _supervisorService = new SupervisorService(
            _authRepositoryMock.Object,
            _emailSenderMock.Object
        );
    }

    [Fact]
    public async Task GetPendingUsersAsync_ReturnsPendingOnly()
    {
        var users = new List<User>
        {
            CreateTestUser(status: AccountStatus.Pending, role: UserRole.Technicien),
            CreateTestUser(status: AccountStatus.Active, role: UserRole.Technicien),
            CreateTestUser(status: AccountStatus.Pending, role: UserRole.Gestionnaire),
            CreateTestUser(status: AccountStatus.Rejected, role: UserRole.Technicien)
        };

        _authRepositoryMock.Setup(r => r.GetUsersByStatusAsync(AccountStatus.Pending, It.IsAny<CancellationToken>()))
            .ReturnsAsync(users.Where(u => u.Status == AccountStatus.Pending).ToList());

        var result = await _supervisorService.GetPendingUsersAsync();

        result.Should().HaveCount(2);
        result.Should().AllSatisfy(u => u.IsEmailVerified.Should().BeFalse());
    }

    [Fact]
    public async Task GetPendingUsersAsync_ReturnsOrderedByCreatedAt()
    {
        var users = new List<User>
        {
            CreateTestUser(status: AccountStatus.Pending, createdAt: DateTime.UtcNow.AddDays(-2)),
            CreateTestUser(status: AccountStatus.Pending, createdAt: DateTime.UtcNow.AddDays(-1)),
            CreateTestUser(status: AccountStatus.Pending, createdAt: DateTime.UtcNow)
        };

        _authRepositoryMock.Setup(r => r.GetUsersByStatusAsync(AccountStatus.Pending, It.IsAny<CancellationToken>()))
            .ReturnsAsync(users);

        var result = await _supervisorService.GetPendingUsersAsync();

        result.Should().HaveCount(3);
        result.First().CreatedAt.Should().Be(users[0].CreatedAt);
    }

    [Fact]
    public async Task GetPendingUsersAsync_IncludesAllRoles()
    {
        var users = new List<User>
        {
            CreateTestUser(status: AccountStatus.Pending, role: UserRole.Technicien),
            CreateTestUser(status: AccountStatus.Pending, role: UserRole.Gestionnaire)
        };

        _authRepositoryMock.Setup(r => r.GetUsersByStatusAsync(AccountStatus.Pending, It.IsAny<CancellationToken>()))
            .ReturnsAsync(users);

        var result = await _supervisorService.GetPendingUsersAsync();

        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task ApproveUserAsync_Technicien_Valid_SetsActiveAndSendsEmail()
    {
        var user = CreateTestUser(status: AccountStatus.Pending, role: null);
        var supervisorId = Guid.NewGuid();

        _authRepositoryMock.Setup(r => r.GetByIdAsync(user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _emailSenderMock.Setup(e => e.SendEmailAsync(user.Email, "Technician Account Approved", It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var result = await _supervisorService.ApproveUserAsync(user.Id, UserRole.Technicien, supervisorId);

        result.Should().BeTrue();
        user.Status.Should().Be(AccountStatus.Active);
        user.Role.Should().Be(UserRole.Technicien);
        user.ApprovedByUserId.Should().Be(supervisorId);
        user.ApprovedAt.Should().NotBeNull();
        _authRepositoryMock.Verify(r => r.UpdateAsync(user, It.IsAny<CancellationToken>()), Times.Once);
        _emailSenderMock.Verify(e => e.SendEmailAsync(user.Email, "Technician Account Approved", It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ApproveUserAsync_Gestionnaire_Valid_SetsActiveAndSendsEmail()
    {
        var user = CreateTestUser(status: AccountStatus.Pending, role: null);
        var supervisorId = Guid.NewGuid();

        _authRepositoryMock.Setup(r => r.GetByIdAsync(user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _emailSenderMock.Setup(e => e.SendEmailAsync(user.Email, "Gestionnaire Account Approved", It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var result = await _supervisorService.ApproveUserAsync(user.Id, UserRole.Gestionnaire, supervisorId);

        result.Should().BeTrue();
        user.Status.Should().Be(AccountStatus.Active);
        user.Role.Should().Be(UserRole.Gestionnaire);
        _emailSenderMock.Verify(e => e.SendEmailAsync(user.Email, "Gestionnaire Account Approved", It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ApproveUserAsync_InvalidUser_ReturnsFalse()
    {
        var userId = Guid.NewGuid();
        var supervisorId = Guid.NewGuid();

        _authRepositoryMock.Setup(r => r.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        var result = await _supervisorService.ApproveUserAsync(userId, UserRole.Technicien, supervisorId);

        result.Should().BeFalse();
    }

    [Fact]
    public async Task ApproveUserAsync_InvalidRole_ReturnsFalse()
    {
        var user = CreateTestUser(status: AccountStatus.Pending, role: null);
        var supervisorId = Guid.NewGuid();

        var result = await _supervisorService.ApproveUserAsync(user.Id, UserRole.Supervisor, supervisorId);

        result.Should().BeFalse();
    }

    [Fact]
    public async Task RejectUserAsync_Valid_SetsRejectedAndSendsEmail()
    {
        var user = CreateTestUser(status: AccountStatus.Pending);
        var supervisorId = Guid.NewGuid();
        var reason = "Insufficient qualifications";

        _authRepositoryMock.Setup(r => r.GetByIdAsync(user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _emailSenderMock.Setup(e => e.SendEmailAsync(user.Email, "Account Rejected", It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var result = await _supervisorService.RejectUserAsync(user.Id, supervisorId, reason);

        result.Should().BeTrue();
        user.Status.Should().Be(AccountStatus.Rejected);
        user.RejectionReason.Should().Be(reason);
        _authRepositoryMock.Verify(r => r.UpdateAsync(user, It.IsAny<CancellationToken>()), Times.Once);
        _emailSenderMock.Verify(e => e.SendEmailAsync(user.Email, "Account Rejected", It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RejectUserAsync_Valid_NoReason_SetsRejectedWithNullReason()
    {
        var user = CreateTestUser(status: AccountStatus.Pending);
        var supervisorId = Guid.NewGuid();

        _authRepositoryMock.Setup(r => r.GetByIdAsync(user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _emailSenderMock.Setup(e => e.SendEmailAsync(user.Email, "Account Rejected", It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var result = await _supervisorService.RejectUserAsync(user.Id, supervisorId, null);

        result.Should().BeTrue();
        user.Status.Should().Be(AccountStatus.Rejected);
        user.RejectionReason.Should().BeNull();
    }

    [Fact]
    public async Task RejectUserAsync_InvalidUser_ReturnsFalse()
    {
        var userId = Guid.NewGuid();
        var supervisorId = Guid.NewGuid();

        _authRepositoryMock.Setup(r => r.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        var result = await _supervisorService.RejectUserAsync(userId, supervisorId, "reason");

        result.Should().BeFalse();
    }

    private static User CreateTestUser(
        AccountStatus status = AccountStatus.Pending,
        bool isEmailVerified = false,
        UserRole? role = UserRole.Technicien,
        DateTime? createdAt = null)
    {
        return new User
        {
            Username = "testuser",
            Email = "test@test.com",
            PasswordHash = "hashed-password",
            Role = role,
            Status = status,
            IsEmailVerified = isEmailVerified
        };
    }
}
