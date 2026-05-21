namespace SmartInventory.Application.Mobile.Auth.DTOs;

public record TokenPairDto(
    string AccessToken,
    string RefreshToken,
    int ExpiresIn);

public class MobileUserDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string? Avatar { get; set; }
}

public record RegisterResultDto(
    Guid UserId,
    bool RequiresVerification,
    string Email);
