namespace SmartInventory.Application.PasswordReset.DTOs;

public record ResetPasswordRequest(string Email, string Otp, string NewPassword);
