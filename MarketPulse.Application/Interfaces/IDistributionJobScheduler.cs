public interface IDistributionJobScheduler
{
    
    Task ScheduleDispatchJobAsync(CancellationToken ct);
}