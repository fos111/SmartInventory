using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartInventory.Domain.Location.Entities;

namespace SmartInventory.Infrastructure.Location.Configurations
{
    public class ZoneSiteShapeConfiguration : IEntityTypeConfiguration<ZoneSiteShape>
    {
        public void Configure(EntityTypeBuilder<ZoneSiteShape> builder)
        {
            builder.ToTable("ZoneSiteShapes");
            builder.HasKey(z => z.Id);
            builder.HasIndex(z => z.ZoneId);
            builder.Property(z => z.Points).HasMaxLength(4000).IsRequired();
            builder.Property(z => z.Color).HasMaxLength(9).IsRequired();
            builder.Property(z => z.Label).HasMaxLength(200);

            builder.HasOne(z => z.Zone)
                .WithMany()
                .HasForeignKey(z => z.ZoneId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
