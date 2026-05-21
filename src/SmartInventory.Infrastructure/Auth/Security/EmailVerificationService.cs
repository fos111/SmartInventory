using Microsoft.EntityFrameworkCore;
using SmartInventory.Application.Auth.Interfaces;
using SmartInventory.Domain.Auth.Entities;
using SmartInventory.Infrastructure.Data;

namespace SmartInventory.Infrastructure.Auth.Security;

public class EmailVerificationService : IEmailVerificationService
{
    private readonly ApplicationDbContext _context;
    private readonly IEmailSender _emailSender;
    private const int TokenExpiryHours = 24;
    private const string BaseUrl = "http://localhost:3000";

    public EmailVerificationService(ApplicationDbContext context, IEmailSender emailSender)
    {
        _context = context;
        _emailSender = emailSender;
    }

    public string GenerateToken()
    {
        var bytes = new byte[32];
        using var rng = System.Security.Cryptography.RandomNumberGenerator.Create();
        rng.GetBytes(bytes);
        return Convert.ToBase64String(bytes)
            .Replace("+", "-")
            .Replace("/", "_")
            .TrimEnd('=');
    }

    public async Task SendVerificationEmailAsync(User user, CancellationToken ct = default)
    {
        var token = GenerateToken();
        var expiresAt = DateTime.UtcNow.AddHours(TokenExpiryHours);

        var verificationToken = new EmailVerificationToken
        {
            Token = token,
            UserId = user.Id,
            ExpiresAt = expiresAt
        };

        _context.EmailVerificationTokens.Add(verificationToken);
        await _context.SaveChangesAsync(ct);

        var verifyUrl = $"{BaseUrl}/verify-email?token={token}";
        await _emailSender.SendVerificationEmailAsync(user.Email, verifyUrl, ct);
    }

    public async Task<(bool Success, string Error)> ValidateTokenAsync(string token, CancellationToken ct = default)
    {
        var verificationToken = await _context.EmailVerificationTokens
            .FirstOrDefaultAsync(t => t.Token == token, ct);

        if (verificationToken == null)
            return (false, "Invalid token");

        if (verificationToken.IsUsed)
            return (false, "Token already used");

        if (verificationToken.IsExpired)
            return (false, "Token expired");

        return (true, string.Empty);
    }

    public async Task MarkTokenAsUsedAsync(string token, CancellationToken ct = default)
    {
        var verificationToken = await _context.EmailVerificationTokens
            .FirstOrDefaultAsync(t => t.Token == token, ct);

        if (verificationToken != null)
        {
            verificationToken.UsedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync(ct);
        }
    }
}