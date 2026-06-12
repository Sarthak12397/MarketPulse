using MarketPulse.Domain.Exceptions;

namespace MarketPulse.Domain.Entities;

public class SignalJob
{
    public Guid SignalJobId { get; init; }
    public Guid AssetId { get; init; }
    public string Symbol { get; init; } = null!;
    public TimeFrame TimeFrame { get; init; }
    public int CandleCount { get; init; }
    public DateTime CreatedAt { get; init; }
    public SignalJobStatus Status { get; private set; }
    public DateTime? StartedAt { get; private set; }
    public DateTime? CompletedAt { get; private set; }
    public string? FailureReason { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    protected SignalJob() { }

    public SignalJob(
        Guid assetId,
        string symbol,
        TimeFrame timeFrame,
        int candleCount)
    {
        if (candleCount <= 0)
            throw new ArgumentException("CandleCount must be greater than zero", nameof(candleCount));

        if (string.IsNullOrWhiteSpace(symbol))
            throw new ArgumentException("Symbol cannot be null or empty", nameof(symbol));

        SignalJobId  = Guid.NewGuid();
        AssetId      = assetId;
        Symbol       = symbol;
        TimeFrame    = timeFrame;
        CandleCount  = candleCount;
        Status       = SignalJobStatus.Queued;
        CreatedAt    = DateTime.UtcNow;
        UpdatedAt    = DateTime.UtcNow;
    }

    public void Begin()
    {
        if (Status != SignalJobStatus.Queued)
            throw new InvalidSignalJobStateException(SignalJobId, Status, "Job must be Queued to begin");

        Status    = SignalJobStatus.Running;
        StartedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Complete()
    {
        if (Status != SignalJobStatus.Running)
            throw new InvalidSignalJobStateException(SignalJobId, Status, "Job must be Running to complete");

        Status      = SignalJobStatus.Completed;
        CompletedAt = DateTime.UtcNow;
        UpdatedAt   = DateTime.UtcNow;
    }

    public void Fail(string reason)
    {
        if (Status != SignalJobStatus.Running)
            throw new InvalidSignalJobStateException(
                SignalJobId, Status, $"Cannot fail a job with status {Status}. Expected Running.");

        if (string.IsNullOrWhiteSpace(reason))
            throw new ArgumentException("Failure reason cannot be null or empty", nameof(reason));

        Status        = SignalJobStatus.Failed;
        FailureReason = reason;
        CompletedAt   = DateTime.UtcNow;
        UpdatedAt     = DateTime.UtcNow;
    }
}