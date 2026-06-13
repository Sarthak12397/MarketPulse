using MarketPulse.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MarketPulse.Infrastructure.Persistence.Configurations;

public class DistributionRecordConfiguration : IEntityTypeConfiguration<DistributionRecord>
{
    public void Configure(EntityTypeBuilder<DistributionRecord> builder)
    {
        builder.ToTable("distribution_records");
        builder.HasKey(r => r.DistributionRecordId);

        builder.Property(r => r.Channel).HasConversion<int>().IsRequired();
        builder.Property(r => r.Status).HasConversion<int>().IsRequired();
        builder.Property(r => r.WebhookEndpoint).HasMaxLength(500).IsRequired();
        builder.Property(r => r.Payload).HasColumnType("text").IsRequired();
        builder.Property(r => r.FailureReason).HasColumnType("text");

        // Partial index for Pending + Failed queries
        builder.HasIndex(r => r.Status)
            .HasFilter("\"status\" IN (0, 2)");
        builder.HasIndex(r => r.TradingSignalId);

        // No cascade — preserve full audit trail
        builder.HasOne<TradingSignal>()
            .WithMany()
            .HasForeignKey(r => r.TradingSignalId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}