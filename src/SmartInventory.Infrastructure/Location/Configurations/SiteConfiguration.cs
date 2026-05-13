using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartInventory.Domain.Location.Entities;

namespace SmartInventory.Infrastructure.Location.Configurations
{
    public class SiteConfiguration : IEntityTypeConfiguration<Site>
    {
        public void Configure(EntityTypeBuilder<Site> builder)
        {
            builder.ToTable("Sites");
            builder.HasKey(s => s.Id);
            builder.HasIndex(s => s.Code).IsUnique();
            builder.Property(s => s.Code).HasMaxLength(50).IsRequired();
            builder.Property(s => s.Name).HasMaxLength(200).IsRequired();
            builder.Property(s => s.Description).HasMaxLength(500);
        }
    }
}