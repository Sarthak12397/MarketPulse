using MarketPulse.Domain.Exceptions;

namespace MarketPulse.Domain.Entities;

public class SignalOutcome
{
    public Guid SignalOutcomeId { get; init; }
    public Guid TradingSignalId { get; init; }
    public DateTime CreatedAt { get; init; }

    public OutComeResult? OutcomeResult { get; private set; }
    public decimal? PriceAtExpiry { get; private set; }
    public decimal? PriceChangePercent { get; private set; }
    public string? EvaluationNotes { get; private set; }
    public DateTime? EvaluatedAt { get; private set; }

    protected SignalOutcome() { }

    public SignalOutcome(Guid tradingSignalId)
    {
        if (tradingSignalId == Guid.Empty)
            throw new ArgumentException("TradingSignalId cannot be empty");

        SignalOutcomeId = Guid.NewGuid();
        TradingSignalId = tradingSignalId;
        OutcomeResult   = null;
        CreatedAt       = DateTime.UtcNow;
    }

    public void Evaluate(
        OutComeResult result,
        decimal priceAtExpiry,
        decimal priceChangePercent,
        string? notes)
    {
        if (EvaluatedAt != null)
            throw new InvalidSignalOutcomeStateException(SignalOutcomeId);

        if (priceAtExpiry <= 0)
            throw new ArgumentException("PriceAtExpiry must be greater than zero");

        OutcomeResult      = result;
        PriceAtExpiry      = priceAtExpiry;
        PriceChangePercent = priceChangePercent;
        EvaluationNotes    = notes;
        EvaluatedAt        = DateTime.UtcNow;
    }
}