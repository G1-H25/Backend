namespace GpsApp.Domain.ValueObjects;

/// <summary>
/// Value object representing a unique shipment identifier
/// Implements IEquatable&lt;ShipmentId&gt; for comparison
/// </summary>
public class ShipmentId : IEquatable<ShipmentId>
{
    public Guid Value { get; private set; }

    public ShipmentId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("Shipment ID cannot be empty", nameof(value));

        Value = value;
    }

    public static ShipmentId NewId() => new(Guid.NewGuid());

    public override string ToString() => Value.ToString();

    public override bool Equals(object? obj) => obj is ShipmentId shipmentId && Equals(shipmentId);

    public override int GetHashCode() => Value.GetHashCode();

    public bool Equals(ShipmentId? other) => other is not null && Value == other.Value;

    public static bool operator ==(ShipmentId? left, ShipmentId? right) => left?.Equals(right) ?? right is null;

    public static bool operator !=(ShipmentId? left, ShipmentId? right) => !(left == right);

    public static implicit operator Guid(ShipmentId shipmentId) => shipmentId.Value;

    public static implicit operator ShipmentId(Guid value) => new(value);
}
