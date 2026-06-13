using MarketPulse.Application.Interfaces;
using MarketPulse.Domain.Entities;
using MarketPulse.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace MarketPulse.Infrastructure.Persistence.Repositories;

public class SignalOutcomeRepository : ISignalOutcomeRepository
{
    private readonly AppDbContext _db;
    public SignalOutcomeRepository(AppDbContext db) => _db = db;

    public async Task<SignalOutcome?> GetBySignalIdAsync(
        Guid tradingSignalId, CancellationToken ct)
        => await _db.SignalOutcomes
            .FirstOrDefaultAsync(o => o.TradingSignalId == tradingSignalId, ct);

    public async Task<IReadOnlyList<SignalOutcome>> GetAllPendingAsync(CancellationToken ct)
        => await _db.SignalOutcomes
            .Where(o => o.OutcomeResult == null)
            .ToListAsync(ct);

    public async Task AddAsync(SignalOutcome outcome, CancellationToken ct)
        => await _db.SignalOutcomes.AddAsync(outcome, ct);

    public Task UpdateAsync(SignalOutcome outcome, CancellationToken ct)
    {
        _db.SignalOutcomes.Update(outcome);
        return Task.CompletedTask;
    }
}