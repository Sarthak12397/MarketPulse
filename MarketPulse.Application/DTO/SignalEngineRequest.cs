using System.Collections.Generic;
using MarketPulse.Domain.Enums;

namespace MarketPulse.Application.DTOs
{
 
    public record SignalEngineRequest(
        string Symbol,
        TimeFrame TimeFrame,
        IReadOnlyList<RawCandleDto> Candles
    );
}
