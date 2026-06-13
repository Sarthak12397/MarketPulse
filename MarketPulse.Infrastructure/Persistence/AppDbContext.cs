using MarketPulse.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace MarketPulse.Infrastructure.Persistence;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Asset>              Assets              => Set<Asset>();
    public DbSet<CandleRecord>       CandleRecords       => Set<CandleRecord>();
    public DbSet<SignalJob>          SignalJobs           => Set<SignalJob>();
    public DbSet<TradingSignal>      TradingSignals       => Set<TradingSignal>();
    public DbSet<SignalOutcome>      SignalOutcomes       => Set<SignalOutcome>();
    public DbSet<DistributionRecord> DistributionRecords => Set<DistributionRecord>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}