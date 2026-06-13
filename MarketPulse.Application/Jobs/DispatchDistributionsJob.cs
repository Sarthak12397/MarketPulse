using Hangfire;
using MarketPulse.Application.Services;

namespace MarketPulse.Application.Jobs;

public class DispatchDistributionsJob
{
    private readonly SignalDistributionService _distribution;

    public DispatchDistributionsJob(SignalDistributionService distribution)
        => _distribution = distribution;

    [Queue("distribution")]
    public async Task ExecuteAsync(CancellationToken ct)
        => await _distribution.DispatchPendingAsync(ct);
}