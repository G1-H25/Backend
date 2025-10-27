namespace GpsApp.Domain.ValueObjects;

/// <summary>
/// Value object representing a summary of measurements for a specific period
/// Contains aggregated statistics for temperature and humidity readings
/// </summary>
public class MeasurementSummary : IEquatable<MeasurementSummary>
{
    public DateTime StartTime { get; private set; }
    public DateTime EndTime { get; private set; }
    public int TotalReadings { get; private set; }
    public Temperature? MinTemperature { get; private set; }
    public Temperature? MaxTemperature { get; private set; }
    public Temperature? AverageTemperature { get; private set; }
    public Humidity? MinHumidity { get; private set; }
    public Humidity? MaxHumidity { get; private set; }
    public Humidity? AverageHumidity { get; private set; }
    public int OutOfRangeCount { get; private set; }
    public TimeSpan TotalOutOfRangeDuration { get; private set; }

    public MeasurementSummary(DateTime startTime, DateTime endTime, int totalReadings,
        Temperature? minTemperature, Temperature? maxTemperature, Temperature? averageTemperature,
        Humidity? minHumidity, Humidity? maxHumidity, Humidity? averageHumidity,
        int outOfRangeCount, TimeSpan totalOutOfRangeDuration)
    {
        if (startTime >= endTime)
            throw new ArgumentException("Start time must be before end time");

        StartTime = startTime;
        EndTime = endTime;
        TotalReadings = totalReadings;
        MinTemperature = minTemperature;
        MaxTemperature = maxTemperature;
        AverageTemperature = averageTemperature;
        MinHumidity = minHumidity;
        MaxHumidity = maxHumidity;
        AverageHumidity = averageHumidity;
        OutOfRangeCount = outOfRangeCount;
        TotalOutOfRangeDuration = totalOutOfRangeDuration;
    }

    /// <summary>
    /// Creates a measurement summary from a collection of readings
    /// </summary>
    public static MeasurementSummary FromReadings(IReadOnlyList<MeasurementReading> readings,
        ExpectedRange<Temperature>? expectedTemperatureRange = null,
        ExpectedRange<Humidity>? expectedHumidityRange = null)
    {
        if (readings == null || readings.Count == 0)
            throw new ArgumentException("Cannot create summary from empty readings", nameof(readings));

        var orderedReadings = readings.OrderBy(r => r.Timestamp).ToList();
        var startTime = orderedReadings.First().Timestamp;
        var endTime = orderedReadings.Last().Timestamp;

        // Calculate temperature statistics
        var temperatures = orderedReadings.Select(r => r.Temperature).ToList();
        var minTemp = temperatures.Min();
        var maxTemp = temperatures.Max();
        var avgTemp = new Temperature(temperatures.Average(t => t.Value));

        // Calculate humidity statistics
        var humidities = orderedReadings.Select(r => r.Humidity).ToList();
        var minHumidity = humidities.Min();
        var maxHumidity = humidities.Max();
        var avgHumidity = new Humidity(humidities.Average(h => h.Value));

        // Count out-of-range readings
        var outOfRangeCount = orderedReadings.Count(r => 
            r.IsOutOfRange(expectedTemperatureRange, expectedHumidityRange));

        // Calculate total out-of-range duration
        var totalOutOfRangeDuration = CalculateOutOfRangeDuration(
            orderedReadings, expectedTemperatureRange, expectedHumidityRange);

        return new MeasurementSummary(
            startTime, endTime, readings.Count,
            minTemp, maxTemp, avgTemp,
            minHumidity, maxHumidity, avgHumidity,
            outOfRangeCount, totalOutOfRangeDuration);
    }

