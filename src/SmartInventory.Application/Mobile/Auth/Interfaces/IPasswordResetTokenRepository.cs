using SmartInventory.Domain.Mobile.Auth.Entities;

namespace SmartInventory.Application.Mobile.Auth.Interfaces;

public interface IPasswordResetTokenRepository
{
    Task<PasswordResetToken?> GetValidByEmailAndOtpAsync(string email, string otp, CancellationToken ct = default);
    Task AddAsync(PasswordResetToken token, CancellationToken ct = default);
    Task MarkAsUsedAsync(Guid id, CancellationToken ct = default);
    Task InvalidateByEmailAsync(string email, CancellationToken ct = default);
}
