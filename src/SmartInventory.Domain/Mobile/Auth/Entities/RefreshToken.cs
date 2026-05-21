using SmartInventory.Domain.Auth.Entities;
using SmartInventory.Domain.Entities;

namespace SmartInventory.Domain.Mobile.Auth.Entities;

public class RefreshToken : Entity
{
    /// <summary>
    /// For EF Core and service layer creation.
    /// </summary>
    public RefreshToken() { }

    /// <summary>
    /// For deserializing cached tokens from Redis with the original Id and CreatedAt.
    /// </summary>
    public RefreshToken(Guid id, DateTime createdAt)
    {
        Id = id;
        CreatedAt = createdAt;
    }

    public string TokenHash { get; set; } = string.Empty;
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;
    public DateTime ExpiresAt { get; set; }
    public DateTime? RevokedAt { get; set; }
    public bool IsExpired => DateTime.UtcNow > ExpiresAt;
    public bool IsRevoked => RevokedAt.HasValue;
    public bool IsActive => !IsExpired && !IsRevoked;
}
