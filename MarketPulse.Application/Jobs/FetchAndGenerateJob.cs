using Hangfire;
using MarketPulse.Application.Interfaces;
using MarketPulse.Application.Services;
using Microsoft.Extensions.Logging;
using MarketPulse.Domain.Enums;

namespace MarketPulse.Application.Jobs;

public class FetchAndGenerateJob
{
    private readonly IAssetRepository       _assets;
    private readonly CandleIngestionService _ingestion;
    private readonly SignalOrchestrationService _orchestration;
    private readonly ILogger<FetchAndGenerateJob> _logger;

    public FetchAndGenerateJob(
        IAssetRepository            assets,
        CandleIngestionService      ingestion,
        SignalOrchestrationService  orchestration,
        ILogger<FetchAndGenerateJob> logger)
    {
        _assets        = assets;
        _ingestion     = ingestion;
        _orchestration = orchestration;
        _logger        = logger;
    }

    [Queue("signal-pipeline")]
    [DisableConcurrentExecution(300)]
    public async Task ExecuteForTimeFrameAsync(TimeFrame timeFrame, CancellationToken ct)
    {
        var allActive = await _assets.GetAllActiveAsync(ct);
        var forFrame  = allActive.Where(a => a.TimeFrame == timeFrame).ToList();

        _logger.LogInformation("FetchAndGenerateJob: {Count} assets for {Frame}",
            forFrame.Count, timeFrame);

        foreach (var asset in forFrame)
        {
            try
            {
                await _ingestion.IngestForAssetAsync(asset.AssetId, ct);
                await _orchestration.OrchestrateForAssetAsync(asset.AssetId, ct);
            }
            catch (Exception ex)
            {
                // One failing asset does NOT stop others
                _logger.LogError(ex, "Asset {AssetId} failed in pipeline", asset.AssetId);
            }
        }
    }
}