using GpsApp.Domain.Aggregates;
using GpsApp.Domain.ValueObjects;
using GpsApp.Domain.Events;

namespace GpsApp.Tests.Unit.Domain;

public class PackageMeasurementTests
{
    [Fact]
    public void Constructor_ValidParameters_ShouldCreatePackageMeasurement()
    {
        // Arrange
        var id = PackageMeasurementId.NewId();
        var packageId = PackageId.NewId();
        var deliveryLeg = CreateValidDeliveryLeg();
        var shipmentId = ShipmentId.NewId();
        var tempRange = new ExpectedRange<Temperature>(new Temperature(20.0m), new Temperature(30.0m));
        var humidityRange = new ExpectedRange<Humidity>(new Humidity(50.0m), new Humidity(70.0m));

        // Act
        var measurement = new PackageMeasurement(id, packageId, deliveryLeg, shipmentId, tempRange, humidityRange);

        // Assert
        Assert.Equal(id, measurement.Id);
        Assert.Equal(packageId, measurement.PackageId);
        Assert.Equal(deliveryLeg, measurement.DeliveryLeg);
        Assert.Equal(tempRange, measurement.ExpectedTemperatureRange);
        Assert.Equal(humidityRange, measurement.ExpectedHumidityRange);
        Assert.True(measurement.IsActive);
        Assert.Null(measurement.SessionEndTime);
        Assert.Equal(0, measurement.ReadingCount);
        Assert.Single(measurement.DomainEvents);
        Assert.IsType<MeasurementSessionStartedEvent>(measurement.DomainEvents.First());
    }

