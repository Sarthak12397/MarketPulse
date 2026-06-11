using TradingSignals.Domain.Enums;

namespace TradingSignals.Domain.Exceptions;

public sealed class MarketDataUnavailableException : DomainException
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