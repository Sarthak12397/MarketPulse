using MarketPulse.Application.Interfaces;
using MarketPulse.Domain.Entities;
using MarketPulse.Domain.Enums;
using Microsoft.AspNetCore.Mvc;

namespace MarketPulse.Api.Controllers;

[ApiController]
[Route("api/signals")]
public class SignalsController : ControllerBase
{
    private readonly ITradingSignalRepository  _signals;
    private readonly ISignalOutcomeRepository  _outcomes;

    public SignalsController(
        ITradingSignalRepository signals,
        ISignalOutcomeRepository outcomes)
    {
        _signals  = signals;
        _outcomes = outcomes;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] Guid?   assetId,
        [FromQuery] string? status,
        CancellationToken ct)
    {
        var tier    = HttpContext.Items["Tier"]?.ToString() ?? "Free";
        var active  = await _signals.GetActiveForAssetAsync(assetId ?? Guid.Empty, ct);
        var mapped  = active.Select(s => MapSignal(s, tier));
        return Ok(mapped);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var tier   = HttpContext.Items["Tier"]?.ToString() ?? "Free";
        var signal = await _signals.GetByIdAsync(id, ct);
        if (signal is null) return NotFound();
        return Ok(MapSignal(signal, tier));
    }

    [HttpGet("{id:guid}/outcome")]
    public async Task<IActionResult> GetOutcome(Guid id, CancellationToken ct)
    {
        var outcome = await _outcomes.GetBySignalIdAsync(id, ct);
        if (outcome is null) return NotFound();
        return Ok(outcome);
    }

    [HttpGet("metrics")]
    public async Task<IActionResult> GetMetrics(CancellationToken ct)
    {
        var pending = await _outcomes.GetAllPendingAsync(ct);
        // TODO: expand with full metrics query later
        return Ok(new { Message = "Metrics endpoint — expand in v2" });
    }

    private static object MapSignal(TradingSignal s, string tier)
    {
        const string disclaimer = "Probabilistic data analysis only. Not financial advice.";

        if (tier == "Premium" || tier == "Admin")
        {
            return new
            {
                s.TradingSignalId,
                s.Symbol,
                Direction       = s.Direction.ToString(),
                ConfidenceLevel = s.ConfidenceLevel.ToString(),
                s.ConfidenceScore,
                s.GeneratedAt,
                s.ExpiresAt,
                s.EntryZoneLow,
                s.EntryZoneHigh,
                s.StopLoss,
                s.TakeProfitOne,
                s.TakeProfitTwo,
                s.Reasoning,
                Disclaimer = disclaimer
            };
        }

        // Free tier — Law 2 requires ConfidenceScore always visible
        return new
        {
            s.TradingSignalId,
            s.Symbol,
            Direction       = s.Direction.ToString(),
            ConfidenceLevel = s.ConfidenceLevel.ToString(),
            s.ConfidenceScore,
            s.GeneratedAt,
            s.ExpiresAt,
            Disclaimer = disclaimer
        };
    }
}