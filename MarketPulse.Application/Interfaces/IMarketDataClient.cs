using MarketPulse.Domain.Entities;
namespace MarketPulse.Application.Interfaces;

public interface IMarketDataClient
{
    Task<IReadOnlyList<RawCandleDto>>FetchCandlesAsync(
        string symbol,
        TimeFrame timeFrame,
        int count,
        CancellationToken ct
    );
}