using GpsApp.Domain.ValueObjects;

namespace GpsApp.Domain.Events;

/// <summary>
/// Base class for domain events
/// </summary>
public abstract class DomainEvent
{
    public DateTime OccurredAt { get; private set; } = DateTime.UtcNow;
    public Guid EventId { get; private set; } = Guid.NewGuid();
    
    // Properties for EF Core persistence
    public string AggregateId { get; private set; } = string.Empty;
    public string AggregateType { get; private set; } = string.Empty;
    public int Version { get; private set; } = 1;
    
    // Protected constructor for EF Core
    protected DomainEvent() { }
    
    // Constructor for domain events
    protected DomainEvent(string aggregateId, string aggregateType, int version = 1)
    {
        AggregateId = aggregateId;
        AggregateType = aggregateType;
        Version = version;
    }
}

/// <summary>
/// Event raised when a package is created
/// </summary>
public class PackageCreatedEvent : DomainEvent
{
    public PackageId PackageId { get; private set; }
    public Address Sender { get; private set; }
    public Address Recipient { get; private set; }

    public PackageCreatedEvent(PackageId packageId, Address sender, Address recipient)
        : base(packageId.Value.ToString(), "Package")
    {
        PackageId = packageId;
        Sender = sender;
        Recipient = recipient;
    }
}

/// <summary>
/// Event raised when a sensor is attached to a package
/// </summary>
public class SensorAttachedEvent : DomainEvent
{
    public PackageId PackageId { get; private set; }
    public SensorId SensorId { get; private set; }

    public SensorAttachedEvent(PackageId packageId, SensorId sensorId)
        : base(packageId.Value.ToString(), "Package")
    {
        PackageId = packageId;
        SensorId = sensorId;
    }
}

/// <summary>
/// Event raised when expected temperature range is set for a package
/// </summary>
public class ExpectedTemperatureRangeSetEvent : DomainEvent
{
    public PackageId PackageId { get; private set; }
    public ExpectedRange<Temperature> ExpectedRange { get; private set; }

    public ExpectedTemperatureRangeSetEvent(PackageId packageId, ExpectedRange<Temperature> expectedRange)
        : base(packageId.Value.ToString(), "Package")
    {
        PackageId = packageId;
        ExpectedRange = expectedRange;
    }
}

/// <summary>
/// Event raised when expected humidity range is set for a package
/// </summary>
public class ExpectedHumidityRangeSetEvent : DomainEvent
{
    public PackageId PackageId { get; private set; }
    public ExpectedRange<Humidity> ExpectedRange { get; private set; }

    public ExpectedHumidityRangeSetEvent(PackageId packageId, ExpectedRange<Humidity> expectedRange)
        : base(packageId.Value.ToString(), "Package")
    {
        PackageId = packageId;
        ExpectedRange = expectedRange;
    }
}
