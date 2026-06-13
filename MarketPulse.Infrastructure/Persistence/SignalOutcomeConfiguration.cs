using MarketPulse.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MarketPulse.Infrastructure.Persistence.Configurations;

public class SignalOutcomeConfiguration : IEntityTypeConfiguration<SignalOutcome>
{
    public void Configure(EntityTypeBuilder<SignalOutcome> builder)
    {
        builder.ToTable("signal_outcomes");
        builder.HasKey(o => o.SignalOutcomeId);

        builder.Property(o => o.OutcomeResult).HasConversion<int?>();
        builder.Property(o => o.PriceAtExpiry).HasPrecision(18, 8);
        builder.Property(o => o.PriceChangePercent).HasPrecision(10, 4);
        builder.Property(o => o.EvaluationNotes).HasColumnType("text");

        // One outcome per signal — DB enforced
        builder.HasIndex(o => o.TradingSignalId).IsUnique();

        builder.HasOne<TradingSignal>()
            .WithMany()
            .HasForeignKey(o => o.TradingSignalId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}