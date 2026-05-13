using SmartInventory.Domain.Auth.Enums;

namespace SmartInventory.Application.Auth.DTOs.Requests;

public class LoginRequest
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public class RegisterRequest
{
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}

public class ResendVerificationRequest
{
    public string Email { get; set; } = string.Empty;
}

public class RequestReEvaluationRequest
{
    public Guid UserId { get; set; }
}

public class RejectUserRequest
{
    public string? Reason { get; set; }
}

public class ApproveUserRequest
{
    public UserRole Role { get; set; }
}