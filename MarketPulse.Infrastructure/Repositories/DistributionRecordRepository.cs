using MarketPulse.Application.Interfaces;
using MarketPulse.Domain.Entities;
using MarketPulse.Domain.Enums;
using MarketPulse.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace MarketPulse.Infrastructure.Persistence.Repositories;

public class DistributionRecordRepository : IDistributionRecordRepository
{
    private readonly AppDbContext _db;
    public DistributionRecordRepository(AppDbContext db) => _db = db;

    public async Task<DistributionRecord?> GetByIdAsync(Guid id, CancellationToken ct)
        => await _db.DistributionRecords.FindAsync(new object[] { id }, ct);

    public async Task<IReadOnlyList<DistributionRecord>> GetAllPendingAsync(CancellationToken ct)
        => await _db.DistributionRecords
            .Where(r => r.Status == DistributionStatus.Pending)
            .ToListAsync(ct);

    public async Task<IReadOnlyList<DistributionRecord>> GetAllFailedAsync(CancellationToken ct)
        => await _db.DistributionRecords
            .Where(r => r.Status == DistributionStatus.Failed &&
                        r.AttemptCount < r.MaxAttempts)
            .ToListAsync(ct);

    public async Task<IReadOnlyList<DistributionRecord>> GetBySignalIdAsync(
        Guid tradingSignalId, CancellationToken ct)
        => await _db.DistributionRecords
            .Where(r => r.TradingSignalId == tradingSignalId)
            .ToListAsync(ct);

    public async Task AddRangeAsync(
        IEnumerable<DistributionRecord> records, CancellationToken ct)
        => await _db.DistributionRecords.AddRangeAsync(records, ct);

    public Task UpdateAsync(DistributionRecord record, CancellationToken ct)
    {
        _db.DistributionRecords.Update(record);
        return Task.CompletedTask;
    }
}