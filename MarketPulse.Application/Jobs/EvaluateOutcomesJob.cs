using Hangfire;
using MarketPulse.Application.Services;

namespace MarketPulse.Application.Jobs;

public class EvaluateOutcomesJob
{
    private readonly OutcomeEvaluationService _evaluation;

    public EvaluateOutcomesJob(OutcomeEvaluationService evaluation)
        => _evaluation = evaluation;

    [Queue("evaluation")]
    public async Task ExecuteAsync(CancellationToken ct)
        => await _evaluation.EvaluateExpiredAsync(ct);
}