namespace SmartInventory.Application.PasswordReset.Interfaces;

public interface IPasswordResetService
{
    Task<bool> RequestResetAsync(string email, CancellationToken ct = default);
    Task<bool> ResetPasswordAsync(string email, string otp, string newPassword, CancellationToken ct = default);
}
