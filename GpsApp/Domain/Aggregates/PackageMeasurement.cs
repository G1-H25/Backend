using GpsApp.Domain.ValueObjects;
using GpsApp.Domain.Events;

namespace GpsApp.Domain.Aggregates;

/// <summary>
/// PackageMeasurement aggregate root representing a measurement session for a package during a delivery leg
/// Follows Domain-Driven Design principles with proper encapsulation and business rules
/// </summary>
public class PackageMeasurement
{
    private readonly List<DomainEvent> _domainEvents = new();
    private readonly List<MeasurementReading> _readings = new();

    public PackageMeasurementId Id { get; private set; } = null!;
    public PackageId PackageId { get; private set; } = null!;
    public DeliveryLeg DeliveryLeg { get; private set; } = null!;
    public DateTime SessionStartTime { get; private set; }
    public DateTime? SessionEndTime { get; private set; }
    public ExpectedRange<Temperature>? ExpectedTemperatureRange { get; private set; }
    public ExpectedRange<Humidity>? ExpectedHumidityRange { get; private set; }

    // Domain events
    public IReadOnlyCollection<DomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    // Measurements
    public IReadOnlyList<MeasurementReading> Readings => _readings.AsReadOnly();

    // Private constructor for EF Core or other ORM
    private PackageMeasurement() { }

    /// <summary>
    /// Creates a new package measurement session
    /// </summary>
    public PackageMeasurement(PackageMeasurementId id, PackageId packageId, DeliveryLeg deliveryLeg, ShipmentId shipmentId,
        ExpectedRange<Temperature>? expectedTemperatureRange = null, ExpectedRange<Humidity>? expectedHumidityRange = null)
    {
        Id = id ?? throw new ArgumentNullException(nameof(id));
        PackageId = packageId ?? throw new ArgumentNullException(nameof(packageId));
        DeliveryLeg = deliveryLeg ?? throw new ArgumentNullException(nameof(deliveryLeg));
        SessionStartTime = DateTime.UtcNow;
        ExpectedTemperatureRange = expectedTemperatureRange;
        ExpectedHumidityRange = expectedHumidityRange;

        // Raise domain event
        AddDomainEvent(new MeasurementSessionStartedEvent(
            shipmentId,
            DeliveryLeg, 
            new List<PackageId> { PackageId }, 
            DeliveryLeg.GatewayId!.Value));
    }

    /// <summary>
    /// Creates a package measurement from persistence storage (hydration constructor)
    /// This constructor does not raise domain events as it's used for reconstruction
    /// </summary>
    public PackageMeasurement(PackageMeasurementId id, PackageId packageId, DeliveryLeg deliveryLeg,
        DateTime sessionStartTime, DateTime? sessionEndTime,
        ExpectedRange<Temperature>? expectedTemperatureRange, ExpectedRange<Humidity>? expectedHumidityRange,
        IReadOnlyList<MeasurementReading> readings)
    {
        Id = id ?? throw new ArgumentNullException(nameof(id));
        PackageId = packageId ?? throw new ArgumentNullException(nameof(packageId));
        DeliveryLeg = deliveryLeg ?? throw new ArgumentNullException(nameof(deliveryLeg));
        SessionStartTime = sessionStartTime;
        SessionEndTime = sessionEndTime;
        ExpectedTemperatureRange = expectedTemperatureRange;
        ExpectedHumidityRange = expectedHumidityRange;

        // Add readings without raising events
        _readings.AddRange(readings ?? new List<MeasurementReading>());
    }

    /// <summary>
    /// Records a batch of measurements from the gateway
    /// </summary>
    public void RecordMeasurementBatch(IReadOnlyList<MeasurementReading> readings)
    {
        if (readings == null || readings.Count == 0)
            throw new ArgumentException("Cannot record empty measurement batch", nameof(readings));

        if (SessionEndTime.HasValue)
            throw new InvalidOperationException("Cannot record measurements for completed session");

        foreach (var reading in readings)
        {
            _readings.Add(reading);
            
            // Check for out-of-range values and raise events
            CheckForOutOfRangeValues(reading);
        }

        // Raise domain event for the batch
        AddDomainEvent(new MeasurementBatchRecordedEvent(Id, readings));
    }

