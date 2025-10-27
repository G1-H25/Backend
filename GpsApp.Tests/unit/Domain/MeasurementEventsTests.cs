using GpsApp.Domain.Events;
using GpsApp.Domain.ValueObjects;

namespace GpsApp.Tests.Unit.Domain;

public class MeasurementEventsTests
{
    [Fact]
    public void MeasurementSessionStartedEvent_Constructor_ShouldCreateEvent()
    {
        // Arrange
        var shipmentId = ShipmentId.NewId();
        var deliveryLeg = CreateValidDeliveryLeg();
        var packageIds = new List<PackageId> { PackageId.NewId(), PackageId.NewId() };
        var gatewayId = GatewayId.NewId();

        // Act
        var domainEvent = new MeasurementSessionStartedEvent(shipmentId, deliveryLeg, packageIds, gatewayId);

        // Assert
        Assert.Equal(shipmentId, domainEvent.ShipmentId);
        Assert.Equal(deliveryLeg, domainEvent.DeliveryLeg);
        Assert.Equal(packageIds, domainEvent.PackageIds);
        Assert.Equal(gatewayId, domainEvent.GatewayId);
        Assert.Equal(shipmentId.Value.ToString(), domainEvent.AggregateId);
        Assert.Equal("Shipment", domainEvent.AggregateType);
        Assert.NotEqual(Guid.Empty, domainEvent.EventId);
        Assert.True(domainEvent.OccurredAt <= DateTime.UtcNow);
    }

    [Fact]
    public void SensorPresenceVerifiedEvent_Constructor_ShouldCreateEvent()
    {
        // Arrange
        var shipmentId = ShipmentId.NewId();
        var deliveryLeg = CreateValidDeliveryLeg();
        var presentSensors = new List<SensorId> { SensorId.NewId(), SensorId.NewId() };
        var missingSensors = new List<SensorId> { SensorId.NewId() };

        // Act
        var domainEvent = new SensorPresenceVerifiedEvent(shipmentId, deliveryLeg, presentSensors, missingSensors);

        // Assert
        Assert.Equal(shipmentId, domainEvent.ShipmentId);
        Assert.Equal(deliveryLeg, domainEvent.DeliveryLeg);
        Assert.Equal(presentSensors, domainEvent.PresentSensors);
        Assert.Equal(missingSensors, domainEvent.MissingSensors);
        Assert.Equal(shipmentId.Value.ToString(), domainEvent.AggregateId);
        Assert.Equal("Shipment", domainEvent.AggregateType);
    }

    [Fact]
    public void MeasurementBatchReceivedEvent_Constructor_ShouldCreateEvent()
    {
        // Arrange
        var shipmentId = ShipmentId.NewId();
        var deliveryLeg = CreateValidDeliveryLeg();
        var readings = CreateValidReadings(3);

        // Act
        var domainEvent = new MeasurementBatchReceivedEvent(shipmentId, deliveryLeg, readings);

        // Assert
        Assert.Equal(shipmentId, domainEvent.ShipmentId);
        Assert.Equal(deliveryLeg, domainEvent.DeliveryLeg);
        Assert.Equal(readings, domainEvent.Readings);
        Assert.Equal(shipmentId.Value.ToString(), domainEvent.AggregateId);
        Assert.Equal("Shipment", domainEvent.AggregateType);
    }

    [Fact]
    public void MeasurementOutOfRangeEvent_Constructor_ShouldCreateEvent()
    {
        // Arrange
        var packageId = PackageId.NewId();
        var reading = CreateValidReading();
        var violationType = "Temperature";

        // Act
        var domainEvent = new MeasurementOutOfRangeEvent(packageId, reading, violationType);

        // Assert
        Assert.Equal(packageId, domainEvent.PackageId);
        Assert.Equal(reading, domainEvent.Reading);
        Assert.Equal(violationType, domainEvent.ViolationType);
        Assert.Equal(packageId.Value.ToString(), domainEvent.AggregateId);
        Assert.Equal("Package", domainEvent.AggregateType);
    }

