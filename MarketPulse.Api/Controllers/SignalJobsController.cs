using MarketPulse.Application.Interfaces;
using MarketPulse.Application.Jobs;
using Hangfire;
using Microsoft.AspNetCore.Mvc;

namespace MarketPulse.Api.Controllers;

[ApiController]
[Route("api/signal-jobs")]
public class SignalJobsController : ControllerBase
{
    private readonly IAssetRepository      _assets;
    private readonly IBackgroundJobClient  _jobClient;

    public SignalJobsController(
        IAssetRepository     assets,
        IBackgroundJobClient jobClient)
    {
        _assets    = assets;
        _jobClient = jobClient;
    }

    [HttpPost("trigger/{assetId:guid}")]
    public async Task<IActionResult> Trigger(Guid assetId, CancellationToken ct)
    {
        var tier = HttpContext.Items["Tier"]?.ToString();
if (tier != "Admin") return StatusCode(403, "Forbidden. Admin access required.");

        var asset = await _assets.GetByIdAsync(assetId, ct);
        if (asset is null || !asset.IsActive)
            return NotFound("Asset not found or inactive");

        _jobClient.Enqueue<FetchAndGenerateJob>(
            j => j.ExecuteForTimeFrameAsync(asset.TimeFrame, CancellationToken.None));

        return Accepted(new { assetId });
    }
}