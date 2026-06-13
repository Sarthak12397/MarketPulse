using MarketPulse.Domain.Enums;

namespace MarketPulse.Domain.Exceptions;

public sealed class InvalidTradingSignalStateException : Exception
{
    public InvalidTradingSignalStateException(
        Guid tradingSignalId,
        SignalStatus status,
        string action)
        : base(
            $"Cannot {action} TradingSignal {tradingSignalId}: Status is {status}")
    {
    }
}