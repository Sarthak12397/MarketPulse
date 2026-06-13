using Hangfire;
using MarketPulse.Application.Services;

namespace MarketPulse.Application.Jobs;

public class RetryFailedDistributionsJob
{
    private readonly SignalDistributionService _distribution;

    public RetryFailedDistributionsJob(SignalDistributionService distribution)
        => _distribution = distribution;

    [Queue("distribution-retry")]
    public async Task ExecuteAsync(CancellationToken ct)
        => await _distribution.RetryFailedAsync(ct);
}