using MarketPulse.Application.Interfaces;
using MarketPulse.Domain.Entities;
using MarketPulse.Infrastructure.Persistence;

namespace MarketPulse.Infrastructure.Persistence.Repositories;

public class SignalJobRepository : ISignalJobRepository
{
    private readonly AppDbContext _db;
    public SignalJobRepository(AppDbContext db) => _db = db;

    public async Task<SignalJob?> GetByIdAsync(Guid signalJobId, CancellationToken ct)
        => await _db.SignalJobs.FindAsync(new object[] { signalJobId }, ct);

    public async Task AddAsync(SignalJob job, CancellationToken ct)
        => await _db.SignalJobs.AddAsync(job, ct);

    public Task UpdateAsync(SignalJob job, CancellationToken ct)
    {
        _db.SignalJobs.Update(job);
        return Task.CompletedTask;
    }
}