    [Fact]
    public void Constructor_NullId_ShouldThrowArgumentNullException()
    {
        // Arrange
        var packageId = PackageId.NewId();
        var deliveryLeg = CreateValidDeliveryLeg();
        var shipmentId = ShipmentId.NewId();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => 
            new PackageMeasurement(null!, packageId, deliveryLeg, shipmentId));
    }

    [Fact]
    public void Constructor_NullPackageId_ShouldThrowArgumentNullException()
    {
        // Arrange
        var id = PackageMeasurementId.NewId();
        var deliveryLeg = CreateValidDeliveryLeg();
        var shipmentId = ShipmentId.NewId();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => 
            new PackageMeasurement(id, null!, deliveryLeg, shipmentId));
    }

    [Fact]
    public void Constructor_NullDeliveryLeg_ShouldThrowArgumentNullException()
    {
        // Arrange
        var id = PackageMeasurementId.NewId();
        var packageId = PackageId.NewId();
        var shipmentId = ShipmentId.NewId();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => 
            new PackageMeasurement(id, packageId, null!, shipmentId));
    }

    [Fact]
    public void RecordMeasurementBatch_ValidReadings_ShouldRecordReadings()
    {
        // Arrange
        var measurement = CreateValidPackageMeasurement();
        var readings = CreateValidReadings(3);

        // Act
        measurement.RecordMeasurementBatch(readings);

        // Assert
        Assert.Equal(3, measurement.ReadingCount);
        Assert.Equal(readings, measurement.Readings);
        Assert.Equal(2, measurement.DomainEvents.Count); // Session started + batch recorded
        Assert.IsType<MeasurementBatchRecordedEvent>(measurement.DomainEvents.Last());
    }

    [Fact]
    public void RecordMeasurementBatch_EmptyReadings_ShouldThrowArgumentException()
    {
        // Arrange
        var measurement = CreateValidPackageMeasurement();
        var emptyReadings = new List<MeasurementReading>();

        // Act & Assert
        Assert.Throws<ArgumentException>(() => measurement.RecordMeasurementBatch(emptyReadings));
    }

    [Fact]
    public void RecordMeasurementBatch_NullReadings_ShouldThrowArgumentException()
    {
        // Arrange
        var measurement = CreateValidPackageMeasurement();

        // Act & Assert
        Assert.Throws<ArgumentException>(() => measurement.RecordMeasurementBatch(null!));
    }

    [Fact]
    public void RecordMeasurementBatch_CompletedSession_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var measurement = CreateValidPackageMeasurement();
        measurement.CompleteSession();
        var readings = CreateValidReadings(1);

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => measurement.RecordMeasurementBatch(readings));
    }

    [Fact]
    public void RecordMeasurement_SingleReading_ShouldRecordReading()
    {
        // Arrange
        var measurement = CreateValidPackageMeasurement();
        var reading = CreateValidReading();

        // Act
        measurement.RecordMeasurement(reading);

        // Assert
        Assert.Equal(1, measurement.ReadingCount);
        Assert.Contains(reading, measurement.Readings);
        Assert.Equal(2, measurement.DomainEvents.Count); // Session started + batch recorded
        Assert.IsType<MeasurementBatchRecordedEvent>(measurement.DomainEvents.Last());
    }

    [Fact]
    public void RecordMeasurement_NullReading_ShouldThrowArgumentNullException()
    {
        // Arrange
        var measurement = CreateValidPackageMeasurement();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => measurement.RecordMeasurement(null!));
    }

    [Fact]
    public void RecordMeasurement_OutOfRangeReading_ShouldRaiseOutOfRangeEvent()
    {
        // Arrange
        var measurement = CreateValidPackageMeasurement();
        var outOfRangeReading = new MeasurementReading(
            SensorId.NewId(),
            DateTime.UtcNow, 
            new Temperature(35.0m), // Outside expected range (20-30)
            new Humidity(60.0m));

        // Act
        measurement.RecordMeasurement(outOfRangeReading);

        // Assert
        Assert.Equal(3, measurement.DomainEvents.Count); // Session started + batch recorded + out of range
        Assert.IsType<MeasurementOutOfRangeEvent>(measurement.DomainEvents.Last());
    }

    [Fact]
    public void CompleteSession_ActiveSession_ShouldCompleteSession()
    {
        // Arrange
        var measurement = CreateValidPackageMeasurement();
        measurement.RecordMeasurementBatch(CreateValidReadings(2));

        // Act
        measurement.CompleteSession();

        // Assert
        Assert.False(measurement.IsActive);
        Assert.NotNull(measurement.SessionEndTime);
        Assert.NotNull(measurement.Duration);
        Assert.Equal(3, measurement.DomainEvents.Count); // Session started + batch recorded + completed
        Assert.IsType<PackageMeasurementCompletedEvent>(measurement.DomainEvents.Last());
    }

    [Fact]
    public void CompleteSession_AlreadyCompleted_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var measurement = CreateValidPackageMeasurement();
        measurement.CompleteSession();

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => measurement.CompleteSession());
    }

    [Fact]
    public void GetReadingsForPeriod_ValidPeriod_ShouldReturnFilteredReadings()
    {
        // Arrange
        var measurement = CreateValidPackageMeasurement();
        var baseTime = DateTime.UtcNow.AddHours(-1);
        var readings = new List<MeasurementReading>
        {
            new MeasurementReading(SensorId.NewId(), baseTime, new Temperature(20.0m), new Humidity(50.0m)),
            new MeasurementReading(SensorId.NewId(), baseTime.AddMinutes(15), new Temperature(25.0m), new Humidity(60.0m)),
            new MeasurementReading(SensorId.NewId(), baseTime.AddMinutes(30), new Temperature(30.0m), new Humidity(70.0m)),
            new MeasurementReading(SensorId.NewId(), baseTime.AddMinutes(45), new Temperature(22.0m), new Humidity(55.0m))
        };
        measurement.RecordMeasurementBatch(readings);

        // Act
        var filteredReadings = measurement.GetReadingsForPeriod(
            baseTime.AddMinutes(10), 
            baseTime.AddMinutes(40));

        // Assert
        Assert.Equal(2, filteredReadings.Count);
        Assert.Equal(readings[1], filteredReadings[0]);
        Assert.Equal(readings[2], filteredReadings[1]);
    }

    [Fact]
    public void GetReadingsAtInterval_ValidInterval_ShouldReturnSampledReadings()
    {
        // Arrange
        var measurement = CreateValidPackageMeasurement();
        var baseTime = DateTime.UtcNow.AddHours(-1);
        var readings = new List<MeasurementReading>
        {
            new MeasurementReading(SensorId.NewId(), baseTime, new Temperature(20.0m), new Humidity(50.0m)),
            new MeasurementReading(SensorId.NewId(), baseTime.AddMinutes(10), new Temperature(25.0m), new Humidity(60.0m)),
            new MeasurementReading(SensorId.NewId(), baseTime.AddMinutes(20), new Temperature(30.0m), new Humidity(70.0m)),
            new MeasurementReading(SensorId.NewId(), baseTime.AddMinutes(30), new Temperature(22.0m), new Humidity(55.0m))
        };
        measurement.RecordMeasurementBatch(readings);

        // Act
        var sampledReadings = measurement.GetReadingsAtInterval(TimeSpan.FromMinutes(15));

        // Assert
        Assert.Equal(3, sampledReadings.Count); // At 0, 15, 30 minutes
        Assert.Equal(readings[0], sampledReadings[0]);
        Assert.Equal(readings[2], sampledReadings[1]);
        Assert.Equal(readings[3], sampledReadings[2]);
    }

    [Fact]
    public void GetSummary_WithReadings_ShouldReturnCorrectSummary()
    {
        // Arrange
        var measurement = CreateValidPackageMeasurement();
        var readings = CreateValidReadings(5);
        measurement.RecordMeasurementBatch(readings);

        // Act
        var summary = measurement.GetSummary();

        // Assert
        Assert.Equal(5, summary.TotalReadings);
        Assert.NotNull(summary.MinTemperature);
        Assert.NotNull(summary.MaxTemperature);
        Assert.NotNull(summary.AverageTemperature);
        Assert.NotNull(summary.MinHumidity);
        Assert.NotNull(summary.MaxHumidity);
        Assert.NotNull(summary.AverageHumidity);
    }

    [Fact]
    public void GetSummaryForPeriod_ValidPeriod_ShouldReturnPeriodSummary()
    {
        // Arrange
        var measurement = CreateValidPackageMeasurement();
        var baseTime = DateTime.UtcNow.AddHours(-1);
        var readings = new List<MeasurementReading>
        {
            new MeasurementReading(SensorId.NewId(), baseTime, new Temperature(20.0m), new Humidity(50.0m)),
            new MeasurementReading(SensorId.NewId(), baseTime.AddMinutes(15), new Temperature(25.0m), new Humidity(60.0m)),
            new MeasurementReading(SensorId.NewId(), baseTime.AddMinutes(30), new Temperature(30.0m), new Humidity(70.0m))
        };
        measurement.RecordMeasurementBatch(readings);

        // Act
        var summary = measurement.GetSummaryForPeriod(
            baseTime.AddMinutes(5), 
            baseTime.AddMinutes(25));

        // Assert
        Assert.Equal(1, summary.TotalReadings);
        Assert.Equal(new Temperature(25.0m), summary.MinTemperature);
        Assert.Equal(new Temperature(25.0m), summary.MaxTemperature);
    }

    [Fact]
    public void ClearDomainEvents_ShouldClearAllEvents()
    {
        // Arrange
        var measurement = CreateValidPackageMeasurement();
        measurement.RecordMeasurement(CreateValidReading());
        Assert.Equal(2, measurement.DomainEvents.Count);

        // Act
        measurement.ClearDomainEvents();

        // Assert
        Assert.Empty(measurement.DomainEvents);
    }

    [Fact]
    public void ToString_ShouldReturnFormattedString()
    {
        // Arrange
        var measurement = CreateValidPackageMeasurement();
        measurement.RecordMeasurementBatch(CreateValidReadings(3));

        // Act
        var result = measurement.ToString();

        // Assert
        Assert.Contains("PackageMeasurement", result);
        Assert.Contains(measurement.Id.ToString(), result);
        Assert.Contains(measurement.PackageId.ToString(), result);
        Assert.Contains("3 readings", result);
    }

    private static PackageMeasurement CreateValidPackageMeasurement()
    {
        var id = PackageMeasurementId.NewId();
        var packageId = PackageId.NewId();
        var deliveryLeg = CreateValidDeliveryLeg();
        var shipmentId = ShipmentId.NewId();
        var tempRange = new ExpectedRange<Temperature>(new Temperature(20.0m), new Temperature(30.0m));
        var humidityRange = new ExpectedRange<Humidity>(new Humidity(50.0m), new Humidity(70.0m));
        
        return new PackageMeasurement(id, packageId, deliveryLeg, shipmentId, tempRange, humidityRange);
    }

    private static DeliveryLeg CreateValidDeliveryLeg()
    {
        var startAddress = new Address("Start Street 1", "Start City", "12345", "Country");
        var endAddress = new Address("End Street 1", "End City", "67890", "Country");
        var gatewayId = GatewayId.NewId();
        
        var leg = new DeliveryLeg(startAddress, endAddress);
        return leg.AssignGateway(gatewayId).Start();
    }

    private static MeasurementReading CreateValidReading()
    {
        return new MeasurementReading(
            SensorId.NewId(),
            DateTime.UtcNow, 
            new Temperature(25.0m), 
            new Humidity(60.0m));
    }

    private static IReadOnlyList<MeasurementReading> CreateValidReadings(int count)
    {
        var readings = new List<MeasurementReading>();
        var baseTime = DateTime.UtcNow.AddHours(-1);
        
        for (int i = 0; i < count; i++)
        {
            readings.Add(new MeasurementReading(
                SensorId.NewId(),
                baseTime.AddMinutes(i * 10), 
                new Temperature(20.0m + i * 2), 
                new Humidity(50.0m + i * 5)));
        }
        
        return readings;
    }
}
