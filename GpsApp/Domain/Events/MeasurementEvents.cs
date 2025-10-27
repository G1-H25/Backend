using GpsApp.Domain.ValueObjects;

namespace GpsApp.Domain.Events;

/// <summary>
/// Event raised when a measurement session starts for a delivery leg
/// </summary>
public class MeasurementSessionStartedEvent : DomainEvent
{
    public ShipmentId ShipmentId { get; private set; }
    public DeliveryLeg DeliveryLeg { get; private set; }
    public IReadOnlyList<PackageId> PackageIds { get; private set; }
    public GatewayId GatewayId { get; private set; }

    public MeasurementSessionStartedEvent(ShipmentId shipmentId, DeliveryLeg deliveryLeg, 
        IReadOnlyList<PackageId> packageIds, GatewayId gatewayId)
        : base(shipmentId.Value.ToString(), "Shipment")
    {
        ShipmentId = shipmentId;
        DeliveryLeg = deliveryLeg;
        PackageIds = packageIds;
        GatewayId = gatewayId;
    }
}

/// <summary>
/// Event raised when sensor presence is verified by the gateway
/// </summary>
public class SensorPresenceVerifiedEvent : DomainEvent
{
    public ShipmentId ShipmentId { get; private set; }
    public DeliveryLeg DeliveryLeg { get; private set; }
    public IReadOnlyList<SensorId> PresentSensors { get; private set; }
    public IReadOnlyList<SensorId> MissingSensors { get; private set; }

    public SensorPresenceVerifiedEvent(ShipmentId shipmentId, DeliveryLeg deliveryLeg,
        IReadOnlyList<SensorId> presentSensors, IReadOnlyList<SensorId> missingSensors)
        : base(shipmentId.Value.ToString(), "Shipment")
    {
        ShipmentId = shipmentId;
        DeliveryLeg = deliveryLeg;
        PresentSensors = presentSensors;
        MissingSensors = missingSensors;
    }
}

/// <summary>
/// Event raised when a batch of measurements is received from the gateway
/// </summary>
public class MeasurementBatchReceivedEvent : DomainEvent
{
    public ShipmentId ShipmentId { get; private set; }
    public DeliveryLeg DeliveryLeg { get; private set; }
    public IReadOnlyList<MeasurementReading> Readings { get; private set; }

    public MeasurementBatchReceivedEvent(ShipmentId shipmentId, DeliveryLeg deliveryLeg,
        IReadOnlyList<MeasurementReading> readings)
        : base(shipmentId.Value.ToString(), "Shipment")
    {
        ShipmentId = shipmentId;
        DeliveryLeg = deliveryLeg;
        Readings = readings;
    }
}

/// <summary>
/// Event raised when a measurement reading is out of expected range
/// </summary>
public class MeasurementOutOfRangeEvent : DomainEvent
{
    public PackageId PackageId { get; private set; }
    public MeasurementReading Reading { get; private set; }
    public string ViolationType { get; private set; }

    public MeasurementOutOfRangeEvent(PackageId packageId, MeasurementReading reading, string violationType)
        : base(packageId.Value.ToString(), "Package")
    {
        PackageId = packageId;
        Reading = reading;
        ViolationType = violationType;
    }
}

/// <summary>
/// Event raised when a measurement session is completed for a delivery leg
/// </summary>
public class MeasurementSessionCompletedEvent : DomainEvent
{
    public ShipmentId ShipmentId { get; private set; }
    public DeliveryLeg DeliveryLeg { get; private set; }
    public MeasurementSummary Summary { get; private set; }

    public MeasurementSessionCompletedEvent(ShipmentId shipmentId, DeliveryLeg deliveryLeg, MeasurementSummary summary)
        : base(shipmentId.Value.ToString(), "Shipment")
    {
        ShipmentId = shipmentId;
        DeliveryLeg = deliveryLeg;
        Summary = summary;
    }
}

/// <summary>
/// Event raised when a measurement batch is recorded in the PackageMeasurement aggregate
/// </summary>
public class MeasurementBatchRecordedEvent : DomainEvent
{
    public PackageMeasurementId PackageMeasurementId { get; private set; }
    public IReadOnlyList<MeasurementReading> Readings { get; private set; }

    public MeasurementBatchRecordedEvent(PackageMeasurementId packageMeasurementId, IReadOnlyList<MeasurementReading> readings)
        : base(packageMeasurementId.Value.ToString(), "PackageMeasurement")
    {
        PackageMeasurementId = packageMeasurementId;
        Readings = readings;
    }
}

/// <summary>
/// Event raised when a package measurement session is completed
/// </summary>
public class PackageMeasurementCompletedEvent : DomainEvent
{
    public PackageMeasurementId PackageMeasurementId { get; private set; }
    public PackageId PackageId { get; private set; }
    public MeasurementSummary Summary { get; private set; }

    public PackageMeasurementCompletedEvent(PackageMeasurementId packageMeasurementId, PackageId packageId, MeasurementSummary summary)
        : base(packageMeasurementId.Value.ToString(), "PackageMeasurement")
    {
        PackageMeasurementId = packageMeasurementId;
        PackageId = packageId;
        Summary = summary;
    }
}
