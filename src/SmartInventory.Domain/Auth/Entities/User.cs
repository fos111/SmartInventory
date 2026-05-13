using SmartInventory.Domain.Entities;
using SmartInventory.Domain.Auth.Enums;
using SmartInventory.Domain.UserPreferences.Entities;

namespace SmartInventory.Domain.Auth.Entities;

public class User : Entity
{
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public UserRole? Role { get; set; }
    public AccountStatus Status { get; set; } = AccountStatus.Pending;
    public bool IsEmailVerified { get; set; } = false;
    public Guid? ApprovedByUserId { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public string? RejectionReason { get; set; }

    public new Guid Id { get; set; }

    public ICollection<EmailVerificationToken> EmailVerificationTokens { get; set; } = new List<EmailVerificationToken>();
    public ICollection<UserPreference> Preferences { get; set; } = new List<UserPreference>();
}