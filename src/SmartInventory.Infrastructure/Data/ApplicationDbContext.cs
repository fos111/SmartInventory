using Microsoft.EntityFrameworkCore;
using SmartInventory.Domain.Auth.Entities;
using SmartInventory.Domain.Location.Entities;
using SmartInventory.Domain.Mobile.Auth.Entities;
using SmartInventory.Domain.Mobile.Entities;
using AssetEntity = SmartInventory.Domain.Asset.Entities.Asset;
using CategoryGroupEntity = SmartInventory.Domain.Asset.Entities.CategoryGroup;
using AssetHistoryEntity = SmartInventory.Domain.Asset.Entities.AssetHistory;
using ActivityLogEntity = SmartInventory.Domain.Asset.Entities.ActivityLog;
using NotificationEntity = SmartInventory.Domain.Notification.Entities.Notification;
using UserPreferenceEntity = SmartInventory.Domain.UserPreferences.Entities.UserPreference;

namespace SmartInventory.Infrastructure.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) 
        : base(options)
    {
    }

    // Auth tables
    public DbSet<User> Users => Set<User>();
    public DbSet<EmailVerificationToken> EmailVerificationTokens => Set<EmailVerificationToken>();

    // Mobile auth tables
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<PasswordResetToken> PasswordResetTokens => Set<PasswordResetToken>();

    // Location tables
    public DbSet<Site> Sites => Set<Site>();
    public DbSet<Zone> Zones => Set<Zone>();
    public DbSet<Building> Buildings => Set<Building>();
    public DbSet<Floor> Floors => Set<Floor>();
    public DbSet<Room> Rooms => Set<Room>();
    public DbSet<ZoneSiteShape> ZoneSiteShapes => Set<ZoneSiteShape>();

    // Asset tables
    public DbSet<AssetEntity> Assets => Set<AssetEntity>();
    public DbSet<CategoryGroupEntity> CategoryGroups => Set<CategoryGroupEntity>();
    public DbSet<AssetLocationHistory> AssetLocationHistories => Set<AssetLocationHistory>();
    public DbSet<AssetHistoryEntity> AssetHistories => Set<AssetHistoryEntity>();
    public DbSet<ActivityLogEntity> ActivityLogs => Set<ActivityLogEntity>();

    // Notification tables
    public DbSet<NotificationEntity> Notifications => Set<NotificationEntity>();

    // User preferences tables
    public DbSet<UserPreferenceEntity> UserPreferences => Set<UserPreferenceEntity>();

    // Mobile sync queue tables
    public DbSet<SyncQueueEntry> SyncQueueEntries => Set<SyncQueueEntry>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply configurations from Infrastructure assembly
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);

        // Global query filter for soft delete
        modelBuilder.Entity<AssetEntity>().HasQueryFilter(a => a.DeletedAt == null);

        // Auth configuration (inline)
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasIndex(e => e.Username).IsUnique();
            entity.HasIndex(e => e.Email).IsUnique();
            entity.Property(e => e.Username).HasMaxLength(100);
            entity.Property(e => e.Email).HasMaxLength(255);
            entity.Property(e => e.PasswordHash).HasMaxLength(500);
        });

        modelBuilder.Entity<EmailVerificationToken>(entity =>
        {
            entity.HasIndex(e => e.Token).IsUnique();
            entity.HasOne(e => e.User)
                  .WithMany(u => u.EmailVerificationTokens)
                  .HasForeignKey(e => e.UserId);
        });

        // Asset seed data removed to prevent migration drift.
        // See earlier migrations for seed data (raw InsertData).
    }
}
