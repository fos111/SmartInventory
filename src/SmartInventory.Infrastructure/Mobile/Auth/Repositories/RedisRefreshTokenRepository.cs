using System.Text.Json;
using SmartInventory.Application.Mobile.Auth.Interfaces;
using SmartInventory.Domain.Mobile.Auth.Entities;
using StackExchange.Redis;

namespace SmartInventory.Infrastructure.Mobile.Auth.Repositories;

internal sealed record RefreshTokenData
{
    public Guid Id { get; init; }
    public string TokenHash { get; init; } = "";
    public Guid UserId { get; init; }
    public DateTime ExpiresAt { get; init; }
    public DateTime CreatedAt { get; init; }
}

public class RedisRefreshTokenRepository : IRefreshTokenRepository
{
    private const string KeyPrefix = "SmartInventory:";

    private readonly IDatabase _db;
    private readonly JsonSerializerOptions _jsonOptions;

    public RedisRefreshTokenRepository(IConnectionMultiplexer connection)
    {
        _db = connection.GetDatabase();
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
    }

    public async Task<RefreshToken?> GetByTokenHashAsync(string tokenHash, CancellationToken ct = default)
    {
        var data = await _db.StringGetAsync($"{KeyPrefix}refresh:{tokenHash}");
        if (!data.HasValue) return null;

        return DeserializeToken(data!);
    }

    public async Task<List<RefreshToken>> GetActiveByUserAsync(Guid userId, CancellationToken ct = default)
    {
        var hashes = await _db.SetMembersAsync($"{KeyPrefix}refresh:user:{userId}");
        if (hashes.Length == 0) return new List<RefreshToken>();

        var tasks = hashes.Select(h => _db.StringGetAsync($"{KeyPrefix}refresh:{h}"));
        var results = await Task.WhenAll(tasks);
        var tokens = new List<RefreshToken>(results.Length);

        foreach (var data in results)
        {
            if (!data.HasValue) continue;
            var token = DeserializeToken(data!);
            if (token.IsActive) tokens.Add(token);
        }

        return tokens;
    }

    public async Task AddAsync(RefreshToken refreshToken, CancellationToken ct = default)
    {
        var hash = refreshToken.TokenHash;
        var json = JsonSerializer.Serialize(new RefreshTokenData
        {
            Id = refreshToken.Id,
            TokenHash = hash,
            UserId = refreshToken.UserId,
            ExpiresAt = refreshToken.ExpiresAt,
            CreatedAt = refreshToken.CreatedAt
        }, _jsonOptions);

        var tran = _db.CreateTransaction();
        _ = tran.StringSetAsync($"{KeyPrefix}refresh:{hash}", json);
        _ = tran.KeyExpireAsync($"{KeyPrefix}refresh:{hash}", refreshToken.ExpiresAt);
        _ = tran.SetAddAsync($"{KeyPrefix}refresh:user:{refreshToken.UserId}", hash);
        _ = tran.StringSetAsync($"{KeyPrefix}refresh:id:{refreshToken.Id}", hash, TimeSpan.FromDays(31));
        await tran.ExecuteAsync();
    }

    public async Task RevokeAsync(Guid id, CancellationToken ct = default)
    {
        var hash = await _db.StringGetAsync($"{KeyPrefix}refresh:id:{id}");
        if (!hash.HasValue) return;

        var data = await _db.StringGetAsync($"{KeyPrefix}refresh:{hash}");
        if (!data.HasValue) return;

        var token = DeserializeToken(data!);

        var batch = _db.CreateBatch();
        _ = batch.KeyDeleteAsync($"{KeyPrefix}refresh:{hash}");
        _ = batch.SetRemoveAsync($"{KeyPrefix}refresh:user:{token.UserId}", hash!);
        _ = batch.KeyDeleteAsync($"{KeyPrefix}refresh:id:{id}");
        batch.Execute();
    }

    public async Task RevokeAllForUserAsync(Guid userId, CancellationToken ct = default)
    {
        var hashes = await _db.SetMembersAsync($"{KeyPrefix}refresh:user:{userId}");
        if (hashes.Length == 0) return;

        var batch = _db.CreateBatch();
        foreach (var hash in hashes)
        {
            _ = batch.KeyDeleteAsync($"{KeyPrefix}refresh:{hash}");
        }
        _ = batch.KeyDeleteAsync($"{KeyPrefix}refresh:user:{userId}");
        batch.Execute();
    }

    private RefreshToken DeserializeToken(string data)
    {
        var dto = JsonSerializer.Deserialize<RefreshTokenData>(data, _jsonOptions)
                  ?? throw new InvalidOperationException("Failed to deserialize refresh token data.");

        return new RefreshToken(dto.Id, dto.CreatedAt)
        {
            TokenHash = dto.TokenHash,
            UserId = dto.UserId,
            ExpiresAt = dto.ExpiresAt
        };
    }
}
