using MarketPulse.Application.DTOs;
using MarketPulse.Application.Interfaces;
using MarketPulse.Domain.Entities;
using MarketPulse.Domain.Exceptions;
using System.Text.Json;

namespace MarketPulse.Application.Services;

public class SignalOrchestrationService
{
    private readonly IAssetRepository              _assets;
    private readonly ITradingSignalRepository      _signals;
    private readonly ISignalJobRepository          _jobs;
    private readonly ICandleRepository             _candles;
    private readonly ISignalOutcomeRepository      _outcomes;
    private readonly IDistributionRecordRepository _distributions;
    private readonly IAISignalEngineClient         _aiEngine;
    private readonly IDistributionJobScheduler     _scheduler;
    private readonly IUnitOfWork                   _uow;

    public SignalOrchestrationService(
        IAssetRepository              assets,
        ITradingSignalRepository      signals,
        ISignalJobRepository          jobs,
        ICandleRepository             candles,
        ISignalOutcomeRepository      outcomes,
        IDistributionRecordRepository distributions,
        IAISignalEngineClient         aiEngine,
        IDistributionJobScheduler     scheduler,
        IUnitOfWork                   uow)
    {
        _assets        = assets;
        _signals       = signals;
        _jobs          = jobs;
        _candles       = candles;
        _outcomes      = outcomes;
        _distributions = distributions;
        _aiEngine      = aiEngine;
        _scheduler     = scheduler;
        _uow           = uow;
    }

    public async Task OrchestrateForAssetAsync(Guid assetId, CancellationToken ct)
    {
        // ── Step 1 — get active asset ────────────────────────────────────────
        var asset = await _assets.GetByIdAsync(assetId, ct);
        if (asset == null || !asset.IsActive)
            throw new ArgumentException($"Asset {assetId} not found or inactive");

        // ── Step 2 — skip if active signal already exists ────────────────────
        var existing = await _signals.GetActiveForAssetAsync(assetId, ct);
        if (existing.Count > 0)
            return; // do not generate duplicate

        // ── Step 3 — get candles, check we have enough ───────────────────────
        var candles = await _candles.GetLatestAsync(
            assetId, asset.TimeFrame, asset.CandleContextWin, ct);

        if (candles.Count < asset.CandleContextWin)
            throw new InsufficientCandleDataException(
                asset.Symbol, candles.Count, asset.CandleContextWin);

        // ── Step 4 — build the AI request ────────────────────────────────────
        var candleDtos = candles.Select(c => new RawCandleDto(
            c.OpenTimeUtc, c.CloseTimeUtc,
            c.Open, c.High, c.Low, c.Close, c.Volume
        )).ToList();

        var request = new SignalEngineRequest(
            asset.Symbol, asset.TimeFrame, candleDtos);

        // ── Step 5-6 — create and save SignalJob (Queued) ────────────────────
        var job = new SignalJob(assetId, asset.Symbol, asset.TimeFrame, candles.Count);
        await _jobs.AddAsync(job, ct);
        await _uow.SaveChangesAsync(ct);

        // ── Step 7-8 — mark Running and save ─────────────────────────────────
        job.Begin();
        await _jobs.UpdateAsync(job, ct);
        await _uow.SaveChangesAsync(ct);

        // ── Step 9 — call AI engine ───────────────────────────────────────────
        SignalEngineResponse response;
        try
        {
            response = await _aiEngine.GenerateSignalAsync(request, ct);
            job.Complete();
        }
        catch (AIEngineUnavailableException ex)
        {
            job.Fail(ex.Message);
            await _jobs.UpdateAsync(job, ct);
            await _uow.SaveChangesAsync(ct);
            return; // stop here — no signal created
        }

        // ── Step 10 — create TradingSignal from AI response ──────────────────
        var validFrom  = DateTime.UtcNow;
        var expiresAt  = validFrom.AddHours(GetExpiryHours(asset.TimeFrame));

        var signal = new TradingSignal(
            assetId:           assetId,
            signalJobId:       job.SignalJobId,
            symbol:            asset.Symbol,
            direction:         response.Direction,
            confidenceScore:   response.ConfidenceScore,
            priceAtGeneration: candleDtos.Last().Close,
            reasoning:         response.Reasoning,
            validFrom:         validFrom,
            expiresAt:         expiresAt,
            entryZoneLow:      response.EntryZoneLow,
            entryZoneHigh:     response.EntryZoneHigh,
            stopLoss:          response.StopLoss,
            takeProfitOne:     response.TakeProfitOne,
            takeProfitTwo:     response.TakeProfitTwo);

        // ── Step 11 — create pending outcome ─────────────────────────────────
        var outcome = new SignalOutcome(signal.TradingSignalId);

        // ── TRANSACTION 1 — signal integrity ─────────────────────────────────
        // Job + Signal + Outcome are atomic. All or nothing.
        await _jobs.UpdateAsync(job, ct);
        await _signals.AddAsync(signal, ct);
        await _outcomes.AddAsync(outcome, ct);
        await _uow.SaveChangesAsync(ct); // ← ONE commit for all three
        // ─────────────────────────────────────────────────────────────────────

        // ── TRANSACTION 2 — distribution setup ───────────────────────────────
        // Separate commit. If this fails, signal is NOT rolled back.
        var channels = GetConfiguredChannels();
        foreach (var channel in channels)
        {
            var payload  = BuildPayload(signal, channel);
            var endpoint = GetEndpoint(channel);
            var record   = new DistributionRecord(
                signal.TradingSignalId, channel, endpoint, payload, maxAttempts: 5);
            await _distributions.AddRangeAsync(new[] { record }, ct);
        }
        await _uow.SaveChangesAsync(ct); // ← SEPARATE commit for distributions
        // ─────────────────────────────────────────────────────────────────────

        // ── Step 14 — schedule dispatch job ──────────────────────────────────
        await _scheduler.ScheduleDispatchJobAsync(ct);
    }

