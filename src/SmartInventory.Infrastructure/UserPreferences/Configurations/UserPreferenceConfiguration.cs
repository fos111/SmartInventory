using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartInventory.Domain.UserPreferences.Entities;

namespace SmartInventory.Infrastructure.UserPreferences.Configurations;

public class UserPreferenceConfiguration : IEntityTypeConfiguration<UserPreference>
{
    public void Configure(EntityTypeBuilder<UserPreference> builder)
    {
        builder.ToTable("UserPreferences");
        builder.HasKey(p => p.Id);
        builder.HasIndex(p => new { p.UserId, p.Key }).IsUnique();
        builder.Property(p => p.Key).HasMaxLength(100).IsRequired();
        builder.Property(p => p.Value).HasMaxLength(500).IsRequired();
        builder.Property(p => p.CreatedAt).IsRequired();
        builder.Property(p => p.UpdatedAt).IsRequired();

        builder.HasOne(p => p.User)
               .WithMany(u => u.Preferences)
               .HasForeignKey(p => p.UserId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}
