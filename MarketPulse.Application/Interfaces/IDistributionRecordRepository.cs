using MarketPulse.Domain.Entities;
namespace MarketPulse.Application.Interfaces;

public interface IDistributionRecordRepository
{
    Task<DistributionRecord?> GetByIdAsync(Guid id, CancellationToken ct);
    Task<IReadOnlyList<DistributionRecord>> GetAllPendingAsync(CancellationToken ct);
    Task<IReadOnlyList<DistributionRecord>> GetAllFailedAsync(CancellationToken ct);
    Task<IReadOnlyList<DistributionRecord>> GetBySignalIdAsync(Guid tradingSignalId, CancellationToken ct);
    Task AddRangeAsync(IEnumerable<DistributionRecord> records, CancellationToken ct);
    Task UpdateAsync(DistributionRecord record, CancellationToken ct);
}