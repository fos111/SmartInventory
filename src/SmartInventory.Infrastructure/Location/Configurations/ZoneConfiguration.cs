using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartInventory.Domain.Location.Entities;

namespace SmartInventory.Infrastructure.Location.Configurations
{
    public class ZoneConfiguration : IEntityTypeConfiguration<Zone>
    {
        public void Configure(EntityTypeBuilder<Zone> builder)
        {
            builder.ToTable("Zones");
            builder.HasKey(z => z.Id);
            builder.HasIndex(z => z.Code).IsUnique();
            builder.Property(z => z.Code).HasMaxLength(50).IsRequired();
            builder.Property(z => z.Name).HasMaxLength(200).IsRequired();
            builder.Property(z => z.Description).HasMaxLength(500);

            builder.HasOne(z => z.Site)
                .WithMany(s => s.Zones)
                .HasForeignKey(z => z.SiteId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}