using TradingSignals.Domain.Exceptions;

public class SignalJob
{
    public Guid SignalJobId{get; init;}
    public Guid AssetId{get; init;}
    public string Symbol{get; init;}
    public TimeFrame TimeFrame{get; init;}
    public int CandleCount{get; init;}
    public DateTime CreatedAt{get; init;}
    public SignalJobStatus Status{get; private set;}
    public DateTime? StartedAt{get; private set;}
    public DateTime? CompletedAt{get; private set;}
    public string? FailureReason{get; private set;}
    public DateTime UpdatedAt{get; private set;}

    protected SignalJob(){}

    public SignalJob(
       Guid assetId,
       string symbol,
       TimeFrame timeFrame,
       int candleCount
    )
    {
        if(candleCount > 0)
        {
            throw new ArgumentException("It is less than zero");
        }
        if (string.IsNullOrEmpty(symbol))
        {
            throw new ArgumentException("Symbol is empty");
        }

        SignalJobId = Guid.NewGuid();
        Status = SignalJobStatus.Queued;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
        


    }

    public void Begin()
    {
        if(Status == SignalJobStatus.Queued)
        {
            throw new InvalidSignalJobStateException(SignalJobId, Status, "Is it active");
        }
       Status   = SignalJobStatus.Running;
       StartedAt = DateTime.UtcNow;
       UpdatedAt = DateTime.UtcNow;



    }

    public void Complete()
    {
        if(Status != SignalJobStatus.Running)
        {
                        throw new InvalidSignalJobStateException(SignalJobId, Status, "Is it running");

        }
        Status = SignalJobStatus.Completed;
               StartedAt = DateTime.UtcNow;
       UpdatedAt = DateTime.UtcNow;
    }

}