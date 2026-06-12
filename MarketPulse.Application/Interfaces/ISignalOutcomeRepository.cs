using MarketPulse.Domain.Entities;
namespace MarketPulse.Application.Interfaces;

public interface ISignalOutcomeRepository
{
    Task<SignalOutcome?> GetBySignalIdAsync(Guid tradingSignalId, CancellationToken ct);
    Task<IReadOnlyList<SignalOutcome>> GetAllPendingAsync(CancellationToken ct);
    Task AddAsync(SignalOutcome outcome, CancellationToken ct);
    Task UpdateAsync(SignalOutcome outcome, CancellationToken ct);
}