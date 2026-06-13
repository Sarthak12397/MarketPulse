  
namespace MarketPulse.Application.DTOs;

  
    public record RawCandleDto(
        DateTime OpenTimeUtc,
        DateTime CloseTimeUtc,
        decimal Open,
        decimal High,
        decimal Low,
        decimal Close,
        decimal Volume
    );