    /// <summary>
    /// Calculates the total duration that readings were out of expected ranges
    /// </summary>
    private static TimeSpan CalculateOutOfRangeDuration(
        IReadOnlyList<MeasurementReading> orderedReadings,
        ExpectedRange<Temperature>? expectedTemperatureRange,
        ExpectedRange<Humidity>? expectedHumidityRange)
    {
        if (orderedReadings.Count < 2)
            return TimeSpan.Zero;

        var totalDuration = TimeSpan.Zero;
        var outOfRangeStart = (DateTime?)null;

        for (int i = 0; i < orderedReadings.Count; i++)
        {
            var reading = orderedReadings[i];
            var isOutOfRange = reading.IsOutOfRange(expectedTemperatureRange, expectedHumidityRange);

            if (isOutOfRange && outOfRangeStart == null)
            {
                // Start of out-of-range period
                outOfRangeStart = reading.Timestamp;
            }
            else if (!isOutOfRange && outOfRangeStart != null)
            {
                // End of out-of-range period
                totalDuration += reading.Timestamp - outOfRangeStart.Value;
                outOfRangeStart = null;
            }
        }

        // If we're still in an out-of-range period at the end
        if (outOfRangeStart != null)
        {
            totalDuration += orderedReadings.Last().Timestamp - outOfRangeStart.Value;
        }

        return totalDuration;
    }

    /// <summary>
    /// Gets the duration of the measurement period
    /// </summary>
    public TimeSpan Duration => EndTime - StartTime;

    /// <summary>
    /// Gets the percentage of readings that were out of range
    /// </summary>
    public double OutOfRangePercentage => TotalReadings > 0 ? (double)OutOfRangeCount / TotalReadings * 100 : 0;

    /// <summary>
    /// Gets the percentage of time that readings were out of range
    /// </summary>
    public double OutOfRangeTimePercentage => Duration.TotalSeconds > 0 ? 
        TotalOutOfRangeDuration.TotalSeconds / Duration.TotalSeconds * 100 : 0;

    public override string ToString() => 
        $"Summary [{StartTime:HH:mm} - {EndTime:HH:mm}]: {TotalReadings} readings, " +
        $"Temp: {MinTemperature?.Value:F1}°C - {MaxTemperature?.Value:F1}°C, " +
        $"Humidity: {MinHumidity?.Value:F1}% - {MaxHumidity?.Value:F1}%, " +
        $"Out of range: {OutOfRangeCount} readings ({OutOfRangePercentage:F1}%)";

    public override bool Equals(object? obj) => obj is MeasurementSummary summary && Equals(summary);

    public override int GetHashCode()
    {
        var hash = new HashCode();
        hash.Add(StartTime);
        hash.Add(EndTime);
        hash.Add(TotalReadings);
        hash.Add(MinTemperature);
        hash.Add(MaxTemperature);
        hash.Add(AverageTemperature);
        hash.Add(MinHumidity);
        hash.Add(MaxHumidity);
        hash.Add(AverageHumidity);
        hash.Add(OutOfRangeCount);
        hash.Add(TotalOutOfRangeDuration);
        return hash.ToHashCode();
    }

    public bool Equals(MeasurementSummary? other) => other is not null &&
        StartTime == other.StartTime &&
        EndTime == other.EndTime &&
        TotalReadings == other.TotalReadings &&
        MinTemperature?.Equals(other.MinTemperature) == true &&
        MaxTemperature?.Equals(other.MaxTemperature) == true &&
        AverageTemperature?.Equals(other.AverageTemperature) == true &&
        MinHumidity?.Equals(other.MinHumidity) == true &&
        MaxHumidity?.Equals(other.MaxHumidity) == true &&
        AverageHumidity?.Equals(other.AverageHumidity) == true &&
        OutOfRangeCount == other.OutOfRangeCount &&
        TotalOutOfRangeDuration == other.TotalOutOfRangeDuration;

    public static bool operator ==(MeasurementSummary? left, MeasurementSummary? right) => 
        left?.Equals(right) ?? right is null;

    public static bool operator !=(MeasurementSummary? left, MeasurementSummary? right) => 
        !(left == right);
}
