using MarketPulse.Domain.Enums;


namespace MarketPulse.Domain.Exceptions;

public sealed class InvalidDistributionStateException : Exception
{
    public InvalidDistributionStateException(
        Guid distributionRecordId,
        DistributionStatus status,
        string action)
        : base(
            $"Cannot {action} DistributionRecord {distributionRecordId}: Status is {status}")
    {
    }
}