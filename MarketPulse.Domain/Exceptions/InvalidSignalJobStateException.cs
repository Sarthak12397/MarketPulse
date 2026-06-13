using MarketPulse.Domain.Enums;

namespace MarketPulse.Domain.Exceptions;

public sealed class InvalidSignalJobStateException : Exception
{
    public InvalidSignalJobStateException(
        Guid signalJobId,
        SignalJobStatus status,
        string action)
        : base(
            $"Cannot {action} SignalJob {signalJobId}: current Status is {status}")
    {
    }
}