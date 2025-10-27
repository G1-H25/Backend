using GpsApp.Domain.ValueObjects;

namespace GpsApp.Tests.Unit.Domain;

public class MeasurementSummaryTests
{
    [Fact]
    public void Constructor_ValidParameters_ShouldCreateMeasurementSummary()
    {
        // Arrange
        var startTime = DateTime.UtcNow.AddHours(-1);
        var endTime = DateTime.UtcNow;
        var totalReadings = 10;
        var minTemp = new Temperature(20.0m);
        var maxTemp = new Temperature(30.0m);
        var avgTemp = new Temperature(25.0m);
        var minHumidity = new Humidity(50.0m);
        var maxHumidity = new Humidity(70.0m);
        var avgHumidity = new Humidity(60.0m);
        var outOfRangeCount = 2;
        var outOfRangeDuration = TimeSpan.FromMinutes(30);

        // Act
        var summary = new MeasurementSummary(
            startTime, endTime, totalReadings,
            minTemp, maxTemp, avgTemp,
            minHumidity, maxHumidity, avgHumidity,
            outOfRangeCount, outOfRangeDuration);

        // Assert
        Assert.Equal(startTime, summary.StartTime);
        Assert.Equal(endTime, summary.EndTime);
        Assert.Equal(totalReadings, summary.TotalReadings);
        Assert.Equal(minTemp, summary.MinTemperature);
        Assert.Equal(maxTemp, summary.MaxTemperature);
        Assert.Equal(avgTemp, summary.AverageTemperature);
        Assert.Equal(minHumidity, summary.MinHumidity);
        Assert.Equal(maxHumidity, summary.MaxHumidity);
        Assert.Equal(avgHumidity, summary.AverageHumidity);
        Assert.Equal(outOfRangeCount, summary.OutOfRangeCount);
        Assert.Equal(outOfRangeDuration, summary.TotalOutOfRangeDuration);
    }

    [Fact]
    public void Constructor_StartTimeAfterEndTime_ShouldThrowArgumentException()
    {
        // Arrange
        var startTime = DateTime.UtcNow;
        var endTime = DateTime.UtcNow.AddHours(-1);

        // Act & Assert
        Assert.Throws<ArgumentException>(() => new MeasurementSummary(
            startTime, endTime, 10,
            new Temperature(20.0m), new Temperature(30.0m), new Temperature(25.0m),
            new Humidity(50.0m), new Humidity(70.0m), new Humidity(60.0m),
            2, TimeSpan.FromMinutes(30)));
    }

    [Fact]
    public void FromReadings_EmptyReadings_ShouldThrowArgumentException()
    {
        // Arrange
        var emptyReadings = new List<MeasurementReading>();

        // Act & Assert
        Assert.Throws<ArgumentException>(() => 
            MeasurementSummary.FromReadings(emptyReadings));
    }

