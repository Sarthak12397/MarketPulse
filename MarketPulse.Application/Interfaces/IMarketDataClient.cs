using MarketPulse.Application.DTOs;
using MarketPulse.Domain.Entities;
using MarketPulse.Domain.Enums;

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