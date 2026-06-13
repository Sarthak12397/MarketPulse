using MarketPulse.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MarketPulse.Infrastructure.Persistence.Configurations;

public class SignalJobConfiguration : IEntityTypeConfiguration<SignalJob>
{
    public void Configure(EntityTypeBuilder<SignalJob> builder)
    {
        builder.ToTable("signal_jobs");
        builder.HasKey(j => j.SignalJobId);

        builder.Property(j => j.Symbol).HasMaxLength(20).IsRequired();
        builder.Property(j => j.Status).HasConversion<int>().IsRequired();
        builder.Property(j => j.TimeFrame).HasConversion<int>().IsRequired();
        builder.Property(j => j.FailureReason).HasColumnType("text");

        builder.HasIndex(j => j.AssetId);
        builder.HasIndex(j => new { j.AssetId, j.Status });

        builder.HasOne<Asset>()
            .WithMany()
            .HasForeignKey(j => j.AssetId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}