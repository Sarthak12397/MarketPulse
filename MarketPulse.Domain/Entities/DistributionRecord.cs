using MarketPulse.Domain.Exceptions;
using MarketPulse.Domain.Enums;

namespace MarketPulse.Domain.Entities;

public class DistributionRecord
{
    public Guid DistributionRecordId { get; init; }
    public Guid TradingSignalId { get; init; }
    public DistributionChannel Channel { get; init; }
    public string WebhookEndpoint { get; init; } = null!;
    public string Payload { get; init; } = null!;
    public int MaxAttempts { get; init; }
    public DateTime CreatedAt { get; init; }

    public DistributionStatus Status { get; private set; }
    public int AttemptCount { get; private set; }
    public DateTime? LastAttemptAt { get; private set; }
    public DateTime? DispatchedAt { get; private set; }
    public string? FailureReason { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    protected DistributionRecord() { }

    public DistributionRecord(
        Guid tradingSignalId,
        DistributionChannel channel,
        string webhookEndpoint,
        string payload,
        int maxAttempts)
    {
        if (string.IsNullOrWhiteSpace(webhookEndpoint))
            throw new ArgumentException("WebhookEndpoint cannot be empty", nameof(webhookEndpoint));

        if (string.IsNullOrWhiteSpace(payload))
            throw new ArgumentException("Payload cannot be empty", nameof(payload));

        if (maxAttempts <= 0)
            throw new ArgumentException("MaxAttempts must be at least 1", nameof(maxAttempts));

        DistributionRecordId = Guid.NewGuid();
        TradingSignalId      = tradingSignalId;
        Channel              = channel;
        WebhookEndpoint      = webhookEndpoint;
        Payload              = payload;
        MaxAttempts          = maxAttempts;
        Status               = DistributionStatus.Pending;
        AttemptCount         = 0;
        CreatedAt            = DateTime.UtcNow;
        UpdatedAt            = DateTime.UtcNow;
    }

    public void AttemptDispatch()
    {
        if (Status == DistributionStatus.PermanentlyFailed)
            throw new InvalidDistributionStateException(
                DistributionRecordId, Status, "Cannot dispatch a permanently failed record");

        if (AttemptCount >= MaxAttempts)
        {
            MarkPermanentlyFailed();
            return;
        }

        AttemptCount++;
        LastAttemptAt = DateTime.UtcNow;
        UpdatedAt     = DateTime.UtcNow;
    }

    public void MarkDispatched()
    {
        if (Status == DistributionStatus.PermanentlyFailed ||
            Status == DistributionStatus.Dispatched)
            throw new InvalidDistributionStateException(
                DistributionRecordId, Status, "Cannot dispatch a terminal record");

        Status       = DistributionStatus.Dispatched;
        DispatchedAt = DateTime.UtcNow;
        UpdatedAt    = DateTime.UtcNow;
    }

    public void MarkFailed(string reason)
    {
        if (Status == DistributionStatus.PermanentlyFailed ||
            Status == DistributionStatus.Dispatched)
            throw new InvalidDistributionStateException(
                DistributionRecordId, Status, "Cannot fail a terminal record");

        if (string.IsNullOrWhiteSpace(reason))
            throw new ArgumentException("Failure reason cannot be null or empty", nameof(reason));

        Status        = DistributionStatus.Failed;
        FailureReason = reason;
        UpdatedAt     = DateTime.UtcNow;
    }

    public void MarkPermanentlyFailed()
    {
        Status    = DistributionStatus.PermanentlyFailed;
        UpdatedAt = DateTime.UtcNow;
    }
    public void ResetToPending()
{
    if (Status == DistributionStatus.PermanentlyFailed)
        throw new InvalidDistributionStateException(
            DistributionRecordId, Status, "Cannot reset a permanently failed record");

    Status    = DistributionStatus.Pending;
    UpdatedAt = DateTime.UtcNow;
}
}