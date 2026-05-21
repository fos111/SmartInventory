using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartInventory.Domain.Mobile.Entities;

namespace SmartInventory.Infrastructure.Mobile.Configurations;

public class SyncQueueEntryConfiguration : IEntityTypeConfiguration<SyncQueueEntry>
{
    public void Configure(EntityTypeBuilder<SyncQueueEntry> builder)
    {
        builder.ToTable("SyncQueueEntries");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.DeviceId).HasMaxLength(100).IsRequired();
        builder.Property(e => e.OperationType).HasMaxLength(50).IsRequired();
        builder.Property(e => e.Payload).HasColumnType("jsonb").IsRequired();
        builder.Property(e => e.TargetRoomCode).HasMaxLength(20);
        builder.Property(e => e.NewStatus).HasMaxLength(50);
        builder.Property(e => e.ClientOperationId).HasMaxLength(100).IsRequired();

        builder.HasIndex(e => e.ClientOperationId).IsUnique();
        builder.HasIndex(e => new { e.DeviceId, e.IsProcessed });
        builder.HasIndex(e => e.ReceivedAt);
    }
}
