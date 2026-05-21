using System.Text.Json;
using SmartInventory.Application.Mobile.Auth.Interfaces;
using SmartInventory.Domain.Mobile.Auth.Entities;
using StackExchange.Redis;

namespace SmartInventory.Infrastructure.Mobile.Auth.Repositories;

internal sealed record OtpData
{
    public Guid Id { get; init; }
    public string Otp { get; init; } = "";
    public string Email { get; init; } = "";
    public DateTime ExpiresAt { get; init; }
    public DateTime CreatedAt { get; init; }
}

public class RedisPasswordResetTokenRepository : IPasswordResetTokenRepository
{
    private const string KeyPrefix = "SmartInventory:";
    private static readonly TimeSpan OtpTtl = TimeSpan.FromMinutes(10);

    private readonly IDatabase _db;
    private readonly JsonSerializerOptions _jsonOptions;

    public RedisPasswordResetTokenRepository(IConnectionMultiplexer connection)
    {
        _db = connection.GetDatabase();
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
    }

    public async Task<PasswordResetToken?> GetValidByEmailAndOtpAsync(string email, string otp, CancellationToken ct = default)
    {
        var data = await _db.StringGetAsync($"{KeyPrefix}otp:{email.Normalize().ToLowerInvariant()}");
        if (!data.HasValue) return null;

        var dto = JsonSerializer.Deserialize<OtpData>(data!, _jsonOptions);
        if (dto == null || dto.Otp != otp) return null;

        return new PasswordResetToken(dto.Id, dto.CreatedAt)
        {
            Email = dto.Email,
            Otp = dto.Otp,
            ExpiresAt = dto.ExpiresAt
        };
    }

    public async Task AddAsync(PasswordResetToken token, CancellationToken ct = default)
    {
        var emailKey = $"{KeyPrefix}otp:{token.Email.Normalize().ToLowerInvariant()}";
        var json = JsonSerializer.Serialize(new OtpData
        {
            Id = token.Id,
            Otp = token.Otp,
            Email = token.Email,
            ExpiresAt = token.ExpiresAt,
            CreatedAt = token.CreatedAt
        }, _jsonOptions);

        var tran = _db.CreateTransaction();
        _ = tran.StringSetAsync(emailKey, json);
        _ = tran.KeyExpireAsync(emailKey, OtpTtl);
        _ = tran.StringSetAsync($"{KeyPrefix}otp:id:{token.Id}", token.Email.Normalize().ToLowerInvariant(), OtpTtl);
        await tran.ExecuteAsync();
    }

    public async Task MarkAsUsedAsync(Guid id, CancellationToken ct = default)
    {
        var email = await _db.StringGetAsync($"{KeyPrefix}otp:id:{id}");
        if (!email.HasValue) return;

        var batch = _db.CreateBatch();
        _ = batch.KeyDeleteAsync($"{KeyPrefix}otp:{email}");
        _ = batch.KeyDeleteAsync($"{KeyPrefix}otp:id:{id}");
        batch.Execute();
    }

    public async Task InvalidateByEmailAsync(string email, CancellationToken ct = default)
    {
        var emailKey = $"{KeyPrefix}otp:{email.Normalize().ToLowerInvariant()}";

        var data = await _db.StringGetAsync(emailKey);
        if (!data.HasValue) return;

        var dto = JsonSerializer.Deserialize<OtpData>(data!, _jsonOptions);
        if (dto == null) return;

        var batch = _db.CreateBatch();
        _ = batch.KeyDeleteAsync(emailKey);
        _ = batch.KeyDeleteAsync($"{KeyPrefix}otp:id:{dto.Id}");
        batch.Execute();
    }
}
