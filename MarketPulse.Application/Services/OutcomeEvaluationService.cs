using MarketPulse.Application.Interfaces;
using MarketPulse.Domain.Enums;

namespace MarketPulse.Application.Services;

public class OutcomeEvaluationService
{
    private readonly ITradingSignalRepository _signals;
    private readonly ISignalOutcomeRepository _outcomes;
    private readonly IPriceDataClient         _price;
    private readonly IUnitOfWork              _uow;

    public OutcomeEvaluationService(
        ITradingSignalRepository signals,
        ISignalOutcomeRepository outcomes,
        IPriceDataClient         price,
        IUnitOfWork              uow)
    {
        _signals  = signals;
        _outcomes = outcomes;
        _price    = price;
        _uow      = uow;
    }

    public async Task EvaluateExpiredAsync(CancellationToken ct)
    {
        // Gets Active signals where ExpiresAt <= UtcNow
        var expiredSignals = await _signals.GetExpiredUnevaluatedAsync(ct);

        if (expiredSignals.Count == 0)
            return;

        foreach (var signal in expiredSignals)
        {
            // Step a — transition Active -> Expired
            signal.Expire();

            // Step b — get the outcome record
            var outcome = await _outcomes.GetBySignalIdAsync(signal.TradingSignalId, ct);
            if (outcome == null)
            {
                // Should never happen if OrchestrationService ran correctly
                // Log anomaly and skip — do not crash the whole batch
                continue;
            }

            // Step c — get current market price
            var currentPrice = await _price.GetCurrentPriceAsync(signal.Symbol, ct);

            // Step d — calculate percentage change
            // CORRECT: divide by price THEN multiply by 100
            var pct = (currentPrice - signal.PriceAtGeneration)
                      / signal.PriceAtGeneration * 100;

            // Step e — determine Win / Loss / Neutral
            var result = DetermineOutcome(signal.Direction, pct);

            // Step f — evaluation notes
            var notes = $"Auto-evaluated. Final price: {currentPrice}. Change: {pct:F2}%";

            // Step g — evaluate outcome
            outcome.Evaluate(result, currentPrice, pct, notes);

            // Step h — mark signal as evaluated
            signal.MarkEvaluated();

            // Step i — save both in one transaction
            await _signals.UpdateAsync(signal, ct);
            await _outcomes.UpdateAsync(outcome, ct);
            await _uow.SaveChangesAsync(ct);
        }
    }

    private static OutComeResult DetermineOutcome(SignalDirection direction, decimal pct)
    {
        return direction switch
        {
            SignalDirection.Long =>
                pct > 0.5m  ? OutComeResult.Win  :
                pct < -0.5m ? OutComeResult.Loss :
                              OutComeResult.Neutral,

            SignalDirection.Short =>
                pct < -0.5m ? OutComeResult.Win  :
                pct > 0.5m  ? OutComeResult.Loss :
                              OutComeResult.Neutral,

            SignalDirection.Neutral =>
                Math.Abs(pct) < 0.5m ? OutComeResult.Neutral
                                     : OutComeResult.Inconclusive,

            _ => OutComeResult.Inconclusive
        };
    }
}