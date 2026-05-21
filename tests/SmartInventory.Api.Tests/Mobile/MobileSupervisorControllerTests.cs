using AutoMapper;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using SmartInventory.Api.Controllers.Mobile;
using SmartInventory.Api.Models;
using SmartInventory.Application.Auth.Interfaces;
using SmartInventory.Application.Mobile.Auth.DTOs;
using SmartInventory.Domain.Auth.Entities;
using SmartInventory.Domain.Auth.Enums;
using Xunit;

namespace SmartInventory.Api.Tests.Mobile;

public class MobileSupervisorControllerTests
{
    private readonly Mock<IAuthRepository> _authRepositoryMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly MobileSupervisorController _controller;

    public MobileSupervisorControllerTests()
    {
        _authRepositoryMock = new Mock<IAuthRepository>();
        _mapperMock = new Mock<IMapper>();
        _controller = new MobileSupervisorController(_authRepositoryMock.Object, _mapperMock.Object);
    }

    [Fact]
    public async Task GetUsers_ReturnsAllUsers_Success()
    {
        var users = new List<User>
        {
            new User { Id = Guid.NewGuid(), Username = "Alice", Email = "alice@test.com", Role = UserRole.Technicien },
            new User { Id = Guid.NewGuid(), Username = "Bob", Email = "bob@test.com", Role = UserRole.Gestionnaire },
        };
        var userDtos = users.Select(u => new MobileUserDto { Id = u.Id, Name = u.Username, Email = u.Email, Role = "technicien", Avatar = null }).ToList();

        _authRepositoryMock.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(users);
        _mapperMock
            .Setup(m => m.Map<MobileUserDto>(It.IsAny<User>()))
            .Returns<User>(u => new MobileUserDto { Id = u.Id, Name = u.Username, Email = u.Email, Role = u.Role?.ToString() ?? "", Avatar = u.AvatarUrl });

        var result = await _controller.GetUsers(CancellationToken.None);

        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var envelope = okResult.Value.Should().BeOfType<MobileEnvelope<List<MobileUserDto>>>().Subject;
        envelope.Success.Should().BeTrue();
        envelope.Data.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetUsers_NoUsers_ReturnsEmptyList()
    {
        _authRepositoryMock.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<User>());

        var result = await _controller.GetUsers(CancellationToken.None);

        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var envelope = okResult.Value.Should().BeOfType<MobileEnvelope<List<MobileUserDto>>>().Subject;
        envelope.Success.Should().BeTrue();
        envelope.Data.Should().BeEmpty();
    }

    [Fact]
    public async Task UpdateRole_ValidRole_ReturnsOk()
    {
        var userId = Guid.NewGuid();
        var request = new UpdateRoleRequest("technicien");
        var user = new User { Id = userId, Username = "Alice", Role = UserRole.Gestionnaire };

        _authRepositoryMock.Setup(r => r.GetByIdAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync(user);

        var result = await _controller.UpdateRole(userId, request, CancellationToken.None);

        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var envelope = okResult.Value.Should().BeOfType<MobileEnvelope>().Subject;
        envelope.Success.Should().BeTrue();
        envelope.Message.Should().Be("Role updated");
    }

    [Fact]
    public async Task UpdateRole_InvalidRole_ReturnsBadRequest()
    {
        var userId = Guid.NewGuid();
        var request = new UpdateRoleRequest("supervisor");

        var result = await _controller.UpdateRole(userId, request, CancellationToken.None);

        var badRequest = result.Result.Should().BeOfType<BadRequestObjectResult>().Subject;
        var envelope = badRequest.Value.Should().BeOfType<MobileEnvelope>().Subject;
        envelope.Success.Should().BeFalse();
        envelope.Message.Should().Contain("Invalid role");
    }

    [Fact]
    public async Task UpdateRole_UserNotFound_ReturnsNotFound()
    {
        var userId = Guid.NewGuid();
        var request = new UpdateRoleRequest("technicien");

        _authRepositoryMock.Setup(r => r.GetByIdAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync((User?)null);

        var result = await _controller.UpdateRole(userId, request, CancellationToken.None);

        var notFound = result.Result.Should().BeOfType<NotFoundObjectResult>().Subject;
        var envelope = notFound.Value.Should().BeOfType<MobileEnvelope>().Subject;
        envelope.Success.Should().BeFalse();
        envelope.Message.Should().Be("User not found");
    }

    [Fact]
    public async Task UpdateStatus_ActivateUser_ReturnsOk()
    {
        var userId = Guid.NewGuid();
        var request = new UpdateStatusRequest(true);
        var user = new User { Id = userId, Status = AccountStatus.Rejected };

        _authRepositoryMock.Setup(r => r.GetByIdAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync(user);

        var result = await _controller.UpdateStatus(userId, request, CancellationToken.None);

        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var envelope = okResult.Value.Should().BeOfType<MobileEnvelope>().Subject;
        envelope.Success.Should().BeTrue();
        envelope.Message.Should().Be("Status updated");
        user.Status.Should().Be(AccountStatus.Active);
    }

    [Fact]
    public async Task UpdateStatus_DeactivateUser_ReturnsOk()
    {
        var userId = Guid.NewGuid();
        var request = new UpdateStatusRequest(false);
        var user = new User { Id = userId, Status = AccountStatus.Active };

        _authRepositoryMock.Setup(r => r.GetByIdAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync(user);

        var result = await _controller.UpdateStatus(userId, request, CancellationToken.None);

        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var envelope = okResult.Value.Should().BeOfType<MobileEnvelope>().Subject;
        envelope.Success.Should().BeTrue();
        user.Status.Should().Be(AccountStatus.Rejected);
    }

    [Fact]
    public async Task UpdateStatus_UserNotFound_ReturnsNotFound()
    {
        var userId = Guid.NewGuid();
        var request = new UpdateStatusRequest(true);

        _authRepositoryMock.Setup(r => r.GetByIdAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync((User?)null);

        var result = await _controller.UpdateStatus(userId, request, CancellationToken.None);

        var notFound = result.Result.Should().BeOfType<NotFoundObjectResult>().Subject;
        var envelope = notFound.Value.Should().BeOfType<MobileEnvelope>().Subject;
        envelope.Success.Should().BeFalse();
        envelope.Message.Should().Be("User not found");
    }

    [Fact]
    public async Task DeleteUser_ExistingUser_ReturnsOk()
    {
        var userId = Guid.NewGuid();
        var user = new User { Id = userId, Status = AccountStatus.Active };

        _authRepositoryMock.Setup(r => r.GetByIdAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync(user);

        var result = await _controller.DeleteUser(userId, CancellationToken.None);

        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var envelope = okResult.Value.Should().BeOfType<MobileEnvelope>().Subject;
        envelope.Success.Should().BeTrue();
        envelope.Message.Should().Be("User deleted");
        user.Status.Should().Be(AccountStatus.Rejected);
    }

    [Fact]
    public async Task DeleteUser_NonExistingUser_ReturnsNotFound()
    {
        var userId = Guid.NewGuid();

        _authRepositoryMock.Setup(r => r.GetByIdAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync((User?)null);

        var result = await _controller.DeleteUser(userId, CancellationToken.None);

        var notFound = result.Result.Should().BeOfType<NotFoundObjectResult>().Subject;
        var envelope = notFound.Value.Should().BeOfType<MobileEnvelope>().Subject;
        envelope.Success.Should().BeFalse();
        envelope.Message.Should().Be("User not found");
    }
}
