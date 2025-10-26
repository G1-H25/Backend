using GpsApp.Domain.ValueObjects;
using GpsApp.Domain.Events;

namespace GpsApp.Domain.Aggregates;

/// <summary>
/// Package aggregate root representing a shipping package
/// Follows Domain-Driven Design principles with proper encapsulation and business rules
/// </summary>
public class Package
{
    private readonly List<DomainEvent> _domainEvents = new();

    public PackageId PackageId { get; private set; } = null!;
    public Address Sender { get; private set; } = null!;
    public Address Recipient { get; private set; } = null!;
    public ExpectedRange<Temperature>? ExpectedTemperatureRange { get; private set; }
    public ExpectedRange<Humidity>? ExpectedHumidityRange { get; private set; }
    public SensorId? SensorId { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? SensorAttachedAt { get; private set; }

    // Domain events
    public IReadOnlyCollection<DomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    // Private constructor for EF Core or other ORM
    private Package() { }

    /// <summary>
    /// Creates a new package with sender and recipient addresses
    /// </summary>
    public Package(PackageId packageId, Address sender, Address recipient)
    {
        PackageId = packageId ?? throw new ArgumentNullException(nameof(packageId));
        Sender = sender ?? throw new ArgumentNullException(nameof(sender));
        Recipient = recipient ?? throw new ArgumentNullException(nameof(recipient));
        CreatedAt = DateTime.UtcNow;

        // Raise domain event
        AddDomainEvent(new PackageCreatedEvent(PackageId, Sender, Recipient));
    }

    /// <summary>
    /// Creates a package with all properties for hydration from persistent storage
    /// This constructor does not raise domain events as it's used for reconstruction
    /// </summary>
    public Package(PackageId packageId, Address sender, Address recipient, 
        SensorId? sensorId, ExpectedRange<Temperature>? expectedTemperatureRange, 
        ExpectedRange<Humidity>? expectedHumidityRange, DateTime createdAt, 
        DateTime? sensorAttachedAt)
    {
        PackageId = packageId ?? throw new ArgumentNullException(nameof(packageId));
        Sender = sender ?? throw new ArgumentNullException(nameof(sender));
        Recipient = recipient ?? throw new ArgumentNullException(nameof(recipient));
        SensorId = sensorId;
        ExpectedTemperatureRange = expectedTemperatureRange;
        ExpectedHumidityRange = expectedHumidityRange;
        CreatedAt = createdAt;
        SensorAttachedAt = sensorAttachedAt;
        
        // No domain events raised - this is for hydration from storage
    }

    /// <summary>
    /// Attaches a sensor to the package during packaging
    /// </summary>
    public void AttachSensor(SensorId sensorId)
    {
        if (sensorId == null)
            throw new ArgumentNullException(nameof(sensorId));

        if (SensorId != null)
            throw new InvalidOperationException("A sensor is already attached to this package");

        SensorId = sensorId;
        SensorAttachedAt = DateTime.UtcNow;

        // Raise domain event
        AddDomainEvent(new SensorAttachedEvent(PackageId, SensorId));
    }

    /// <summary>
    /// Sets the expected temperature range for the package
    /// </summary>
    public void SetExpectedTemperatureRange(ExpectedRange<Temperature> temperatureRange)
    {
        if (temperatureRange == null)
            throw new ArgumentNullException(nameof(temperatureRange));

        ExpectedTemperatureRange = temperatureRange;

        // Raise domain event
        AddDomainEvent(new ExpectedTemperatureRangeSetEvent(PackageId, ExpectedTemperatureRange));
    }

    /// <summary>
    /// Sets the expected humidity range for the package
    /// </summary>
    public void SetExpectedHumidityRange(ExpectedRange<Humidity> humidityRange)
    {
        if (humidityRange == null)
            throw new ArgumentNullException(nameof(humidityRange));

        ExpectedHumidityRange = humidityRange;

        // Raise domain event
        AddDomainEvent(new ExpectedHumidityRangeSetEvent(PackageId, ExpectedHumidityRange));
    }

    /// <summary>
    /// Checks if the package has a sensor attached
    /// </summary>
    public bool HasSensor => SensorId != null;

    /// <summary>
    /// Checks if the package is ready for shipping (has sensor and expected ranges)
    /// </summary>
    public bool IsReadyForShipping => HasSensor && ExpectedTemperatureRange != null && ExpectedHumidityRange != null;

    /// <summary>
    /// Validates if a temperature reading is within the expected range
    /// </summary>
    public bool IsTemperatureInRange(Temperature temperature)
    {
        if (ExpectedTemperatureRange == null)
            return true; // No range set, so any temperature is acceptable

        return ExpectedTemperatureRange.IsInRange(temperature);
    }

    /// <summary>
    /// Validates if a humidity reading is within the expected range
    /// </summary>
    public bool IsHumidityInRange(Humidity humidity)
    {
        if (ExpectedHumidityRange == null)
            return true; // No range set, so any humidity is acceptable

        return ExpectedHumidityRange.IsInRange(humidity);
    }

    /// <summary>
    /// Adds a domain event to the aggregate
    /// </summary>
    private void AddDomainEvent(DomainEvent domainEvent)
    {
        _domainEvents.Add(domainEvent);
    }

    /// <summary>
    /// Clears all domain events (typically called after persistence)
    /// </summary>
    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }

    public override string ToString() => $"Package {PackageId} from {Sender.City} to {Recipient.City}";
}