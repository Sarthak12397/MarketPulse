using MarketPulse.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MarketPulse.Infrastructure.Persistence.Configurations;

public class CandleRecordConfiguration : IEntityTypeConfiguration<CandleRecord>
{
    public void Configure(EntityTypeBuilder<CandleRecord> builder)
    {
        builder.ToTable("candle_records");
        builder.HasKey(c => c.CandleRecordId);

        builder.Property(c => c.Symbol).HasMaxLength(20).IsRequired();
        builder.Property(c => c.TimeFrame).HasConversion<int>().IsRequired();
        builder.Property(c => c.Open).HasPrecision(18, 8).IsRequired();
        builder.Property(c => c.High).HasPrecision(18, 8).IsRequired();
        builder.Property(c => c.Low).HasPrecision(18, 8).IsRequired();
        builder.Property(c => c.Close).HasPrecision(18, 8).IsRequired();
        builder.Property(c => c.Volume).HasPrecision(18, 8).IsRequired();

        // THE idempotency guard — one candle per asset/frame/time
        builder.HasIndex(c => new { c.AssetId, c.TimeFrame, c.OpenTimeUtc }).IsUnique();
        builder.HasIndex(c => new { c.Symbol, c.TimeFrame, c.OpenTimeUtc });

        // No cascade delete — preserve history
        builder.HasOne<Asset>()
            .WithMany()
            .HasForeignKey(c => c.AssetId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}