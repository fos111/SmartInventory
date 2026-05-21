using SmartInventory.Domain.Auth.Entities;
using SmartInventory.Domain.Auth.Enums;

namespace SmartInventory.Application.Auth.Interfaces;

public interface IAuthRepository
{
    Task<User?> GetByUsernameAsync(string username, CancellationToken ct = default);
    Task<User?> GetByEmailAsync(string email, CancellationToken ct = default);
    Task<User?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<User?> GetUserByVerificationTokenAsync(string token, CancellationToken ct = default);
    Task<bool> ExistsAsync(string username, CancellationToken ct = default);
    Task<bool> ExistsByEmailAsync(string email, CancellationToken ct = default);
    Task AddAsync(User user, CancellationToken ct = default);
    Task UpdateAsync(User user, CancellationToken ct = default);
    Task<List<User>> GetAllAsync(CancellationToken ct = default);
    Task<List<User>> GetUsersByRoleAsync(UserRole role, CancellationToken ct = default);
    Task<List<User>> GetUsersByStatusAsync(AccountStatus status, CancellationToken ct = default);
}