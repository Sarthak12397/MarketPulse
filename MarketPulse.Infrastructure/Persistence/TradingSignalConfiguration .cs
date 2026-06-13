using MarketPulse.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MarketPulse.Infrastructure.Persistence.Configurations;

public class TradingSignalConfiguration : IEntityTypeConfiguration<TradingSignal>
{
    public void Configure(EntityTypeBuilder<TradingSignal> builder)
    {
        builder.ToTable("trading_signals");
        builder.HasKey(s => s.TradingSignalId);

        builder.Property(s => s.Symbol).HasMaxLength(20).IsRequired();
        builder.Property(s => s.Direction).HasConversion<int>().IsRequired();
        builder.Property(s => s.ConfidenceLevel).HasConversion<int>().IsRequired();
        builder.Property(s => s.Status).HasConversion<int>().IsRequired();
        builder.Property(s => s.ConfidenceScore).HasPrecision(5, 4).IsRequired();
        builder.Property(s => s.PriceAtGeneration).HasPrecision(18, 8).IsRequired();
        builder.Property(s => s.EntryZoneLow).HasPrecision(18, 8);
        builder.Property(s => s.EntryZoneHigh).HasPrecision(18, 8);
        builder.Property(s => s.StopLoss).HasPrecision(18, 8);
        builder.Property(s => s.TakeProfitOne).HasPrecision(18, 8);
        builder.Property(s => s.TakeProfitTwo).HasPrecision(18, 8);
        builder.Property(s => s.Reasoning).HasColumnType("text").IsRequired();

        builder.HasIndex(s => new { s.AssetId, s.Status });
        builder.HasIndex(s => new { s.Status, s.ExpiresAt });

        builder.HasOne<Asset>()
            .WithMany()
            .HasForeignKey(s => s.AssetId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<SignalJob>()
            .WithMany()
            .HasForeignKey(s => s.SignalJobId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}