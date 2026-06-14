namespace MarketPulse.Domain.ValueObjects;

public class AssetSymbol : IEquatable<AssetSymbol>
{
    public string Value { get; }

    public AssetSymbol(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Symbol cannot be empty", nameof(value));

        Value = value.ToUpperInvariant().Trim();
    }

    // THIS IS WHAT WAS MISSING
    public bool Equals(AssetSymbol? other)
    {
        if (other is null) return false;
        return Value == other.Value;
    }

    public override bool Equals(object? obj)
        => Equals(obj as AssetSymbol);

    public override int GetHashCode()
        => Value.GetHashCode();

    public override string ToString()
        => Value;

    public static bool operator ==(AssetSymbol? left, AssetSymbol? right)
        => Equals(left, right);

    public static bool operator !=(AssetSymbol? left, AssetSymbol? right)
        => !Equals(left, right);
}