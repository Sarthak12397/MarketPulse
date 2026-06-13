using Hangfire;
using MarketPulse.Application.Interfaces;
using MarketPulse.Application.Jobs;

namespace MarketPulse.Infrastructure.BackgroundJobs;

public class HangfireJobScheduler : IDistributionJobScheduler
{
    private readonly IBackgroundJobClient _client;

    public HangfireJobScheduler(IBackgroundJobClient client)
        => _client = client;

    public Task ScheduleDispatchJobAsync(CancellationToken ct)
    {
        _client.Enqueue<DispatchDistributionsJob>(
            j => j.ExecuteAsync(CancellationToken.None));
        return Task.CompletedTask;
    }
}