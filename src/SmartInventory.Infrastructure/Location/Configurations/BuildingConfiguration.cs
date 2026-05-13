using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartInventory.Domain.Location.Entities;

namespace SmartInventory.Infrastructure.Location.Configurations
{
    public class BuildingConfiguration : IEntityTypeConfiguration<Building>
    {
        public void Configure(EntityTypeBuilder<Building> builder)
        {
            builder.ToTable("Buildings");
            builder.HasKey(b => b.Id);
            builder.HasIndex(b => new { b.ZoneId, b.Code }).IsUnique();
            builder.Property(b => b.Code).HasMaxLength(50).IsRequired();
            builder.Property(b => b.Name).HasMaxLength(200).IsRequired();
            builder.Property(b => b.Description).HasMaxLength(500);

            builder.HasOne(b => b.Zone)
                .WithMany(z => z.Buildings)
                .HasForeignKey(b => b.ZoneId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}