    /// <summary>
    /// Records a single measurement reading
    /// </summary>
    public void RecordMeasurement(MeasurementReading reading)
    {
        if (reading == null)
            throw new ArgumentNullException(nameof(reading));

        if (SessionEndTime.HasValue)
            throw new InvalidOperationException("Cannot record measurements for completed session");

        _readings.Add(reading);
        
        // Check for out-of-range values and raise events
        CheckForOutOfRangeValues(reading);

        // Raise domain event for the single reading
        AddDomainEvent(new MeasurementBatchRecordedEvent(Id, new List<MeasurementReading> { reading }));
    }

    /// <summary>
    /// Completes the measurement session
    /// </summary>
    public void CompleteSession()
    {
        if (SessionEndTime.HasValue)
            throw new InvalidOperationException("Session is already completed");

        SessionEndTime = DateTime.UtcNow;
        
        var summary = GetSummary();
        AddDomainEvent(new PackageMeasurementCompletedEvent(Id, PackageId, summary));
    }

    /// <summary>
    /// Gets measurements for a specific time range (for graphing)
    /// </summary>
    public IReadOnlyList<MeasurementReading> GetReadingsForPeriod(DateTime startTime, DateTime endTime)
    {
        return _readings
            .Where(r => r.Timestamp >= startTime && r.Timestamp <= endTime)
            .OrderBy(r => r.Timestamp)
            .ToList();
    }

    /// <summary>
    /// Gets measurements at specific intervals (for graph data points)
    /// </summary>
    public IReadOnlyList<MeasurementReading> GetReadingsAtInterval(TimeSpan interval)
    {
        if (_readings.Count == 0)
            return new List<MeasurementReading>();

        var orderedReadings = _readings.OrderBy(r => r.Timestamp).ToList();
        var result = new List<MeasurementReading>();
        var currentTime = orderedReadings.First().Timestamp;

        while (currentTime <= orderedReadings.Last().Timestamp)
        {
            var readingAtTime = orderedReadings
                .Where(r => r.Timestamp <= currentTime)
                .LastOrDefault();

            if (readingAtTime != null)
            {
                result.Add(readingAtTime);
            }

            currentTime = currentTime.Add(interval);
        }

        return result;
    }

    /// <summary>
    /// Gets aggregated data (min, max, average) for the entire session
    /// </summary>
    public MeasurementSummary GetSummary()
    {
        return MeasurementSummary.FromReadings(_readings, ExpectedTemperatureRange, ExpectedHumidityRange);
    }

    /// <summary>
    /// Gets aggregated data for a specific time period
    /// </summary>
    public MeasurementSummary GetSummaryForPeriod(DateTime startTime, DateTime endTime)
    {
        var periodReadings = GetReadingsForPeriod(startTime, endTime);
        return MeasurementSummary.FromReadings(periodReadings, ExpectedTemperatureRange, ExpectedHumidityRange);
    }

    /// <summary>
    /// Checks if the session is active (not completed)
    /// </summary>
    public bool IsActive => !SessionEndTime.HasValue;

    /// <summary>
    /// Gets the duration of the measurement session
    /// </summary>
    public TimeSpan? Duration => SessionEndTime?.Subtract(SessionStartTime);

    /// <summary>
    /// Gets the number of readings recorded
    /// </summary>
    public int ReadingCount => _readings.Count;

    /// <summary>
    /// Checks for out-of-range values and raises appropriate events
    /// </summary>
    private void CheckForOutOfRangeValues(MeasurementReading reading)
    {
        var violationType = reading.GetViolationType(ExpectedTemperatureRange, ExpectedHumidityRange);
        
        if (!string.IsNullOrEmpty(violationType))
        {
            AddDomainEvent(new MeasurementOutOfRangeEvent(PackageId, reading, violationType));
        }
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

    public override string ToString() => 
        $"PackageMeasurement {Id} for Package {PackageId} during {DeliveryLeg} " +
        $"({SessionStartTime:yyyy-MM-dd HH:mm} - {SessionEndTime?.ToString("yyyy-MM-dd HH:mm") ?? "Active"}) " +
        $"with {_readings.Count} readings";
}
