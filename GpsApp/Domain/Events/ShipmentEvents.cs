using GpsApp.Domain.ValueObjects;
using GpsApp.Domain.Aggregates;

namespace GpsApp.Domain.Events;

/// <summary>
/// Event raised when a shipment is created
/// </summary>
public class ShipmentCreatedEvent : DomainEvent
{
    public ShipmentId ShipmentId { get; private set; }
    public DateTime ShipmentDate { get; private set; }
    public IReadOnlyList<PackageId> PackageIds { get; private set; }
    public IReadOnlyList<DeliveryLeg> DeliveryLegs { get; private set; }

    public ShipmentCreatedEvent(ShipmentId shipmentId, DateTime shipmentDate, IReadOnlyList<PackageId> packageIds, IReadOnlyList<DeliveryLeg> deliveryLegs)
        : base(shipmentId.Value.ToString(), "Shipment")
    {
        ShipmentId = shipmentId;
        ShipmentDate = shipmentDate;
        PackageIds = packageIds;
        DeliveryLegs = deliveryLegs;
    }
}

/// <summary>
/// Event raised when a package is added to a shipment
/// </summary>
public class PackageAddedToShipmentEvent : DomainEvent
{
    public ShipmentId ShipmentId { get; private set; }
    public PackageId PackageId { get; private set; }

    public PackageAddedToShipmentEvent(ShipmentId shipmentId, PackageId packageId)
        : base(shipmentId.Value.ToString(), "Shipment")
    {
        ShipmentId = shipmentId;
        PackageId = packageId;
    }
}

/// <summary>
/// Event raised when a delivery leg is added to a shipment
/// </summary>
public class DeliveryLegAddedToShipmentEvent : DomainEvent
{
    public ShipmentId ShipmentId { get; private set; }
    public DeliveryLeg DeliveryLeg { get; private set; }

    public DeliveryLegAddedToShipmentEvent(ShipmentId shipmentId, DeliveryLeg deliveryLeg)
        : base(shipmentId.Value.ToString(), "Shipment")
    {
        ShipmentId = shipmentId;
        DeliveryLeg = deliveryLeg;
    }
}

/// <summary>
/// Event raised when a gateway is connected to a delivery leg of a shipment
/// </summary>
public class GatewayConnectedToDeliveryLegEvent : DomainEvent
{
    public ShipmentId ShipmentId { get; private set; }
    public DeliveryLeg DeliveryLeg { get; private set; }
    public GatewayId GatewayId { get; private set; }

    public GatewayConnectedToDeliveryLegEvent(ShipmentId shipmentId, DeliveryLeg deliveryLeg, GatewayId gatewayId)
        : base(shipmentId.Value.ToString(), "Shipment")
    {
        ShipmentId = shipmentId;
        DeliveryLeg = deliveryLeg;
        GatewayId = gatewayId;
    }
}

/// <summary>
/// Event raised when shipment status changes
/// </summary>
public class ShipmentStatusChangedEvent : DomainEvent
{
    public ShipmentId ShipmentId { get; private set; }
    public Shipment.ShipmentStatus OldStatus { get; private set; }
    public Shipment.ShipmentStatus NewStatus { get; private set; }

    public ShipmentStatusChangedEvent(ShipmentId shipmentId, Shipment.ShipmentStatus oldStatus, Shipment.ShipmentStatus newStatus)
        : base(shipmentId.Value.ToString(), "Shipment")
    {
        ShipmentId = shipmentId;
        OldStatus = oldStatus;
        NewStatus = newStatus;
    }
}

/// <summary>
/// Event raised when a delivery leg is started
/// </summary>
public class DeliveryLegStartedEvent : DomainEvent
{
    public ShipmentId ShipmentId { get; private set; }
    public DeliveryLeg DeliveryLeg { get; private set; }

    public DeliveryLegStartedEvent(ShipmentId shipmentId, DeliveryLeg deliveryLeg)
        : base(shipmentId.Value.ToString(), "Shipment")
    {
        ShipmentId = shipmentId;
        DeliveryLeg = deliveryLeg;
    }
}

/// <summary>
/// Event raised when a delivery leg is completed
/// </summary>
public class DeliveryLegCompletedEvent : DomainEvent
{
    public ShipmentId ShipmentId { get; private set; }
    public DeliveryLeg DeliveryLeg { get; private set; }

    public DeliveryLegCompletedEvent(ShipmentId shipmentId, DeliveryLeg deliveryLeg)
        : base(shipmentId.Value.ToString(), "Shipment")
    {
        ShipmentId = shipmentId;
        DeliveryLeg = deliveryLeg;
    }
}
