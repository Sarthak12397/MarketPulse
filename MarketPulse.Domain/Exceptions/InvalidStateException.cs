public sealed class InvalidAssetStateException : InvalidOperationException
{
    public Guid AssetId { get; }
    public bool IsActive { get; }

    public InvalidAssetStateException(Guid assetId, bool isActive, string operation)
        : base($"Cannot {operation} asset {assetId}: already in state {isActive}")
    {
        AssetId = assetId;
        IsActive = isActive;
    }
}