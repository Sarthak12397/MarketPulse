using MarketPulse.Application.DTOs;

public interface IAISignalEngineClient
{
    Task<SignalEngineResponse>GenerateSignalAsync(

        SignalEngineRequest request,
        CancellationToken ct
    );
}