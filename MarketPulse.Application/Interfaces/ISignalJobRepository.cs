using MarketPulse.Domain.Entities;
namespace MarketPulse.Application.Interfaces;

public interface ISignalJobRepository
{
    Task<SignalJob?> GetByIdAsync(Guid signalJobId, CancellationToken ct);
    Task AddAsync(SignalJob job, CancellationToken ct);
    Task UpdateAsync(SignalJob job, CancellationToken ct);
}