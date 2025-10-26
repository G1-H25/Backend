namespace GpsApp.Domain.ValueObjects;

/// <summary>
/// Value object representing a unique package identifier
/// Implements IEquatable&lt;PackageId&gt; for comparison
/// </summary>
public class PackageId : IEquatable<PackageId>
{
    public Guid Value { get; private set; }

    public PackageId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("Package ID cannot be empty", nameof(value));

        Value = value;
    }

    public static PackageId NewId() => new(Guid.NewGuid());

    public override string ToString() => Value.ToString();

    public override bool Equals(object? obj) => obj is PackageId packageId && Equals(packageId);

    public override int GetHashCode() => Value.GetHashCode();

    public bool Equals(PackageId? other) => other is not null && Value == other.Value;

    public static bool operator ==(PackageId? left, PackageId? right) => left?.Equals(right) ?? right is null;

    public static bool operator !=(PackageId? left, PackageId? right) => !(left == right);

    public static implicit operator Guid(PackageId packageId) => packageId.Value;

    public static implicit operator PackageId(Guid value) => new(value);
}
