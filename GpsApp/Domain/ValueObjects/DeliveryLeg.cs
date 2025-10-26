using GpsApp.Domain.ValueObjects;

namespace GpsApp.Domain.ValueObjects;

/// <summary>
/// DeliveryLeg value object representing a leg of a delivery journey
/// A delivery leg has a start and end address, which are addresses
/// For instance, there is always a start address and an end address, but there may be intermediate addresses in between
/// Implements IEquatable&lt;DeliveryLeg&gt; for comparison
/// </summary>
public class DeliveryLeg : IEquatable<DeliveryLeg>
{
    public Address StartAddress { get; private set; } = null!;
    public Address EndAddress { get; private set; } = null!;

    public DeliveryLeg(Address startAddress, Address endAddress)
    {
        StartAddress = startAddress ?? throw new ArgumentNullException(nameof(startAddress));
        EndAddress = endAddress ?? throw new ArgumentNullException(nameof(endAddress));
    }

    /// <summary>
    /// Checks if this leg connects to another leg (this leg's end address matches the other leg's start address)
    /// </summary>
    public bool ConnectsTo(DeliveryLeg otherLeg)
    {
        if (otherLeg == null) return false;
        return EndAddress.Equals(otherLeg.StartAddress);
    }

    /// <summary>
    /// Checks if this leg is connected from another leg (this leg's start address matches the other leg's end address)
    /// </summary>
    public bool IsConnectedFrom(DeliveryLeg otherLeg)
    {
        if (otherLeg == null) return false;
        return StartAddress.Equals(otherLeg.EndAddress);
    }

    public override string ToString() => $"From {StartAddress} to {EndAddress}";

    public override bool Equals(object? obj) => obj is DeliveryLeg leg && Equals(leg);

    public override int GetHashCode() => HashCode.Combine(StartAddress, EndAddress);

    public bool Equals(DeliveryLeg? other) => other is not null && 
        StartAddress.Equals(other.StartAddress) && 
        EndAddress.Equals(other.EndAddress);

    public static bool operator ==(DeliveryLeg? left, DeliveryLeg? right) => left?.Equals(right) ?? right is null;

    public static bool operator !=(DeliveryLeg? left, DeliveryLeg? right) => !(left == right);
}