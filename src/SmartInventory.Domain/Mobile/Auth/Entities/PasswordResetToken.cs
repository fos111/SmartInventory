using SmartInventory.Domain.Entities;

namespace SmartInventory.Domain.Mobile.Auth.Entities;

public class PasswordResetToken : Entity
{
    /// <summary>
    /// For EF Core and service layer creation.
    /// </summary>
    public PasswordResetToken() { }

    /// <summary>
    /// For deserializing cached tokens from Redis with original Id and CreatedAt.
    /// </summary>
    public PasswordResetToken(Guid id, DateTime createdAt)
    {
        Id = id;
        CreatedAt = createdAt;
    }

    public string Email { get; set; } = string.Empty;
    public string Otp { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public DateTime? UsedAt { get; set; }
    public bool IsUsed => UsedAt.HasValue;
    public bool IsExpired => DateTime.UtcNow > ExpiresAt;
}
