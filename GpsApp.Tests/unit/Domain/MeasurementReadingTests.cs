using GpsApp.Domain.ValueObjects;

namespace GpsApp.Tests.Unit.Domain;

public class MeasurementReadingTests
{
    [Fact]
    public void Constructor_ValidParameters_ShouldCreateMeasurementReading()
    {
        // Arrange
        var timestamp = DateTime.UtcNow;
        var temperature = new Temperature(25.5m);
        var humidity = new Humidity(60.0m);
        var sensorId = SensorId.NewId();

        // Act
        var reading = new MeasurementReading(timestamp, temperature, humidity, sensorId);

        // Assert
        Assert.Equal(timestamp, reading.Timestamp);
        Assert.Equal(temperature, reading.Temperature);
        Assert.Equal(humidity, reading.Humidity);
        Assert.Equal(sensorId, reading.SensorId);
    }

    [Fact]
    public void Constructor_NullTemperature_ShouldThrowArgumentNullException()
    {
        // Arrange
        var timestamp = DateTime.UtcNow;
        var humidity = new Humidity(60.0m);
        var sensorId = SensorId.NewId();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => 
            new MeasurementReading(timestamp, null!, humidity, sensorId));
    }

    [Fact]
    public void Constructor_NullHumidity_ShouldThrowArgumentNullException()
    {
        // Arrange
        var timestamp = DateTime.UtcNow;
        var temperature = new Temperature(25.5m);
        var sensorId = SensorId.NewId();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => 
            new MeasurementReading(timestamp, temperature, null!, sensorId));
    }

    [Fact]
    public void Constructor_NullSensorId_ShouldThrowArgumentNullException()
    {
        // Arrange
        var timestamp = DateTime.UtcNow;
        var temperature = new Temperature(25.5m);
        var humidity = new Humidity(60.0m);

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => 
            new MeasurementReading(timestamp, temperature, humidity, null!));
    }

    [Fact]
    public void IsTemperatureInRange_WithinRange_ShouldReturnTrue()
    {
        // Arrange
        var reading = CreateValidReading();
        var expectedRange = new ExpectedRange<Temperature>(
            new Temperature(20.0m), new Temperature(30.0m));

        // Act
        var result = reading.IsTemperatureInRange(expectedRange);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IsTemperatureInRange_OutsideRange_ShouldReturnFalse()
    {
        // Arrange
        var reading = new MeasurementReading(
            DateTime.UtcNow, 
            new Temperature(35.0m), // Outside range
            new Humidity(60.0m), 
            SensorId.NewId());
        var expectedRange = new ExpectedRange<Temperature>(
            new Temperature(20.0m), new Temperature(30.0m));

        // Act
        var result = reading.IsTemperatureInRange(expectedRange);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void IsTemperatureInRange_NullRange_ShouldReturnTrue()
    {
        // Arrange
        var reading = CreateValidReading();

        // Act
        var result = reading.IsTemperatureInRange(null);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IsHumidityInRange_WithinRange_ShouldReturnTrue()
    {
        // Arrange
        var reading = CreateValidReading();
        var expectedRange = new ExpectedRange<Humidity>(
            new Humidity(50.0m), new Humidity(70.0m));

        // Act
        var result = reading.IsHumidityInRange(expectedRange);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IsHumidityInRange_OutsideRange_ShouldReturnFalse()
    {
        // Arrange
        var reading = new MeasurementReading(
            DateTime.UtcNow, 
            new Temperature(25.0m), 
            new Humidity(80.0m), // Outside range
            SensorId.NewId());
        var expectedRange = new ExpectedRange<Humidity>(
            new Humidity(50.0m), new Humidity(70.0m));

        // Act
        var result = reading.IsHumidityInRange(expectedRange);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void IsOutOfRange_BothInRange_ShouldReturnFalse()
    {
        // Arrange
        var reading = CreateValidReading();
        var tempRange = new ExpectedRange<Temperature>(
            new Temperature(20.0m), new Temperature(30.0m));
        var humidityRange = new ExpectedRange<Humidity>(
            new Humidity(50.0m), new Humidity(70.0m));

        // Act
        var result = reading.IsOutOfRange(tempRange, humidityRange);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void IsOutOfRange_TemperatureOutOfRange_ShouldReturnTrue()
    {
        // Arrange
        var reading = new MeasurementReading(
            DateTime.UtcNow, 
            new Temperature(35.0m), // Outside range
            new Humidity(60.0m), 
            SensorId.NewId());
        var tempRange = new ExpectedRange<Temperature>(
            new Temperature(20.0m), new Temperature(30.0m));
        var humidityRange = new ExpectedRange<Humidity>(
            new Humidity(50.0m), new Humidity(70.0m));

        // Act
        var result = reading.IsOutOfRange(tempRange, humidityRange);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IsOutOfRange_HumidityOutOfRange_ShouldReturnTrue()
    {
        // Arrange
        var reading = new MeasurementReading(
            DateTime.UtcNow, 
            new Temperature(25.0m), 
            new Humidity(80.0m), // Outside range
            SensorId.NewId());
        var tempRange = new ExpectedRange<Temperature>(
            new Temperature(20.0m), new Temperature(30.0m));
        var humidityRange = new ExpectedRange<Humidity>(
            new Humidity(50.0m), new Humidity(70.0m));

        // Act
        var result = reading.IsOutOfRange(tempRange, humidityRange);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void GetViolationType_BothInRange_ShouldReturnNull()
    {
        // Arrange
        var reading = CreateValidReading();
        var tempRange = new ExpectedRange<Temperature>(
            new Temperature(20.0m), new Temperature(30.0m));
        var humidityRange = new ExpectedRange<Humidity>(
            new Humidity(50.0m), new Humidity(70.0m));

        // Act
        var result = reading.GetViolationType(tempRange, humidityRange);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void GetViolationType_TemperatureOutOfRange_ShouldReturnTemperature()
    {
        // Arrange
        var reading = new MeasurementReading(
            DateTime.UtcNow, 
            new Temperature(35.0m), // Outside range
            new Humidity(60.0m), 
            SensorId.NewId());
        var tempRange = new ExpectedRange<Temperature>(
            new Temperature(20.0m), new Temperature(30.0m));
        var humidityRange = new ExpectedRange<Humidity>(
            new Humidity(50.0m), new Humidity(70.0m));

        // Act
        var result = reading.GetViolationType(tempRange, humidityRange);

        // Assert
        Assert.Equal("Temperature", result);
    }

    [Fact]
    public void GetViolationType_BothOutOfRange_ShouldReturnBoth()
    {
        // Arrange
        var reading = new MeasurementReading(
            DateTime.UtcNow, 
            new Temperature(35.0m), // Outside range
            new Humidity(80.0m), // Outside range
            SensorId.NewId());
        var tempRange = new ExpectedRange<Temperature>(
            new Temperature(20.0m), new Temperature(30.0m));
        var humidityRange = new ExpectedRange<Humidity>(
            new Humidity(50.0m), new Humidity(70.0m));

        // Act
        var result = reading.GetViolationType(tempRange, humidityRange);

        // Assert
        Assert.Equal("Temperature, Humidity", result);
    }

    [Fact]
    public void Equals_SameValues_ShouldReturnTrue()
    {
        // Arrange
        var timestamp = DateTime.UtcNow;
        var temperature = new Temperature(25.5m);
        var humidity = new Humidity(60.0m);
        var sensorId = SensorId.NewId();
        
        var reading1 = new MeasurementReading(timestamp, temperature, humidity, sensorId);
        var reading2 = new MeasurementReading(timestamp, temperature, humidity, sensorId);

        // Act & Assert
        Assert.True(reading1.Equals(reading2));
        Assert.True(reading1 == reading2);
        Assert.False(reading1 != reading2);
        Assert.Equal(reading1.GetHashCode(), reading2.GetHashCode());
    }

    [Fact]
    public void Equals_DifferentValues_ShouldReturnFalse()
    {
        // Arrange
        var reading1 = CreateValidReading();
        var reading2 = CreateValidReading();

        // Act & Assert
        Assert.False(reading1.Equals(reading2));
        Assert.False(reading1 == reading2);
        Assert.True(reading1 != reading2);
    }

    [Fact]
    public void ToString_ShouldReturnFormattedString()
    {
        // Arrange
        var timestamp = new DateTime(2024, 1, 15, 14, 30, 0);
        var temperature = new Temperature(25.5m);
        var humidity = new Humidity(60.0m);
        var sensorId = SensorId.NewId();
        var reading = new MeasurementReading(timestamp, temperature, humidity, sensorId);

        // Act
        var result = reading.ToString();

        // Assert
        Assert.Contains("2024-01-15 14:30:00", result);
        Assert.Contains("25.5°C", result);
        Assert.Contains("60.0%", result);
        Assert.Contains(sensorId.ToString(), result);
    }

    private static MeasurementReading CreateValidReading()
    {
        return new MeasurementReading(
            DateTime.UtcNow, 
            new Temperature(25.0m), 
            new Humidity(60.0m), 
            SensorId.NewId());
    }
}
