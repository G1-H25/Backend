namespace GpsApp.Domain.ValueObjects;

/// <summary>
/// Value object representing a unique identifier for a package measurement session
/// </summary>
public class PackageMeasurementId : IEquatable<PackageMeasurementId>
{
    public Guid Value { get; private set; }

    public PackageMeasurementId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("PackageMeasurementId cannot be empty", nameof(value));
        
        Value = value;
    }

    /// <summary>
    /// Creates a new PackageMeasurementId with a random GUID
    /// </summary>
    public static PackageMeasurementId NewId() => new(Guid.NewGuid());

    /// <summary>
    /// Creates a PackageMeasurementId from a string representation
    /// </summary>
    public static PackageMeasurementId FromString(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("PackageMeasurementId string cannot be null or empty", nameof(value));
        
        if (!Guid.TryParse(value, out var guid))
            throw new ArgumentException("Invalid GUID format for PackageMeasurementId", nameof(value));
        
        return new PackageMeasurementId(guid);
    }

    public override string ToString() => Value.ToString();

    public override bool Equals(object? obj) => obj is PackageMeasurementId other && Equals(other);

    public override int GetHashCode() => Value.GetHashCode();

    public bool Equals(PackageMeasurementId? other) => other is not null && Value == other.Value;

    public static bool operator ==(PackageMeasurementId? left, PackageMeasurementId? right) => 
        left?.Equals(right) ?? right is null;

    public static bool operator !=(PackageMeasurementId? left, PackageMeasurementId? right) => 
        !(left == right);

    public static implicit operator Guid(PackageMeasurementId id) => id.Value;
}
