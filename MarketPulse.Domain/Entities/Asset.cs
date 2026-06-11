namespace MarketPulse.Domain.Entities;

public class Asset
{
    public Guid AssetId { get; init; }
    public string Symbol { get; init; } = null!;
    public string BaseAsset { get; init; } = null!;
    public string QuoteAsset { get; init; } = null!;
    public string Provider { get; init; } = null!;
    public int CandleContextWin { get; init; }
    public TimeFrame TimeFrame { get; init; }
    public DateTime CreatedAtUTC { get; init; }
    public bool IsActive { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    public Asset(
        string symbol,
        string baseAsset,
        string quoteAsset,
        string provider,
        TimeFrame timeFrame,
        int candleContextWin)
    {
        if (string.IsNullOrWhiteSpace(symbol))
            throw new ArgumentException("Symbol cannot be null or empty");

        if (string.IsNullOrWhiteSpace(provider))
            throw new ArgumentException("Provider cannot be null or empty");

        if (candleContextWin < 50)
            throw new ArgumentException("CandleContextWin must be at least 50");

        if (candleContextWin > 1000)
            throw new ArgumentException("CandleContextWin cannot exceed 1000");

        AssetId          = Guid.NewGuid();
        Symbol           = symbol;
        BaseAsset        = baseAsset;
        QuoteAsset       = quoteAsset;
        Provider         = provider;
        TimeFrame        = timeFrame;
        CandleContextWin = candleContextWin;
        IsActive         = false;
        CreatedAtUTC     = DateTime.UtcNow;
        UpdatedAt        = DateTime.UtcNow;
    }

    protected Asset() { }

    public void Activate()
    {
        if (IsActive == true)
            throw new InvalidAssetStateException(AssetId, IsActive, "Asset is already active");

        IsActive  = true;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Deactivate()
    {
        if (IsActive == false)
            throw new InvalidAssetStateException(AssetId, IsActive, "Asset is already inactive");

        IsActive  = false;
        UpdatedAt = DateTime.UtcNow;
    }
}