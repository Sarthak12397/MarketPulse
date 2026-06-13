using MarketPulse.Application.Interfaces;
using MarketPulse.Domain.Entities;
using MarketPulse.Domain.Enums;
using MarketPulse.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace MarketPulse.Infrastructure.Persistence.Repositories;

public class TradingSignalRepository : ITradingSignalRepository
{
    private readonly AppDbContext _db;
    public TradingSignalRepository(AppDbContext db) => _db = db;

    public async Task<TradingSignal?> GetByIdAsync(Guid signalId, CancellationToken ct)
        => await _db.TradingSignals.FindAsync(new object[] { signalId }, ct);

    public async Task<IReadOnlyList<TradingSignal>> GetActiveForAssetAsync(
        Guid assetId, CancellationToken ct)
        => await _db.TradingSignals
            .Where(s => s.AssetId == assetId && s.Status == SignalStatus.Active)
            .ToListAsync(ct);

    public async Task<IReadOnlyList<TradingSignal>> GetExpiredUnevaluatedAsync(
        CancellationToken ct)
    {
        var now = DateTime.UtcNow;
        return await _db.TradingSignals
            .Where(s => s.Status == SignalStatus.Active && s.ExpiresAt <= now)
            .ToListAsync(ct);
    }

    public async Task AddAsync(TradingSignal signal, CancellationToken ct)
        => await _db.TradingSignals.AddAsync(signal, ct);

    public Task UpdateAsync(TradingSignal signal, CancellationToken ct)
    {
        _db.TradingSignals.Update(signal);
        return Task.CompletedTask;
    }
}