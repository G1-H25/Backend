using GpsApp.Domain.ValueObjects;

namespace GpsApp.Domain.ValueObjects;

/// <summary>
/// DeliveryLegId value object representing a unique identifier for a delivery leg
/// </summary>
public class DeliveryLegId
{
    public Guid Value { get; private set; }

    public DeliveryLegId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("DeliveryLegId cannot be empty", nameof(value));
        
        Value = value;
    }

    public static DeliveryLegId NewId() => new(Guid.NewGuid());

    public override string ToString() => Value.ToString();

    public override bool Equals(object? obj) => obj is DeliveryLegId other && Value == other.Value;

    public override int GetHashCode() => Value.GetHashCode();

    public static bool operator ==(DeliveryLegId? left, DeliveryLegId? right) => 
        left?.Value == right?.Value;

    public static bool operator !=(DeliveryLegId? left, DeliveryLegId? right) => 
        !(left == right);
}
