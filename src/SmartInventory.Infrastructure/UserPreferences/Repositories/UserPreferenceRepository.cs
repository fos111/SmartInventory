using Microsoft.EntityFrameworkCore;
using SmartInventory.Application.UserPreferences.Interfaces;
using SmartInventory.Domain.UserPreferences.Entities;
using SmartInventory.Infrastructure.Data;

namespace SmartInventory.Infrastructure.UserPreferences.Repositories;

public class UserPreferenceRepository : IUserPreferenceRepository
{
    private readonly ApplicationDbContext _context;

    public UserPreferenceRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Dictionary<string, string>> GetUserPreferencesAsync(Guid userId)
    {
        return await _context.UserPreferences
            .AsNoTracking()
            .Where(p => p.UserId == userId)
            .ToDictionaryAsync(p => p.Key, p => p.Value);
    }

    public async Task UpsertAsync(Guid userId, Dictionary<string, string> preferences)
    {
        var existingKeys = await _context.UserPreferences
            .Where(p => p.UserId == userId && preferences.Keys.Contains(p.Key))
            .ToListAsync();

        var existingKeyMap = existingKeys.ToDictionary(p => p.Key);

        foreach (var (key, value) in preferences)
        {
            if (existingKeyMap.TryGetValue(key, out var existing))
            {
                existing.Value = value;
                existing.UpdatedAt = DateTime.UtcNow;
            }
            else
            {
                _context.UserPreferences.Add(new UserPreference
                {
                    UserId = userId,
                    Key = key,
                    Value = value,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                });
            }
        }

        await _context.SaveChangesAsync();
    }
}
