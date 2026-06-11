public sealed class InsufficientCandleDataException : Exception
{
    public InsufficientCandleDataException(
        string symbol,
        int available,
        int required)
        : base(
            $"Asset {symbol} has {available} candles. Required: {required}. Cannot generate signal.")
    {
    }
}