using MarketPulse.Domain.Entities;

public interface IAssetRepository
{
    Task<Asset?>GetByIdAsync(Guid assetId, CancellationToken ct);
    Task<IReadOnlyList<Asset>>GetAllActiveAsync(CancellationToken ct);
    Task<Asset?>GetBySymbolAsync(string symbol, CancellationToken ct);
    Task AddAsync(Asset asset, CancellationToken ct);
    Task UpdateAsync(Asset asset, CancellationToken ct);
}