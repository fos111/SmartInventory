using System.Security.Cryptography;
using SmartInventory.Application.Auth.Interfaces;
using SmartInventory.Application.Mobile.Auth.Interfaces;
using SmartInventory.Application.PasswordReset.Interfaces;
using SmartInventory.Domain.Mobile.Auth.Entities;

namespace SmartInventory.Application.PasswordReset.Services;

public class PasswordResetService : IPasswordResetService
{
    private readonly IPasswordResetTokenRepository _tokenRepository;
    private readonly IAuthRepository _authRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IEmailSender _emailSender;

    private const int OtpExpiryMinutes = 10;

    public PasswordResetService(
        IPasswordResetTokenRepository tokenRepository,
        IAuthRepository authRepository,
        IPasswordHasher passwordHasher,
        IEmailSender emailSender)
    {
        _tokenRepository = tokenRepository;
        _authRepository = authRepository;
        _passwordHasher = passwordHasher;
        _emailSender = emailSender;
    }

    public async Task<bool> RequestResetAsync(string email, CancellationToken ct = default)
    {
        var user = await _authRepository.GetByEmailAsync(email, ct);
        if (user == null)
            return false;

        await _tokenRepository.InvalidateByEmailAsync(email, ct);

        var otp = GenerateOtp();
        var resetToken = new PasswordResetToken
        {
            Email = email,
            Otp = otp,
            ExpiresAt = DateTime.UtcNow.AddMinutes(OtpExpiryMinutes)
        };

        await _tokenRepository.AddAsync(resetToken, ct);

        var subject = "Password Reset Code";
        var body = $"""
            <h2>Password Reset</h2>
            <p>Your password reset code is:</p>
            <h1 style="font-size: 32px; letter-spacing: 4px; text-align: center;">{otp}</h1>
            <p>This code expires in {OtpExpiryMinutes} minutes.</p>
            <p>If you did not request this, please ignore this email.</p>
            """;

        await _emailSender.SendEmailAsync(email, subject, body, ct);

        return true;
    }

    public async Task<bool> ResetPasswordAsync(string email, string otp, string newPassword, CancellationToken ct = default)
    {
        var validToken = await _tokenRepository.GetValidByEmailAndOtpAsync(email, otp, ct);
        if (validToken == null)
            return false;

        var user = await _authRepository.GetByEmailAsync(email, ct);
        if (user == null)
            return false;

        user.PasswordHash = _passwordHasher.Hash(newPassword);
        await _authRepository.UpdateAsync(user, ct);
        await _tokenRepository.MarkAsUsedAsync(validToken.Id, ct);

        return true;
    }

    private static string GenerateOtp()
    {
        var randomBytes = new byte[4];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomBytes);
        var value = BitConverter.ToUInt32(randomBytes, 0) % 1_000_000;
        return value.ToString("D6");
    }
}
