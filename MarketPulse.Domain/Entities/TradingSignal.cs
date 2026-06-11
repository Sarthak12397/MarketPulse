using System;

public class TradingSignal
{

    public Guid TradingSignalId { get; init; }

    public Guid AssetId { get; init; }
    public Guid SignalJobId { get; init; }

    public string Symbol { get; init; }
    public SignalDirection Direction { get; init; }
    public decimal ConfidenceScore { get; init; }
    public ConfidenceLevel ConfidenceLevel { get; init; }
    public decimal PriceAtGeneration { get; init; }
    public string Reasoning { get; init; }
    public DateTime ValidFrom { get; init; }
    public DateTime ExpiresAt { get; init; }
    public DateTime GeneratedAt { get; init; }

    public decimal? EntryZoneLow { get; init; }
    public decimal? EntryZoneHigh { get; init; }
    public decimal? StopLoss { get; init; }
    public decimal? TakeProfitOne { get; init; }
    public decimal? TakeProfitTwo { get; init; }

    public SignalStatus Status { get; private set; }
    public DateTime UpdatedAt { get; private set; }


        protected TradingSignal() { }


        public TradingSignal(
    Guid assetId,
    Guid signalJobId,
    string symbol,
    SignalDirection direction,
    decimal confidenceScore,
    decimal priceAtGeneration,
    string reasoning,
    DateTime validFrom,
    DateTime expiresAt,
    decimal? entryZoneLow,
    decimal? entryZoneHigh,
    decimal? stopLoss,
    decimal? takeProfitOne,
    decimal? takeProfitTwo)
{
    if (confidenceScore < 0.0m || confidenceScore > 1.0m)
        throw new ArgumentException("Confidence score must be between 0.0 and 1.0.", nameof(confidenceScore));

    if (priceAtGeneration <= 0)
        throw new ArgumentException("Price at generation must be greater than zero.", nameof(priceAtGeneration));

    if (string.IsNullOrWhiteSpace(reasoning))
        throw new ArgumentException("Reasoning cannot be null or empty.", nameof(reasoning));

    if (expiresAt <= validFrom)
        throw new ArgumentException("ExpiresAt must be greater than ValidFrom.");

    if (entryZoneLow.HasValue && !entryZoneHigh.HasValue)
        throw new ArgumentException("EntryZoneHigh must be provided when EntryZoneLow is set.");

    if (entryZoneLow.HasValue &&
        entryZoneHigh.HasValue &&
        entryZoneHigh.Value <= entryZoneLow.Value)
        throw new ArgumentException("EntryZoneHigh must be greater than EntryZoneLow.");

    TradingSignalId = Guid.NewGuid();
    AssetId = assetId;
    SignalJobId = signalJobId;
    Symbol = symbol;
    Direction = direction;
    ConfidenceScore = confidenceScore;
    ConfidenceLevel = DeriveLevel(confidenceScore);
    PriceAtGeneration = priceAtGeneration;
    Reasoning = reasoning;
    ValidFrom = validFrom;
    ExpiresAt = expiresAt;
    EntryZoneLow = entryZoneLow;
    EntryZoneHigh = entryZoneHigh;
    StopLoss = stopLoss;
    TakeProfitOne = takeProfitOne;
    TakeProfitTwo = takeProfitTwo;

    Status = SignalStatus.Active;
    GeneratedAt = DateTime.UtcNow;
    UpdatedAt = DateTime.UtcNow;
}
private static ConfidenceLevel DeriveLevel(decimal score)
{
    if (score <= 0.45m)
        return ConfidenceLevel.Low;

    if (score <= 0.70m)
        return ConfidenceLevel.Medium;

    return ConfidenceLevel.High;
}
}