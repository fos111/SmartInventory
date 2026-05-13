using Microsoft.EntityFrameworkCore;
using SmartInventory.Application.Auth.Interfaces;
using SmartInventory.Domain.Auth.Entities;
using SmartInventory.Domain.Auth.Enums;
using SmartInventory.Infrastructure.Data;

namespace SmartInventory.Infrastructure.Auth.Repositories;

public class AuthRepository : IAuthRepository
{
    private readonly ApplicationDbContext _context;

    public AuthRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<User?> GetByUsernameAsync(string username, CancellationToken ct = default)
    {
        return await _context.Users.FirstOrDefaultAsync(u => u.Username == username, ct);
    }

    public async Task<User?> GetByEmailAsync(string email, CancellationToken ct = default)
    {
        return await _context.Users.FirstOrDefaultAsync(u => u.Email == email, ct);
    }

    public async Task<User?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _context.Users.FindAsync(new object[] { id }, ct);
    }

    public async Task<User?> GetUserByVerificationTokenAsync(string token, CancellationToken ct = default)
    {
        var verificationToken = await _context.EmailVerificationTokens
            .Include(t => t.User)
            .FirstOrDefaultAsync(t => t.Token == token, ct);
        return verificationToken?.User;
    }

    public async Task<bool> ExistsAsync(string username, CancellationToken ct = default)
    {
        return await _context.Users.AnyAsync(u => u.Username == username, ct);
    }

    public async Task<bool> ExistsByEmailAsync(string email, CancellationToken ct = default)
    {
        return await _context.Users.AnyAsync(u => u.Email == email, ct);
    }

    public async Task AddAsync(User user, CancellationToken ct = default)
    {
        await _context.Users.AddAsync(user, ct);
        await _context.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(User user, CancellationToken ct = default)
    {
        _context.Users.Update(user);
        await _context.SaveChangesAsync(ct);
    }

    public async Task<List<User>> GetUsersByRoleAsync(UserRole role, CancellationToken ct = default)
    {
        return await _context.Users
            .Where(u => u.Role == role)
            .ToListAsync(ct);
    }

    public async Task<List<User>> GetUsersByStatusAsync(AccountStatus status, CancellationToken ct = default)
    {
        return await _context.Users
            .Where(u => u.Status == status)
            .OrderBy(u => u.CreatedAt)
            .ToListAsync(ct);
    }
}