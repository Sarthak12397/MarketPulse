using System.Net.Http.Json;
using MarketPulse.Application.Interfaces;

namespace MarketPulse.Infrastructure.ExternalClients;

public class BinancePriceDataClient : IPriceDataClient
{
    private readonly HttpClient _http;

    public BinancePriceDataClient(HttpClient http)
        => _http = http;

    public async Task<decimal> GetCurrentPriceAsync(string symbol, CancellationToken ct)
    {
        var clean    = symbol.Replace("/", "");
        var response = await _http.GetFromJsonAsync<BinanceTickerResponse>(
            $"/api/v3/ticker/price?symbol={clean}", ct);

        return response?.Price ?? throw new Exception($"Could not get price for {symbol}");
    }

    private record BinanceTickerResponse(string Symbol, decimal Price);
}