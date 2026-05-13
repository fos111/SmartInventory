using Microsoft.EntityFrameworkCore;
using SmartInventory.Domain.Auth.Entities;
using SmartInventory.Domain.Location.Entities;
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

    // Location tables
    public DbSet<Site> Sites => Set<Site>();
    public DbSet<Zone> Zones => Set<Zone>();
    public DbSet<Building> Buildings => Set<Building>();
    public DbSet<Floor> Floors => Set<Floor>();
    public DbSet<Room> Rooms => Set<Room>();

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

        modelBuilder.Entity<AssetEntity>().HasData(
            new AssetEntity { Id = Guid.Parse("11111111-1111-1111-1111-111111111111"), AssetTag = "AST-LI-001", Name = "Dell Laptop XPS 15", Category = "Computer", Status = SmartInventory.Domain.Asset.Enums.AssetStatus.Active, Manufacturer = "Dell", Model = "XPS 15 9530", SerialNumber = "DL-XPS-001", CurrentRoomCode = "LI1", InstallDate = DateTime.UtcNow.AddMonths(-6), LastServiceDate = DateTime.UtcNow.AddMonths(-1), MaintenanceDueDate = DateTime.UtcNow.AddMonths(2) },
            new AssetEntity { Id = Guid.Parse("22222222-2222-2222-2222-222222222222"), AssetTag = "AST-LI-002", Name = "HP Monitor 24inch", Category = "Display", Status = SmartInventory.Domain.Asset.Enums.AssetStatus.Active, Manufacturer = "HP", Model = "P24h", SerialNumber = "HP-MON-002", CurrentRoomCode = "LI2", InstallDate = DateTime.UtcNow.AddMonths(-4) },
            new AssetEntity { Id = Guid.Parse("33333333-3333-3333-3333-333333333333"), AssetTag = "AST-LI-003", Name = "Canon Printer MF644", Category = "Printer/Scanner", Status = SmartInventory.Domain.Asset.Enums.AssetStatus.Active, Manufacturer = "Canon", Model = "MF644CDW", SerialNumber = "CN-PRT-003", CurrentRoomCode = "LI3", InstallDate = DateTime.UtcNow.AddMonths(-8), LastServiceDate = DateTime.UtcNow.AddMonths(-2), MaintenanceDueDate = DateTime.UtcNow.AddMonths(1) },
            new AssetEntity { Id = Guid.Parse("44444444-4444-4444-4444-444444444444"), AssetTag = "AST-LI-004", Name = "Epson Projector", Category = "Projector", Status = SmartInventory.Domain.Asset.Enums.AssetStatus.Active, Manufacturer = "Epson", Model = "EB-2250U", SerialNumber = "EP-PRJ-004", CurrentRoomCode = "LI4", InstallDate = DateTime.UtcNow.AddMonths(-12) },
            new AssetEntity { Id = Guid.Parse("55555555-5555-5555-5555-555555555555"), AssetTag = "AST-MEC-001", Name = "Dell Laptop Inspiron", Category = "Computer", Status = SmartInventory.Domain.Asset.Enums.AssetStatus.Active, Manufacturer = "Dell", Model = "Inspiron 15", SerialNumber = "DL-INS-005", CurrentRoomCode = "MEC1", InstallDate = DateTime.UtcNow.AddMonths(-3), LastServiceDate = DateTime.UtcNow.AddDays(-14) },
            new AssetEntity { Id = Guid.Parse("66666666-6666-6666-6666-666666666666"), AssetTag = "AST-MEC-002", Name = "Samsung Monitor 27inch", Category = "Display", Status = SmartInventory.Domain.Asset.Enums.AssetStatus.Active, Manufacturer = "Samsung", Model = "S27F350", SerialNumber = "SM-MON-006", CurrentRoomCode = "MEC2", InstallDate = DateTime.UtcNow.AddMonths(-5) },
            new AssetEntity { Id = Guid.Parse("77777777-7777-7777-7777-777777777777"), AssetTag = "AST-GEST-001", Name = "HP Printer LaserJet", Category = "Printer/Scanner", Status = SmartInventory.Domain.Asset.Enums.AssetStatus.Active, Manufacturer = "HP", Model = "LaserJet Pro M404n", SerialNumber = "HP-PRT-007", CurrentRoomCode = "GEST1", InstallDate = DateTime.UtcNow.AddMonths(-10), LastServiceDate = DateTime.UtcNow.AddMonths(-1), MaintenanceDueDate = DateTime.UtcNow.AddMonths(3) },
            new AssetEntity { Id = Guid.Parse("88888888-8888-8888-8888-888888888888"), AssetTag = "AST-0000003", Name = "Imprimante HP LaserJet Pro", Category = "Printer/Scanner", Status = SmartInventory.Domain.Asset.Enums.AssetStatus.Active, Manufacturer = "HP", Model = "LaserJet Pro M404n", SerialNumber = "HP-LJ-003", CurrentRoomCode = "LI1", DetectedRoomCode = "LI1", InstallDate = DateTime.UtcNow.AddMonths(-2), MaintenanceDueDate = DateTime.UtcNow.AddMonths(4), Description = "Imprimante laser couleur - 30 ppm" },
            new AssetEntity { Id = Guid.Parse("99999999-9999-9999-9999-999999999999"), AssetTag = "AST-INV-001", Name = "Dell Monitor 22inch", Category = "Display", Status = SmartInventory.Domain.Asset.Enums.AssetStatus.InStock, Manufacturer = "Dell", Model = "E2222H", SerialNumber = "DL-MON-INV-001", CurrentRoomCode = "STOCK", Description = "Nouveau moniteur en stock, pas encore déployé" }
        );
    }
}
