using MarketPulse.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MarketPulse.Infrastructure.Persistence.Configurations;

public class AssetConfiguration : IEntityTypeConfiguration<Asset>
{
    public void Configure(EntityTypeBuilder<Asset> builder)
    {
        builder.ToTable("assets");
        builder.HasKey(a => a.AssetId);

        builder.Property(a => a.Symbol).HasMaxLength(20).IsRequired();
        builder.Property(a => a.BaseAsset).HasMaxLength(10).IsRequired();
        builder.Property(a => a.QuoteAsset).HasMaxLength(10).IsRequired();
        builder.Property(a => a.Provider).HasMaxLength(50).IsRequired();
        builder.Property(a => a.TimeFrame).HasConversion<int>().IsRequired();
        builder.Property(a => a.CandleContextWin).IsRequired();
        builder.Property(a => a.IsActive).IsRequired();

        builder.HasIndex(a => a.Symbol).IsUnique();
        builder.HasIndex(a => a.IsActive).HasFilter("\"is_active\" = true");
    }
}