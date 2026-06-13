using MarketPulse.Application.Interfaces;
using MarketPulse.Domain.Entities;
using MarketPulse.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace MarketPulse.Infrastructure.Persistence.Repositories;

public class AssetRepository : IAssetRepository
{
    private readonly AppDbContext _db;
    public AssetRepository(AppDbContext db) => _db = db;

    public async Task<Asset?> GetByIdAsync(Guid assetId, CancellationToken ct)
        => await _db.Assets.FindAsync(new object[] { assetId }, ct);

    public async Task<IReadOnlyList<Asset>> GetAllActiveAsync(CancellationToken ct)
        => await _db.Assets.Where(a => a.IsActive).ToListAsync(ct);

    public async Task<Asset?> GetBySymbolAsync(string symbol, CancellationToken ct)
        => await _db.Assets.FirstOrDefaultAsync(a => a.Symbol == symbol, ct);

    public async Task AddAsync(Asset asset, CancellationToken ct)
        => await _db.Assets.AddAsync(asset, ct);

    public Task UpdateAsync(Asset asset, CancellationToken ct)
    {
        _db.Assets.Update(asset);
        return Task.CompletedTask;
    }
}