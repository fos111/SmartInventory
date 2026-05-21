using Microsoft.EntityFrameworkCore;
using SmartInventory.Application.Mobile.Auth.Interfaces;
using SmartInventory.Domain.Mobile.Auth.Entities;
using SmartInventory.Infrastructure.Data;

namespace SmartInventory.Infrastructure.Mobile.Auth.Repositories;

public class PasswordResetTokenRepository : IPasswordResetTokenRepository
{
    private readonly ApplicationDbContext _context;

    public PasswordResetTokenRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PasswordResetToken?> GetValidByEmailAndOtpAsync(string email, string otp, CancellationToken ct = default)
    {
        return await _context.PasswordResetTokens
            .Where(t => t.Email == email
                     && t.Otp == otp
                     && t.UsedAt == null
                     && t.ExpiresAt > DateTime.UtcNow)
            .OrderByDescending(t => t.CreatedAt)
            .FirstOrDefaultAsync(ct);
    }

    public async Task AddAsync(PasswordResetToken token, CancellationToken ct = default)
    {
        await _context.PasswordResetTokens.AddAsync(token, ct);
        await _context.SaveChangesAsync(ct);
    }

    public async Task MarkAsUsedAsync(Guid id, CancellationToken ct = default)
    {
        var token = await _context.PasswordResetTokens.FindAsync(new object[] { id }, ct);
        if (token != null)
        {
            token.UsedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync(ct);
        }
    }

    public async Task InvalidateByEmailAsync(string email, CancellationToken ct = default)
    {
        var validTokens = await _context.PasswordResetTokens
            .Where(t => t.Email == email && t.UsedAt == null && t.ExpiresAt > DateTime.UtcNow)
            .ToListAsync(ct);

        foreach (var token in validTokens)
        {
            token.UsedAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync(ct);
    }
}
