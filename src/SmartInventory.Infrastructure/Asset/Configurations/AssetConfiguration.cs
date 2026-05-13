using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using AssetEntity = SmartInventory.Domain.Asset.Entities.Asset;

namespace SmartInventory.Infrastructure.Asset.Configurations;

public class AssetConfiguration : IEntityTypeConfiguration<AssetEntity>
{
    public void Configure(EntityTypeBuilder<AssetEntity> builder)
    {
        builder.ToTable("Assets");
        builder.HasKey(a => a.Id);
        
        builder.HasIndex(a => a.AssetTag).IsUnique();
        builder.HasIndex(a => a.RfidTagId).IsUnique();
        builder.HasIndex(a => a.SerialNumber).IsUnique();
        
        builder.Property(a => a.AssetTag).HasMaxLength(50).IsRequired();
        builder.Property(a => a.Name).HasMaxLength(100).IsRequired();
        builder.Property(a => a.Description).HasMaxLength(500);
        builder.Property(a => a.Category).HasMaxLength(50).IsRequired();
        builder.Property(a => a.Manufacturer).HasMaxLength(100);
        builder.Property(a => a.Model).HasMaxLength(100);
        builder.Property(a => a.SerialNumber).HasMaxLength(100);
        builder.Property(a => a.CurrentRoomCode).HasMaxLength(20).IsRequired();
        builder.Property(a => a.DetectedRoomCode).HasMaxLength(20);
        builder.Property(a => a.RfidTagId).HasMaxLength(100);
    }
}