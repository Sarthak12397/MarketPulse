using MarketPulse.Domain.Entities;
namespace MarketPulse.Application.Interfaces;

public interface ITradingSignalRepository
{
    Task<TradingSignal?> GetByIdAsync(Guid signalId, CancellationToken ct);
    Task<IReadOnlyList<TradingSignal>> GetActiveForAssetAsync(Guid assetId, CancellationToken ct);
    Task<IReadOnlyList<TradingSignal>> GetExpiredUnevaluatedAsync(CancellationToken ct);
    Task AddAsync(TradingSignal signal, CancellationToken ct);
    Task UpdateAsync(TradingSignal signal, CancellationToken ct);
}