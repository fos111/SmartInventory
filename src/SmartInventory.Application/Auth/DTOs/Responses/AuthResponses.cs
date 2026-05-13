using SmartInventory.Domain.Auth.Enums;

namespace SmartInventory.Application.Auth.DTOs.Responses;

public class AuthResponse
{
    public string Token { get; set; } = string.Empty;
    public Guid UserId { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public UserRole? Role { get; set; }
    public AccountStatus Status { get; set; }
    public bool IsActive { get; set; }
    public string Message { get; set; } = string.Empty;
}

public class UserListResponse
{
    public Guid Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public bool IsEmailVerified { get; set; }
}