public class CandleRecord
{
    public Guid CandleRecordId{get; init;}
    public Guid AssetId{get; init;}
    public string Symbol{get; init;}
    public TimeFrame TimeFrame{get; init;}
    public DateTime OpenTimeUtc{get; init;}
    public DateTime CloseTimeUtc{get; init;}
    public decimal Open{get; init;}
    public decimal Close{get; init;}
    public decimal High{get; init;}
    public decimal Low{get; init;}
    public decimal Volume{get; init;}
    public DateTime IngestedAt{get; init;}

   protected CandleRecord(){}
 public CandleRecord(
        Guid assetId,
        string symbol,
        TimeFrame timeFrame,
        DateTime openTimeUtc,
        DateTime closeTimeUtc,
        decimal open,
        decimal high,
        decimal low,
        decimal close,
        decimal volume)
    {
        if (open <= 0)
            throw new ArgumentException("Open must be greater than 0.", nameof(open));

        if (high <= 0)
            throw new ArgumentException("High must be greater than 0.", nameof(high));

        if (low <= 0)
            throw new ArgumentException("Low must be greater than 0.", nameof(low));

        if (close <= 0)
            throw new ArgumentException("Close must be greater than 0.", nameof(close));

        if (high < low)
            throw new ArgumentException("High must be greater than or equal to Low.");

        if (high < open || high < close)
            throw new ArgumentException("High must be greater than or equal to Open and Close.", nameof(high));

        if (low > open || low > close)
            throw new ArgumentException("Low must be less than or equal to Open and Close.", nameof(low));

        if (openTimeUtc >= closeTimeUtc)
            throw new ArgumentException("OpenTimeUtc must be earlier than CloseTimeUtc.");

        if (volume < 0)
            throw new ArgumentException("Volume must be greater than or equal to 0.", nameof(volume));

        CandleRecordId = Guid.NewGuid();
        IngestedAt = DateTime.UtcNow;

        AssetId = assetId;
        Symbol = symbol;
        TimeFrame = timeFrame;
        OpenTimeUtc = openTimeUtc;
        CloseTimeUtc = closeTimeUtc;
        Open = open;
        High = high;
        Low = low;
        Close = close;
        Volume = volume;
    }

}