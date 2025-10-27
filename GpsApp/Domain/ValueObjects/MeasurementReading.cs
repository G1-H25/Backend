namespace GpsApp.Domain.ValueObjects;

/// <summary>
/// Value object representing a single sensor measurement reading
/// Contains timestamp, temperature, humidity, and sensor identification
/// </summary>
public class MeasurementReading : IEquatable<MeasurementReading>
{
    public DateTime Timestamp { get; private set; }
    public Temperature Temperature { get; private set; }
    public Humidity Humidity { get; private set; }
    public SensorId SensorId { get; private set; }

    public MeasurementReading(DateTime timestamp, Temperature temperature, Humidity humidity, SensorId sensorId)
    {
        Timestamp = timestamp;
        Temperature = temperature ?? throw new ArgumentNullException(nameof(temperature));
        Humidity = humidity ?? throw new ArgumentNullException(nameof(humidity));
        SensorId = sensorId ?? throw new ArgumentNullException(nameof(sensorId));
    }

    /// <summary>
    /// Creates a measurement reading from persistence storage (hydration constructor)
    /// This constructor allows setting all properties for reconstruction from storage
    /// </summary>
    public MeasurementReading(DateTime timestamp, Temperature temperature, Humidity humidity, SensorId sensorId, bool skipValidation = false)
    {
        Timestamp = timestamp;
        Temperature = temperature ?? throw new ArgumentNullException(nameof(temperature));
        Humidity = humidity ?? throw new ArgumentNullException(nameof(humidity));
        SensorId = sensorId ?? throw new ArgumentNullException(nameof(sensorId));
    }

    /// <summary>
    /// Checks if this reading is within the expected temperature range
    /// </summary>
    public bool IsTemperatureInRange(ExpectedRange<Temperature>? expectedRange)
    {
        if (expectedRange == null)
            return true; // No range set, so any temperature is acceptable

        return expectedRange.IsInRange(Temperature);
    }

    /// <summary>
    /// Checks if this reading is within the expected humidity range
    /// </summary>
    public bool IsHumidityInRange(ExpectedRange<Humidity>? expectedRange)
    {
        if (expectedRange == null)
            return true; // No range set, so any humidity is acceptable

        return expectedRange.IsInRange(Humidity);
    }

    /// <summary>
    /// Checks if this reading violates any expected ranges
    /// </summary>
    public bool IsOutOfRange(ExpectedRange<Temperature>? expectedTemperatureRange, ExpectedRange<Humidity>? expectedHumidityRange)
    {
        return !IsTemperatureInRange(expectedTemperatureRange) || !IsHumidityInRange(expectedHumidityRange);
    }

    /// <summary>
    /// Gets the type of range violation (if any)
    /// </summary>
    public string? GetViolationType(ExpectedRange<Temperature>? expectedTemperatureRange, ExpectedRange<Humidity>? expectedHumidityRange)
    {
        var violations = new List<string>();
        
        if (!IsTemperatureInRange(expectedTemperatureRange))
            violations.Add("Temperature");
            
        if (!IsHumidityInRange(expectedHumidityRange))
            violations.Add("Humidity");
            
        return violations.Count > 0 ? string.Join(", ", violations) : null;
    }

    public override string ToString() => 
        $"[{Timestamp:yyyy-MM-dd HH:mm:ss}] Sensor {SensorId}: {Temperature}, {Humidity}";

    public override bool Equals(object? obj) => obj is MeasurementReading reading && Equals(reading);

    public override int GetHashCode() => HashCode.Combine(Timestamp, Temperature, Humidity, SensorId);

    public bool Equals(MeasurementReading? other) => other is not null &&
        Timestamp == other.Timestamp &&
        Temperature.Equals(other.Temperature) &&
        Humidity.Equals(other.Humidity) &&
        SensorId.Equals(other.SensorId);

    public static bool operator ==(MeasurementReading? left, MeasurementReading? right) => 
        left?.Equals(right) ?? right is null;

    public static bool operator !=(MeasurementReading? left, MeasurementReading? right) => 
        !(left == right);
}
