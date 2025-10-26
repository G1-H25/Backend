namespace GpsApp.Domain.ValueObjects;

/// <summary>
/// Value object representing a unique gateway identifier
/// Implements IEquatable&lt;GatewayId&gt; for comparison
/// Can never be null or empty
/// Can be invalid (00000000-0000-0000-0000-000000000000)
/// </summary>
public class GatewayId : IEquatable<GatewayId>
{
    public Guid Value { get; private set; }

    public GatewayId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("Gateway ID cannot be empty", nameof(value));

        Value = value;
    }

    public static GatewayId NewId() => new(Guid.NewGuid());

    public override string ToString() => Value.ToString();

    public override bool Equals(object? obj) => obj is GatewayId gatewayId && Equals(gatewayId);

    public override int GetHashCode() => Value.GetHashCode();

    public bool Equals(GatewayId? other) => other is not null && Value == other.Value;

    public static bool operator ==(GatewayId? left, GatewayId? right) => left?.Equals(right) ?? right is null;

    public static bool operator !=(GatewayId? left, GatewayId? right) => !(left == right);

    public static implicit operator Guid(GatewayId gatewayId) => gatewayId.Value;

    public static implicit operator GatewayId(Guid value) => new(value);
}