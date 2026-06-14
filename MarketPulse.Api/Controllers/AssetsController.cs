using MarketPulse.Application.Interfaces;
using MarketPulse.Domain.Entities;
using MarketPulse.Domain.Enums;
using Microsoft.AspNetCore.Mvc;

namespace MarketPulse.Api.Controllers;

[ApiController]
[Route("api/assets")]
public class AssetsController : ControllerBase
{
    private readonly IAssetRepository _assets;
    private readonly IUnitOfWork      _uow;

    public AssetsController(IAssetRepository assets, IUnitOfWork uow)
    {
        _assets = assets;
        _uow    = uow;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var all = await _assets.GetAllActiveAsync(ct);
        return Ok(all);
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreateAssetRequest request, CancellationToken ct)
    {
        var tier = HttpContext.Items["Tier"]?.ToString();
if (tier != "Admin") return StatusCode(403, "Forbidden. Admin access required.");

        var asset = new Asset(
            request.Symbol,
            request.BaseAsset,
            request.QuoteAsset,
            request.Provider,
            request.TimeFrame,
            request.CandleContextWindow);

        await _assets.AddAsync(asset, ct);
        await _uow.SaveChangesAsync(ct);

        return CreatedAtAction(nameof(GetAll), new { assetId = asset.AssetId });
    }

    [HttpPut("{id:guid}/activate")]
    public async Task<IActionResult> Activate(Guid id, CancellationToken ct)
    {
        var tier = HttpContext.Items["Tier"]?.ToString();
if (tier != "Admin") return StatusCode(403, "Forbidden. Admin access required.");

        var asset = await _assets.GetByIdAsync(id, ct);
        if (asset is null) return NotFound();

        try { asset.Activate(); }
        catch (Exception ex) { return BadRequest(ex.Message); }
        

        await _assets.UpdateAsync(asset, ct);
        await _uow.SaveChangesAsync(ct);
        return Ok();
    }

    [HttpPut("{id:guid}/deactivate")]
    public async Task<IActionResult> Deactivate(Guid id, CancellationToken ct)
    {
        var tier = HttpContext.Items["Tier"]?.ToString();
if (tier != "Admin") return StatusCode(403, "Forbidden. Admin access required.");

        var asset = await _assets.GetByIdAsync(id, ct);
        if (asset is null) return NotFound();

        try { asset.Deactivate(); }
        catch (Exception ex) { return BadRequest(ex.Message); }

        await _assets.UpdateAsync(asset, ct);
        await _uow.SaveChangesAsync(ct);
        return Ok();
    }
}

public record CreateAssetRequest(
    string Symbol,
    string BaseAsset,
    string QuoteAsset,
    string Provider,
    TimeFrame TimeFrame,
    int CandleContextWindow);