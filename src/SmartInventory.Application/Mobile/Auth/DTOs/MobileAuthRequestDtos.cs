namespace SmartInventory.Application.Mobile.Auth.DTOs;

public record MobileLoginRequest(string Email, string Password);

public record MobileRegisterRequest(
    string Name,
    string Email,
    string Password,
    string? Role = null);

public record VerifyEmailRequest(string Email, string Otp);

public record ResendVerificationRequest(string Email);

public record RefreshTokenRequest(string RefreshToken);

public record LogoutRequest(string RefreshToken);

public record GoogleAuthRequest(string IdToken);
