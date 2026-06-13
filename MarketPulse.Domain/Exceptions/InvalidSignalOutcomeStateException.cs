
public sealed class InvalidSignalOutcomeStateException : Exception
{
    public InvalidSignalOutcomeStateException(Guid signalOutcomeId)
        : base(
            $"SignalOutcome {signalOutcomeId} has already been evaluated")
    {
    }
}