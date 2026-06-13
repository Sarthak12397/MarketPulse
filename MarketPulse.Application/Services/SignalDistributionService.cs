using MarketPulse.Application.Interfaces;
using MarketPulse.Domain.Enums;

namespace MarketPulse.Application.Services;

public class SignalDistributionService
{
    private readonly IDistributionRecordRepository _distributions;
    private readonly IWebhookDispatcherClient      _webhook;
    private readonly IUnitOfWork                   _uow;

    public SignalDistributionService(
        IDistributionRecordRepository distributions,
        IWebhookDispatcherClient      webhook,
        IUnitOfWork                   uow)
    {
        _distributions = distributions;
        _webhook       = webhook;
        _uow           = uow;
    }

    public async Task DispatchPendingAsync(CancellationToken ct)
    {
        // Get everything in Pending state
        var records = await _distributions.GetAllPendingAsync(ct);

        if (records.Count == 0)
            return;

        // Fan out ALL channels in parallel
        // One dead Telegram bot cannot block Discord and Email
        var tasks = records.Select(record => DispatchOneAsync(record, ct));
        await Task.WhenAll(tasks);
    }

    private async Task DispatchOneAsync(
        Domain.Entities.DistributionRecord record,
        CancellationToken ct)
    {
        // AttemptDispatch() internally calls MarkPermanentlyFailed()
        // if AttemptCount >= MaxAttempts — entity handles it
        record.AttemptDispatch();

        // If entity just permanently failed it — save and stop
        if (record.Status == DistributionStatus.PermanentlyFailed)
        {
            await _distributions.UpdateAsync(record, ct);
            await _uow.SaveChangesAsync(ct);
            return;
        }

        // Call the webhook — DispatchAsync never throws, returns bool
        var success = await _webhook.DispatchAsync(
            record.WebhookEndpoint,
            record.Payload,
            ct);

        if (success)
            record.MarkDispatched();
        else
            record.MarkFailed("Webhook returned non-2xx response");

        await _distributions.UpdateAsync(record, ct);
        await _uow.SaveChangesAsync(ct);
    }

    public async Task RetryFailedAsync(CancellationToken ct)
    {
        // Get all Failed records that still have attempts left
        var failed = await _distributions.GetAllFailedAsync(ct);

        if (failed.Count == 0)
            return;

        // Reset each one back to Pending
        // DispatchPendingAsync will pick them up on next run
        foreach (var record in failed)
        {
            record.ResetToPending(); // ← add this method to entity
            await _distributions.UpdateAsync(record, ct);
        }

        await _uow.SaveChangesAsync(ct);
    }
    
}