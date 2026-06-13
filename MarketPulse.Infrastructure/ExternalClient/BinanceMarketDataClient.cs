using System.Net.Http.Json;
using MarketPulse.Application.DTOs;
using MarketPulse.Application.Interfaces;
using MarketPulse.Domain.Exceptions;

namespace MarketPulse.Infrastructure.ExternalClients;

public class BinanceMarketDataClient : IMarketDataClient
{
    private readonly HttpClient _http;

    public BinanceMarketDataClient(HttpClient http)
        => _http = http;

    public async Task<IReadOnlyList<RawCandleDto>> FetchCandlesAsync(
        string symbol, TimeFrame timeFrame, int count, CancellationToken ct)
    {
        var interval = MapInterval(timeFrame);
        var clean    = symbol.Replace("/", ""); // BTC/USDT -> BTCUSDT

        var url      = $"/api/v3/klines?symbol={clean}&interval={interval}&limit={count}";
        var response = await _http.GetAsync(url, ct);

        if (!response.IsSuccessStatusCode)
            throw new MarketDataUnavailableException(symbol, timeFrame,
                $"Binance returned {response.StatusCode}");

        // Binance returns array of arrays: [openTime, open, high, low, close, volume, closeTime, ...]
        var raw = await response.Content
            .ReadFromJsonAsync<decimal[][]>(cancellationToken: ct);

        if (raw is null || raw.Length == 0)
            throw new MarketDataUnavailableException(symbol, timeFrame, "Empty candle data");

        return raw.Select(k => new RawCandleDto(
            OpenTimeUtc:  DateTimeOffset.FromUnixTimeMilliseconds((long)k[0]).UtcDateTime,
            CloseTimeUtc: DateTimeOffset.FromUnixTimeMilliseconds((long)k[6]).UtcDateTime,
            Open:   k[1],
            High:   k[2],
            Low:    k[3],
            Close:  k[4],
            Volume: k[5]
        )).ToList();
    }

    private static string MapInterval(TimeFrame tf) => tf switch
    {
        TimeFrame.M1  => "1m",
        TimeFrame.M5  => "5m",
        TimeFrame.M15 => "15m",
        TimeFrame.H1  => "1h",
        TimeFrame.H4  => "4h",
        TimeFrame.D1  => "1d",
        _             => "1h"
    };
}