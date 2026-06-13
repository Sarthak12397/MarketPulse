using MarketPulse.Application.Interfaces;
using MarketPulse.Domain.Entities;
using MarketPulse.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace MarketPulse.Infrastructure.Persistence.Repositories;

public class CandleRepository : ICandleRepository
{
    private readonly AppDbContext _db;
    public CandleRepository(AppDbContext db) => _db = db;

    public async Task<IReadOnlyList<CandleRecord>> GetLatestAsync(
        Guid assetId, TimeFrame timeFrame, int count, CancellationToken ct)
    {
        return await _db.CandleRecords
            .Where(c => c.AssetId == assetId && c.TimeFrame == timeFrame)
            .OrderByDescending(c => c.OpenTimeUtc)
            .Take(count)
            .OrderBy(c => c.OpenTimeUtc)   // return ascending
            .ToListAsync(ct);
    }

    public async Task AddRangeAsync(IEnumerable<CandleRecord> candles, CancellationToken ct)
    {
        // ON CONFLICT DO NOTHING — DB unique index is the only idempotency guard
        foreach (var candle in candles)
        {
            await _db.Database.ExecuteSqlInterpolatedAsync($@"
                INSERT INTO candle_records
                (candle_record_id, asset_id, symbol, time_frame,
                 open_time_utc, close_time_utc, open, high, low, close, volume, ingested_at)
                VALUES
                ({candle.CandleRecordId}, {candle.AssetId}, {candle.Symbol}, {(int)candle.TimeFrame},
                 {candle.OpenTimeUtc}, {candle.CloseTimeUtc},
                 {candle.Open}, {candle.High}, {candle.Low}, {candle.Close},
                 {candle.Volume}, {candle.IngestedAt})
                ON CONFLICT (asset_id, time_frame, open_time_utc) DO NOTHING", ct);
        }
    }
}