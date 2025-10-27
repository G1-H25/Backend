namespace GpsApp.Domain.ValueObjects;

/// <summary>
/// Value object representing a unique sensor identifier
/// Implements IEquatable&lt;SensorId&gt; for comparison
/// </summary>
public class SensorId : IEquatable<SensorId>
{
    public Guid Value { get; private set; }

    public SensorId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("Sensor ID cannot be empty", nameof(value));

        Value = value;
    }

    public static SensorId NewId() => new(Guid.NewGuid());

    public override string ToString() => Value.ToString();

    public override bool Equals(object? obj) => obj is SensorId sensorId && Equals(sensorId);

    public override int GetHashCode() => Value.GetHashCode();

    public bool Equals(SensorId? other) => other is not null && Value == other.Value;

    public static bool operator ==(SensorId? left, SensorId? right) => left?.Equals(right) ?? right is null;

    public static bool operator !=(SensorId? left, SensorId? right) => !(left == right);

    public static implicit operator Guid(SensorId sensorId) => sensorId.Value;

    public static implicit operator SensorId(Guid value) => new(value);
}
