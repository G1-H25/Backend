namespace GpsApp.Domain.ValueObjects;

/// <summary>
/// Value object representing a unique sensor identifier
/// Implements IEquatable&lt;SensorId&gt; for comparison
/// </summary>
public class SensorId : IEquatable<SensorId>
{
    public string Value { get; private set; }

    public SensorId(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Sensor ID cannot be null or empty", nameof(value));

        Value = value.Trim();
    }

    public override string ToString() => Value;

    public override bool Equals(object? obj) => obj is SensorId sensorId && Equals(sensorId);

    public override int GetHashCode() => Value.GetHashCode();

    public bool Equals(SensorId? other) => other is not null && Value == other.Value;

    public static bool operator ==(SensorId? left, SensorId? right) => left?.Equals(right) ?? right is null;

    public static bool operator !=(SensorId? left, SensorId? right) => !(left == right);

    public static implicit operator string(SensorId sensorId) => sensorId.Value;

    public static implicit operator SensorId(string value) => new(value);
}
