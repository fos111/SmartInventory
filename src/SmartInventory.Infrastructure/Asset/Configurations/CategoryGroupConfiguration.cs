using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using AssetEntity = SmartInventory.Domain.Asset.Entities.CategoryGroup;

namespace SmartInventory.Infrastructure.Asset.Configurations;

public class CategoryGroupConfiguration : IEntityTypeConfiguration<AssetEntity>
{
    public void Configure(EntityTypeBuilder<AssetEntity> builder)
    {
        builder.ToTable("CategoryGroups");
        builder.HasKey(cg => cg.Id);
        builder.HasIndex(cg => cg.Category).IsUnique();
        builder.Property(cg => cg.Category).HasMaxLength(50).IsRequired();
        builder.Property(cg => cg.Group).HasMaxLength(50).IsRequired();
    }
}