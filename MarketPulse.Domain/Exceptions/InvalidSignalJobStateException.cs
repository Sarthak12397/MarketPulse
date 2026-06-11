public sealed class InvalidSignalJobStateException : InvalidOperationException
{
    public Guid SignalJobId { get; }
    public SignalJobStatus ExpectedStatus { get; }
    public SignalJobStatus ActualStatus { get; }
    public string Operation { get; }

    public InvalidSignalJobStateException(
        Guid signalJobId,
        string operation,
        SignalJobStatus expectedStatus,
        SignalJobStatus actualStatus)
        : base(
            $"Cannot {operation} SignalJob {signalJobId}: expected Status {expectedStatus}, but current Status is {actualStatus}")
    {
        SignalJobId = signalJobId;
        Operation = operation;
        ExpectedStatus = expectedStatus;
        ActualStatus = actualStatus;
    }
}