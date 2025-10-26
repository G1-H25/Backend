using GpsApp.Domain.ValueObjects;

namespace GpsApp.Domain.ValueObjects;

/// <summary>
/// Status of a delivery leg in the shipment journey
/// </summary>
public enum DeliveryLegStatus
{
    /// <summary>
    /// Leg is planned but not yet started (no gateway assigned)
    /// </summary>
    Planned,
    
    /// <summary>
    /// Leg is ready to start (gateway assigned, waiting to begin)
    /// </summary>
    Ready,
    
    /// <summary>
    /// Leg is currently in progress
    /// </summary>
    InProgress,
    
    /// <summary>
    /// Leg has been completed successfully
    /// </summary>
    Completed
}

/// <summary>
/// DeliveryLeg value object representing a leg of a delivery journey
/// A delivery leg has a start and end address, which are addresses
/// For instance, there is always a start address and an end address, but there may be intermediate addresses in between
/// Each leg can be associated with a gateway/ECU that communicates with the sensors in the packages
/// The GatewayId is initially null and can be set later when workers load the truck in the warehouse
/// Each leg has a status that tracks its progression through the delivery journey
/// Implements IEquatable&lt;DeliveryLeg&gt; for comparison
/// </summary>
public class DeliveryLeg : IEquatable<DeliveryLeg>
{
    public Address StartAddress { get; private set; } = null!;
    public Address EndAddress { get; private set; } = null!;
    public GatewayId? GatewayId { get; private set; }
    public DeliveryLegStatus Status { get; private set; }

    public DeliveryLeg(Address startAddress, Address endAddress)
    {
        StartAddress = startAddress ?? throw new ArgumentNullException(nameof(startAddress));
        EndAddress = endAddress ?? throw new ArgumentNullException(nameof(endAddress));
        GatewayId = null; // Initially no gateway assigned
        Status = DeliveryLegStatus.Planned; // Initially planned
    }

    /// <summary>
    /// Assigns a gateway to this delivery leg (typically when workers load the truck)
    /// Changes status from Planned to Ready
    /// </summary>
    public DeliveryLeg AssignGateway(GatewayId gatewayId)
    {
        if (gatewayId == null)
            throw new ArgumentNullException(nameof(gatewayId));

        if (GatewayId != null)
            throw new InvalidOperationException("Gateway is already assigned to this delivery leg");

        if (Status != DeliveryLegStatus.Planned)
            throw new InvalidOperationException("Gateway can only be assigned to planned legs");

        return new DeliveryLeg(StartAddress, EndAddress) 
        { 
            GatewayId = gatewayId,
            Status = DeliveryLegStatus.Ready
        };
    }

    /// <summary>
    /// Starts this delivery leg (changes status from Ready to InProgress)
    /// </summary>
    public DeliveryLeg Start()
    {
        if (Status != DeliveryLegStatus.Ready)
            throw new InvalidOperationException("Only ready legs can be started");

        if (GatewayId == null)
            throw new InvalidOperationException("Leg must have a gateway assigned before starting");

        return new DeliveryLeg(StartAddress, EndAddress)
        {
            GatewayId = GatewayId!.Value,
            Status = DeliveryLegStatus.InProgress
        };
    }

    /// <summary>
    /// Completes this delivery leg (changes status from InProgress to Completed)
    /// </summary>
    public DeliveryLeg Complete()
    {
        if (Status != DeliveryLegStatus.InProgress)
            throw new InvalidOperationException("Only legs in progress can be completed");

        return new DeliveryLeg(StartAddress, EndAddress)
        {
            GatewayId = GatewayId!.Value,
            Status = DeliveryLegStatus.Completed
        };
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

    public override string ToString() => $"From {StartAddress} to {EndAddress}" + 
        (GatewayId != null ? $" (Gateway: {GatewayId})" : " (No Gateway)") +
        $" [{Status}]";

    public override bool Equals(object? obj) => obj is DeliveryLeg leg && Equals(leg);

    public override int GetHashCode() => HashCode.Combine(StartAddress, EndAddress, GatewayId?.Value ?? Guid.Empty, Status);

    public bool Equals(DeliveryLeg? other) => other is not null && 
        StartAddress.Equals(other.StartAddress) && 
        EndAddress.Equals(other.EndAddress) &&
        GatewayId?.Value == other.GatewayId?.Value &&
        Status == other.Status;

    public static bool operator ==(DeliveryLeg? left, DeliveryLeg? right) => left?.Equals(right) ?? right is null;

    public static bool operator !=(DeliveryLeg? left, DeliveryLeg? right) => !(left == right);
}