    [Fact]
    public void MeasurementSessionCompletedEvent_Constructor_ShouldCreateEvent()
    {
        // Arrange
        var shipmentId = ShipmentId.NewId();
        var deliveryLeg = CreateValidDeliveryLeg();
        var summary = CreateValidMeasurementSummary();

        // Act
        var domainEvent = new MeasurementSessionCompletedEvent(shipmentId, deliveryLeg, summary);

        // Assert
        Assert.Equal(shipmentId, domainEvent.ShipmentId);
        Assert.Equal(deliveryLeg, domainEvent.DeliveryLeg);
        Assert.Equal(summary, domainEvent.Summary);
        Assert.Equal(shipmentId.Value.ToString(), domainEvent.AggregateId);
        Assert.Equal("Shipment", domainEvent.AggregateType);
    }

    [Fact]
    public void MeasurementBatchRecordedEvent_Constructor_ShouldCreateEvent()
    {
        // Arrange
        var packageMeasurementId = PackageMeasurementId.NewId();
        var readings = CreateValidReadings(2);

        // Act
        var domainEvent = new MeasurementBatchRecordedEvent(packageMeasurementId, readings);

        // Assert
        Assert.Equal(packageMeasurementId, domainEvent.PackageMeasurementId);
        Assert.Equal(readings, domainEvent.Readings);
        Assert.Equal(packageMeasurementId.Value.ToString(), domainEvent.AggregateId);
        Assert.Equal("PackageMeasurement", domainEvent.AggregateType);
    }

    [Fact]
    public void PackageMeasurementCompletedEvent_Constructor_ShouldCreateEvent()
    {
        // Arrange
        var packageMeasurementId = PackageMeasurementId.NewId();
        var packageId = PackageId.NewId();
        var summary = CreateValidMeasurementSummary();

        // Act
        var domainEvent = new PackageMeasurementCompletedEvent(packageMeasurementId, packageId, summary);

        // Assert
        Assert.Equal(packageMeasurementId, domainEvent.PackageMeasurementId);
        Assert.Equal(packageId, domainEvent.PackageId);
        Assert.Equal(summary, domainEvent.Summary);
        Assert.Equal(packageMeasurementId.Value.ToString(), domainEvent.AggregateId);
        Assert.Equal("PackageMeasurement", domainEvent.AggregateType);
    }

    [Fact]
    public void DomainEvent_BaseProperties_ShouldBeSetCorrectly()
    {
        // Arrange
        var shipmentId = ShipmentId.NewId();
        var deliveryLeg = CreateValidDeliveryLeg();
        var packageIds = new List<PackageId> { PackageId.NewId() };
        var gatewayId = GatewayId.NewId();

        // Act
        var domainEvent = new MeasurementSessionStartedEvent(shipmentId, deliveryLeg, packageIds, gatewayId);

        // Assert
        Assert.NotEqual(Guid.Empty, domainEvent.EventId);
        Assert.True(domainEvent.OccurredAt <= DateTime.UtcNow);
        Assert.True(domainEvent.OccurredAt >= DateTime.UtcNow.AddSeconds(-1));
        Assert.Equal(1, domainEvent.Version);
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
            DateTime.UtcNow, 
            new Temperature(25.0m), 
            new Humidity(60.0m), 
            SensorId.NewId());
    }

    private static IReadOnlyList<MeasurementReading> CreateValidReadings(int count)
    {
        var readings = new List<MeasurementReading>();
        var baseTime = DateTime.UtcNow.AddHours(-1);
        
        for (int i = 0; i < count; i++)
        {
            readings.Add(new MeasurementReading(
                baseTime.AddMinutes(i * 10), 
                new Temperature(20.0m + i * 2), 
                new Humidity(50.0m + i * 5), 
                SensorId.NewId()));
        }
        
        return readings;
    }

    private static MeasurementSummary CreateValidMeasurementSummary()
    {
        var startTime = DateTime.UtcNow.AddHours(-1);
        var endTime = DateTime.UtcNow;
        
        return new MeasurementSummary(
            startTime, endTime, 10,
            new Temperature(20.0m), new Temperature(30.0m), new Temperature(25.0m),
            new Humidity(50.0m), new Humidity(70.0m), new Humidity(60.0m),
            2, TimeSpan.FromMinutes(30));
    }
}