    // ── Helpers ──────────────────────────────────────────────────────────────
    private static int GetExpiryHours(TimeFrame timeFrame) => timeFrame switch
    {
        TimeFrame.M1  => 1,
        TimeFrame.M5  => 2,
        TimeFrame.M15 => 4,
        TimeFrame.H1  => 8,
        TimeFrame.H4  => 24,
        TimeFrame.D1  => 72,
        _             => 8
    };

    private static List<DistributionChannel> GetConfiguredChannels()
    {
        // MVP — hardcoded. Replace with config later.
        return new List<DistributionChannel>
        {
            DistributionChannel.Telegram,
            DistributionChannel.Discord
        };
    }

    private static string GetEndpoint(DistributionChannel channel)
    {
        // MVP — read from config/env vars in real implementation
        return channel switch
        {
            DistributionChannel.Telegram => Environment.GetEnvironmentVariable("TELEGRAM_WEBHOOK") ?? "",
            DistributionChannel.Discord  => Environment.GetEnvironmentVariable("DISCORD_WEBHOOK")  ?? "",
            _                            => ""
        };
    }

    private static string BuildPayload(TradingSignal signal, DistributionChannel channel)
    {
        // Clean JSON — Friend's bot formats it for display
        var payload = new
        {
            disclaimer       = "Probabilistic data analysis only. Not financial advice.",
            symbol           = signal.Symbol,
            direction        = signal.Direction.ToString(),
            confidence_level = signal.ConfidenceLevel.ToString(),
            confidence_score = signal.ConfidenceScore,
            generated_at     = signal.GeneratedAt,
            expires_at       = signal.ExpiresAt,
            // Premium fields — Friend's bot decides what to show
            entry_zone_low   = signal.EntryZoneLow,
            entry_zone_high  = signal.EntryZoneHigh,
            stop_loss        = signal.StopLoss,
            take_profit_one  = signal.TakeProfitOne,
            take_profit_two  = signal.TakeProfitTwo,
            reasoning        = signal.Reasoning
        };
        return JsonSerializer.Serialize(payload);
    }
}