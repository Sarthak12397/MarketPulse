using System;
using MarketPulse.Domain.Enums;

namespace MarketPulse.Application.DTOs;


    public record SignalEngineResponse(
        SignalDirection Direction,
        decimal ConfidenceScore,
        string Reasoning,
        decimal? EntryZoneLow = null,
        decimal? EntryZoneHigh = null,
        decimal? StopLoss = null,
        decimal? TakeProfitOne = null,
        decimal? TakeProfitTwo = null
    );