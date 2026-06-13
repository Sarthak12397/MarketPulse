using MarketPulse.Domain.Enums;

namespace MarketPulse.Domain.Exceptions;

public sealed class MarketDataUnavailableException : Exception
{
    public MarketDataUnavailableException(
        string symbol,
        TimeFrame timeFrame,
        string reason)
        : base(
            $"Market data unavailable for {symbol}/{timeFrame}: {reason}")
    {
    }
}