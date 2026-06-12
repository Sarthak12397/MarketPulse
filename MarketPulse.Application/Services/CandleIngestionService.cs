using MarketPulse.Application.DTOs;
using MarketPulse.Application.Interfaces;
using MarketPulse.Domain.Entities;
using MarketPulse.Domain.Enums;
using MarketPulse.Domain.Exceptions;

namespace MarketPulse.Application.Services;

public class CandleIngestionService
{
    private readonly ICandleRepository _candles;
    private readonly IMarketDataClient _market;
    private readonly IAssetRepository _assets;

    public CandleIngestionService(
        ICandleRepository candles,
        IMarketDataClient market,
        IAssetRepository assets)
    {
        _candles = candles;
        _market  = market;
        _assets  = assets;
    }

    public async Task<int> IngestForAssetAsync(Guid assetId, CancellationToken ct)
    {
        // Step 1 — get asset, verify it exists and is active
        var asset = await _assets.GetByIdAsync(assetId, ct);
        if (asset == null || !asset.IsActive)
            throw new ArgumentException($"Asset {assetId} not found or inactive");

        // Step 2 — fetch candles from market provider
        // +50 buffer so we always have enough history
        var rawCandles = await _market.FetchCandlesAsync(
            asset.Symbol,
            asset.TimeFrame,
            asset.CandleContextWin + 50,
            ct);

        if (rawCandles == null || rawCandles.Count == 0)
            throw new MarketDataUnavailableException(
                asset.Symbol, asset.TimeFrame.ToString(), "Provider returned empty result");

        // Step 3 — map RawCandleDto -> CandleRecord
        // No ExistsAsync loop. Constructor validates each candle.
        // ON CONFLICT DO NOTHING at DB level handles duplicates.
        var records = rawCandles.Select(r => new CandleRecord(
            assetId:      asset.AssetId,
            symbol:       asset.Symbol,
            timeFrame:    asset.TimeFrame,
            openTimeUtc:  r.OpenTimeUtc,
            closeTimeUtc: r.CloseTimeUtc,
            open:         r.Open,
            high:         r.High,
            low:          r.Low,
            close:        r.Close,
            volume:       r.Volume
        )).ToList();

        // Step 4 — bulk insert, DB handles duplicates silently
        await _candles.AddRangeAsync(records, ct);

        // Step 5 — return count for logging
        return records.Count;
    }
}