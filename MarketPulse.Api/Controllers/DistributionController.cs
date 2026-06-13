using MarketPulse.Application.Interfaces;
using MarketPulse.Domain.Enums;
using Microsoft.AspNetCore.Mvc;

namespace MarketPulse.Api.Controllers;

[ApiController]
[Route("api/distribution")]
public class DistributionController : ControllerBase
{
    private readonly IDistributionRecordRepository _distributions;

    public DistributionController(IDistributionRecordRepository distributions)
        => _distributions = distributions;

    [HttpGet("failed")]
    public async Task<IActionResult> GetFailed(CancellationToken ct)
    {
        var tier = HttpContext.Items["Tier"]?.ToString();
        if (tier != "Admin") return Forbid();

        var failed = await _distributions.GetAllFailedAsync(ct);
        var permanently = failed
            .Where(r => r.Status == DistributionStatus.PermanentlyFailed);
        return Ok(permanently);
    }

    [HttpGet("pending-count")]
    public async Task<IActionResult> GetPendingCount(CancellationToken ct)
    {
        var tier = HttpContext.Items["Tier"]?.ToString();
        if (tier != "Admin") return Forbid();

        var pending = await _distributions.GetAllPendingAsync(ct);
        return Ok(new { count = pending.Count });
    }
}