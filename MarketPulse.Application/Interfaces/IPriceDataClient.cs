
using MarketPulse.Domain.Entities;
namespace MarketPulse.Application.Interfaces;
public interface IPriceDataClient
{
    Task<decimal>GetCurrentPriceAsync(string symbol, CancellationToken ct);

}