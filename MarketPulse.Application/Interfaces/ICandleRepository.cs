public interface ICandleRepository
{
    Task<IReadOnlyList<CandleRecord>>GetLatestAsync(Guid assetId, TimeFrame timeFrame, int count, CancellationToken ct);

    Task AddRangeAsync( IEnumerable<CandleRecord>candles, CancellationToken ct);
    
}