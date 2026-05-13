using FluentAssertions;
using SmartInventory.Domain.Auth.Entities;
using SmartInventory.Domain.Auth.Enums;
using Xunit;

namespace SmartInventory.Domain.Tests;

public class EntityTests : DomainTestBase
{
    [Fact]
    public void NewEntity_ShouldHaveGeneratedId()
    {
        var entity = new TestEntity();
        Assert.NotEqual(Guid.Empty, entity.Id);
    }

    private class TestEntity : Domain.Entities.Entity
    {
    }
}

public class UserTests : DomainTestBase
{
    [Fact]
    public void NewUser_DefaultRole_IsNull()
    {
        var user = new User();

        user.Role.Should().BeNull();
    }

    [Fact]
    public void NewUser_DefaultStatus_IsPending()
    {
        var user = new User();

        user.Status.Should().Be(AccountStatus.Pending);
    }

    [Fact]
    public void NewUser_DefaultIsEmailVerified_IsFalse()
    {
        var user = new User();

        user.IsEmailVerified.Should().BeFalse();
    }

    [Fact]
    public void User_CanSetAllProperties()
    {
        var user = new User
        {
            Username = "testuser",
            Email = "test@test.com",
            PasswordHash = "hashed",
            Role = UserRole.Supervisor,
            Status = AccountStatus.Active,
            IsEmailVerified = true,
            ApprovedByUserId = Guid.NewGuid(),
            ApprovedAt = DateTime.UtcNow,
            RejectionReason = "Some reason"
        };

        user.Username.Should().Be("testuser");
        user.Email.Should().Be("test@test.com");
        user.PasswordHash.Should().Be("hashed");
        user.Role.Should().Be(UserRole.Supervisor);
        user.Status.Should().Be(AccountStatus.Active);
        user.IsEmailVerified.Should().BeTrue();
        user.ApprovedByUserId.Should().NotBeNull();
        user.ApprovedAt.Should().NotBeNull();
        user.RejectionReason.Should().Be("Some reason");
    }

    [Fact]
    public void User_EmailVerificationTokens_NavigationPropertyInitialized()
    {
        var user = new User();

        user.EmailVerificationTokens.Should().NotBeNull();
        user.EmailVerificationTokens.Should().BeEmpty();
    }
}

public class EmailVerificationTokenTests : DomainTestBase
{
    [Fact]
    public void Token_DefaultValues_AreCorrect()
    {
        var token = new EmailVerificationToken();

        token.Token.Should().BeEmpty();
        token.ExpiresAt.Should().Be(default);
        token.UsedAt.Should().BeNull();
    }

    [Fact]
    public void IsUsed_WhenUsedAtIsNull_ReturnsFalse()
    {
        var token = new EmailVerificationToken
        {
            UsedAt = null
        };

        token.IsUsed.Should().BeFalse();
    }

    [Fact]
    public void IsUsed_WhenUsedAtHasValue_ReturnsTrue()
    {
        var token = new EmailVerificationToken
        {
            UsedAt = DateTime.UtcNow
        };

        token.IsUsed.Should().BeTrue();
    }

    [Fact]
    public void IsExpired_WhenBeforeExpiry_ReturnsFalse()
    {
        var token = new EmailVerificationToken
        {
            ExpiresAt = DateTime.UtcNow.AddHours(1)
        };

        token.IsExpired.Should().BeFalse();
    }

    [Fact]
    public void IsExpired_WhenAfterExpiry_ReturnsTrue()
    {
        var token = new EmailVerificationToken
        {
            ExpiresAt = DateTime.UtcNow.AddHours(-1)
        };

        token.IsExpired.Should().BeTrue();
    }

    [Fact]
    public void IsExpired_WhenExactlyAtExpiry_ReturnsTrue()
    {
        var token = new EmailVerificationToken
        {
            ExpiresAt = DateTime.UtcNow.AddSeconds(-1)
        };

        token.IsExpired.Should().BeTrue();
    }
}