    [Fact]
    public void FromReadings_NullReadings_ShouldThrowArgumentException()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => 
            MeasurementSummary.FromReadings(null!));
    }

    [Fact]
    public void FromReadings_SingleReading_ShouldCalculateCorrectly()
    {
        // Arrange
        var timestamp = DateTime.UtcNow;
        var temperature = new Temperature(25.0m);
        var humidity = new Humidity(60.0m);
        var sensorId = SensorId.NewId();
    var reading = new MeasurementReading(sensorId, timestamp, temperature, humidity);
        var readings = new List<MeasurementReading> { reading };

        // Act
        var summary = MeasurementSummary.FromReadings(readings);

        // Assert
        Assert.Equal(timestamp, summary.StartTime);
        Assert.Equal(timestamp, summary.EndTime);
        Assert.Equal(1, summary.TotalReadings);
        Assert.Equal(temperature, summary.MinTemperature);
        Assert.Equal(temperature, summary.MaxTemperature);
        Assert.Equal(temperature, summary.AverageTemperature);
        Assert.Equal(humidity, summary.MinHumidity);
        Assert.Equal(humidity, summary.MaxHumidity);
        Assert.Equal(humidity, summary.AverageHumidity);
        Assert.Equal(0, summary.OutOfRangeCount);
        Assert.Equal(TimeSpan.Zero, summary.TotalOutOfRangeDuration);
    }

    [Fact]
    public void FromReadings_MultipleReadings_ShouldCalculateCorrectly()
    {
        // Arrange
        var baseTime = DateTime.UtcNow.AddHours(-1);
        var readings = new List<MeasurementReading>
        {
            new MeasurementReading(SensorId.NewId(), baseTime, new Temperature(20.0m), new Humidity(50.0m)),
            new MeasurementReading(SensorId.NewId(), baseTime.AddMinutes(15), new Temperature(25.0m), new Humidity(60.0m)),
            new MeasurementReading(SensorId.NewId(), baseTime.AddMinutes(30), new Temperature(30.0m), new Humidity(70.0m)),
            new MeasurementReading(SensorId.NewId(), baseTime.AddMinutes(45), new Temperature(22.0m), new Humidity(55.0m)),
            new MeasurementReading(SensorId.NewId(), baseTime.AddMinutes(60), new Temperature(28.0m), new Humidity(65.0m))
        };

        // Act
        var summary = MeasurementSummary.FromReadings(readings);

        // Assert
        Assert.Equal(baseTime, summary.StartTime);
        Assert.Equal(baseTime.AddMinutes(60), summary.EndTime);
        Assert.Equal(5, summary.TotalReadings);
        Assert.Equal(new Temperature(20.0m), summary.MinTemperature);
        Assert.Equal(new Temperature(30.0m), summary.MaxTemperature);
        Assert.Equal(new Temperature(25.0m), summary.AverageTemperature);
        Assert.Equal(new Humidity(50.0m), summary.MinHumidity);
        Assert.Equal(new Humidity(70.0m), summary.MaxHumidity);
        Assert.Equal(new Humidity(60.0m), summary.AverageHumidity);
    }

    [Fact]
    public void FromReadings_WithOutOfRangeReadings_ShouldCalculateOutOfRangeStats()
    {
        // Arrange
        var baseTime = DateTime.UtcNow.AddHours(-1);
        var tempRange = new ExpectedRange<Temperature>(new Temperature(20.0m), new Temperature(30.0m));
        var humidityRange = new ExpectedRange<Humidity>(new Humidity(50.0m), new Humidity(70.0m));
        
        var readings = new List<MeasurementReading>
        {
            new MeasurementReading(SensorId.NewId(), baseTime, new Temperature(25.0m), new Humidity(60.0m)), // In range
            new MeasurementReading(SensorId.NewId(), baseTime.AddMinutes(15), new Temperature(35.0m), new Humidity(60.0m)), // Temp out of range
            new MeasurementReading(SensorId.NewId(), baseTime.AddMinutes(30), new Temperature(25.0m), new Humidity(80.0m)), // Humidity out of range
            new MeasurementReading(SensorId.NewId(), baseTime.AddMinutes(45), new Temperature(25.0m), new Humidity(60.0m)) // In range
        };

        // Act
        var summary = MeasurementSummary.FromReadings(readings, tempRange, humidityRange);

        // Assert
        Assert.Equal(4, summary.TotalReadings);
        Assert.Equal(2, summary.OutOfRangeCount);
        Assert.Equal(50.0, summary.OutOfRangePercentage);
    }

    [Fact]
    public void Duration_ShouldReturnCorrectTimeSpan()
    {
        // Arrange
        var startTime = DateTime.UtcNow.AddHours(-2);
        var endTime = DateTime.UtcNow;
        var summary = new MeasurementSummary(
            startTime, endTime, 10,
            new Temperature(20.0m), new Temperature(30.0m), new Temperature(25.0m),
            new Humidity(50.0m), new Humidity(70.0m), new Humidity(60.0m),
            2, TimeSpan.FromMinutes(30));

        // Act
        var duration = summary.Duration;

        // Assert
        Assert.Equal(TimeSpan.FromHours(2), duration);
    }

    [Fact]
    public void OutOfRangePercentage_ShouldCalculateCorrectly()
    {
        // Arrange
        var summary = new MeasurementSummary(
            DateTime.UtcNow.AddHours(-1), DateTime.UtcNow, 10,
            new Temperature(20.0m), new Temperature(30.0m), new Temperature(25.0m),
            new Humidity(50.0m), new Humidity(70.0m), new Humidity(60.0m),
            3, TimeSpan.FromMinutes(30));

        // Act
        var percentage = summary.OutOfRangePercentage;

        // Assert
        Assert.Equal(30.0, percentage);
    }

    [Fact]
    public void OutOfRangeTimePercentage_ShouldCalculateCorrectly()
    {
        // Arrange
        var startTime = DateTime.UtcNow.AddHours(-1);
        var endTime = DateTime.UtcNow;
        var summary = new MeasurementSummary(
            startTime, endTime, 10,
            new Temperature(20.0m), new Temperature(30.0m), new Temperature(25.0m),
            new Humidity(50.0m), new Humidity(70.0m), new Humidity(60.0m),
            3, TimeSpan.FromMinutes(30)); // 30 minutes out of 60 minutes = 50%

        // Act
        var percentage = summary.OutOfRangeTimePercentage;

        // Assert
        Assert.Equal(50.0, percentage);
    }

    [Fact]
    public void Equals_SameValues_ShouldReturnTrue()
    {
        // Arrange
        var startTime = DateTime.UtcNow.AddHours(-1);
        var endTime = DateTime.UtcNow;
        var summary1 = new MeasurementSummary(
            startTime, endTime, 10,
            new Temperature(20.0m), new Temperature(30.0m), new Temperature(25.0m),
            new Humidity(50.0m), new Humidity(70.0m), new Humidity(60.0m),
            2, TimeSpan.FromMinutes(30));
        
        var summary2 = new MeasurementSummary(
            startTime, endTime, 10,
            new Temperature(20.0m), new Temperature(30.0m), new Temperature(25.0m),
            new Humidity(50.0m), new Humidity(70.0m), new Humidity(60.0m),
            2, TimeSpan.FromMinutes(30));

        // Act & Assert
        Assert.True(summary1.Equals(summary2));
        Assert.True(summary1 == summary2);
        Assert.False(summary1 != summary2);
        Assert.Equal(summary1.GetHashCode(), summary2.GetHashCode());
    }

    [Fact]
    public void Equals_DifferentValues_ShouldReturnFalse()
    {
        // Arrange
        var startTime = DateTime.UtcNow.AddHours(-1);
        var endTime = DateTime.UtcNow;
        var summary1 = new MeasurementSummary(
            startTime, endTime, 10,
            new Temperature(20.0m), new Temperature(30.0m), new Temperature(25.0m),
            new Humidity(50.0m), new Humidity(70.0m), new Humidity(60.0m),
            2, TimeSpan.FromMinutes(30));
        
        var summary2 = new MeasurementSummary(
            startTime, endTime, 15, // Different total readings
            new Temperature(20.0m), new Temperature(30.0m), new Temperature(25.0m),
            new Humidity(50.0m), new Humidity(70.0m), new Humidity(60.0m),
            2, TimeSpan.FromMinutes(30));

        // Act & Assert
        Assert.False(summary1.Equals(summary2));
        Assert.False(summary1 == summary2);
        Assert.True(summary1 != summary2);
    }

    [Fact]
    public void ToString_ShouldReturnFormattedString()
    {
        // Arrange
        var startTime = new DateTime(2024, 1, 15, 14, 0, 0);
        var endTime = new DateTime(2024, 1, 15, 15, 0, 0);
        var summary = new MeasurementSummary(
            startTime, endTime, 10,
            new Temperature(20.0m), new Temperature(30.0m), new Temperature(25.0m),
            new Humidity(50.0m), new Humidity(70.0m), new Humidity(60.0m),
            2, TimeSpan.FromMinutes(30));

        // Act
        var result = summary.ToString();

        // Assert
        Assert.Contains("14:00 - 15:00", result);
        Assert.Contains("10 readings", result);
        Assert.Contains("20.0°C - 30.0°C", result);
        Assert.Contains("50.0% - 70.0%", result);
        Assert.Contains("2 readings (20.0%)", result);
    }
}
