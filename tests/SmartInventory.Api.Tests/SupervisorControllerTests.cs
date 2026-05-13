using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using SmartInventory.Api.Controllers.Auth;
using SmartInventory.Application.Auth.DTOs.Requests;
using SmartInventory.Application.Auth.DTOs.Responses;
using SmartInventory.Application.Auth.Interfaces;
using SmartInventory.Domain.Auth.Enums;
using Moq;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Xunit;

namespace SmartInventory.Api.Tests;

public class SupervisorControllerTests : ApiTestBase
{
    private readonly Mock<ISupervisorService> _supervisorServiceMock;
    private readonly SupervisorController _controller;
    private readonly Guid _supervisorId;

    public SupervisorControllerTests()
    {
        _supervisorServiceMock = new Mock<ISupervisorService>();
        _supervisorId = Guid.NewGuid();
        _controller = new SupervisorController(_supervisorServiceMock.Object);

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, _supervisorId.ToString()),
            new Claim(ClaimTypes.Name, "supervisor"),
            new Claim(ClaimTypes.Role, "Supervisor")
        };
        var identity = new ClaimsIdentity(claims, "Test");
        var principal = new ClaimsPrincipal(identity);
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = principal }
        };
    }

    [Fact]
    public async Task GetPendingUsers_ReturnsOk()
    {
        var pendingUsers = new List<UserListResponse>
        {
            new UserListResponse
            {
                Id = Guid.NewGuid(),
                Username = "user1",
                Email = "user1@test.com",
                CreatedAt = DateTime.UtcNow,
                IsEmailVerified = false
            },
            new UserListResponse
            {
                Id = Guid.NewGuid(),
                Username = "user2",
                Email = "user2@test.com",
                CreatedAt = DateTime.UtcNow,
                IsEmailVerified = false
            }
        };
        _supervisorServiceMock.Setup(s => s.GetPendingUsersAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(pendingUsers);

        var result = await _controller.GetPendingUsers(CancellationToken.None);

        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeOfType<List<UserListResponse>>().Subject;
        response.Should().HaveCount(2);
    }

    [Fact]
    public async Task ApproveUser_ValidId_ReturnsOk()
    {
        var userId = Guid.NewGuid();
        var request = new ApproveUserRequest { Role = UserRole.Technicien };
        _supervisorServiceMock.Setup(s => s.ApproveUserAsync(userId, request.Role, _supervisorId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var result = await _controller.ApproveUser(userId, request, CancellationToken.None);

        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task ApproveUser_InvalidId_ReturnsBadRequest()
    {
        var userId = Guid.NewGuid();
        var request = new ApproveUserRequest { Role = UserRole.Technicien };
        _supervisorServiceMock.Setup(s => s.ApproveUserAsync(userId, request.Role, _supervisorId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var result = await _controller.ApproveUser(userId, request, CancellationToken.None);

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task RejectUser_ValidId_ReturnsOk()
    {
        var userId = Guid.NewGuid();
        var request = new RejectUserRequest { Reason = "Not qualified" };
        _supervisorServiceMock.Setup(s => s.RejectUserAsync(userId, _supervisorId, request.Reason, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var result = await _controller.RejectUser(userId, request, CancellationToken.None);

        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task RejectUser_InvalidId_ReturnsBadRequest()
    {
        var userId = Guid.NewGuid();
        var request = new RejectUserRequest { Reason = "Not qualified" };
        _supervisorServiceMock.Setup(s => s.RejectUserAsync(userId, _supervisorId, request.Reason, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var result = await _controller.RejectUser(userId, request, CancellationToken.None);

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task RejectUser_WithoutReason_ReturnsOk()
    {
        var userId = Guid.NewGuid();
        var request = new RejectUserRequest { Reason = null };
        _supervisorServiceMock.Setup(s => s.RejectUserAsync(userId, _supervisorId, request.Reason, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var result = await _controller.RejectUser(userId, request, CancellationToken.None);

        result.Should().BeOfType<OkObjectResult>();
    }
}
