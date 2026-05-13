using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartInventory.Domain.Location.Entities;

namespace SmartInventory.Infrastructure.Location.Configurations
{
    public class RoomGeometryConfiguration : IEntityTypeConfiguration<RoomGeometry>
    {
        public void Configure(EntityTypeBuilder<RoomGeometry> builder)
        {
            builder.ToTable("RoomGeometries");
            builder.HasKey(rg => rg.Id);
            builder.HasIndex(rg => rg.RoomId).IsUnique();
            builder.Property(rg => rg.ShapeType).HasMaxLength(20).IsRequired();
            builder.Property(rg => rg.X).IsRequired();
            builder.Property(rg => rg.Y).IsRequired();
            builder.Property(rg => rg.Width).IsRequired();
            builder.Property(rg => rg.Height).IsRequired();
            builder.Property(rg => rg.Color).HasMaxLength(9).IsRequired();
            builder.Property(rg => rg.Stroke).HasMaxLength(9).IsRequired();
            builder.Property(rg => rg.Properties);

            builder.HasOne(rg => rg.Room)
                .WithOne(r => r.RoomGeometry)
                .HasForeignKey<RoomGeometry>(rg => rg.RoomId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
