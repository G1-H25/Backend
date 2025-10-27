using GpsApp.Domain.Aggregates;
using GpsApp.Domain.ValueObjects;
using GpsApp.Domain.Events;

namespace GpsApp.Tests.Unit.Domain;

public class ShipmentTests
{
    [Fact]
    public void Constructor_ValidParameters_ShouldCreateShipment()
    {
        // Arrange
        var shipmentId = ShipmentId.NewId();
        var shipmentDate = DateTime.UtcNow;
        var packages = CreateValidPackages();
        var deliveryLegs = CreateConnectedDeliveryLegs();

        // Act
        var shipment = new Shipment(shipmentId, shipmentDate, packages, deliveryLegs);

        // Assert
        Assert.Equal(shipmentId, shipment.ShipmentId);
        Assert.Equal(shipmentDate, shipment.ShipmentDate);
        Assert.Equal(Shipment.ShipmentStatus.Pending, shipment.Status);
        Assert.Equal(2, shipment.Packages.Count);
        Assert.Equal(2, shipment.DeliveryLegs.Count);
        Assert.True(shipment.IsValid);
        Assert.True(shipment.HasPackages);
        Assert.True(shipment.HasValidDeliveryRoute);
        Assert.Single(shipment.DomainEvents);
        Assert.IsType<ShipmentCreatedEvent>(shipment.DomainEvents.First());
    }

    [Fact]
    public void Constructor_NullShipmentId_ShouldThrowArgumentNullException()
    {
        // Arrange
        var packages = CreateValidPackages();
        var deliveryLegs = CreateConnectedDeliveryLegs();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new Shipment(null!, DateTime.UtcNow, packages, deliveryLegs));
    }

    [Fact]
    public void Constructor_NoPackages_ShouldThrowArgumentException()
    {
        // Arrange
        var shipmentId = ShipmentId.NewId();
        var deliveryLegs = CreateConnectedDeliveryLegs();

        // Act & Assert
        Assert.Throws<ArgumentException>(() => new Shipment(shipmentId, DateTime.UtcNow, new List<Package>(), deliveryLegs));
    }

    [Fact]
    public void Constructor_NullPackages_ShouldThrowArgumentException()
    {
        // Arrange
        var shipmentId = ShipmentId.NewId();
        var deliveryLegs = CreateConnectedDeliveryLegs();

        // Act & Assert
        Assert.Throws<ArgumentException>(() => new Shipment(shipmentId, DateTime.UtcNow, null!, deliveryLegs));
    }

    [Fact]
    public void Constructor_NoDeliveryLegs_ShouldThrowArgumentException()
    {
        // Arrange
        var shipmentId = ShipmentId.NewId();
        var packages = CreateValidPackages();

        // Act & Assert
        Assert.Throws<ArgumentException>(() => new Shipment(shipmentId, DateTime.UtcNow, packages, new List<DeliveryLeg>()));
    }

    [Fact]
    public void Constructor_NullDeliveryLegs_ShouldThrowArgumentException()
    {
        // Arrange
        var shipmentId = ShipmentId.NewId();
        var packages = CreateValidPackages();

        // Act & Assert
        Assert.Throws<ArgumentException>(() => new Shipment(shipmentId, DateTime.UtcNow, packages, null!));
    }

    [Fact]
    public void Constructor_DisconnectedDeliveryLegs_ShouldThrowArgumentException()
    {
        // Arrange
        var shipmentId = ShipmentId.NewId();
        var packages = CreateValidPackages();
        var disconnectedLegs = CreateDisconnectedDeliveryLegs();

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => new Shipment(shipmentId, DateTime.UtcNow, packages, disconnectedLegs));
        Assert.Contains("does not connect", exception.Message);
    }

    [Fact]
    public void AddPackage_ValidPackage_ShouldAddPackage()
    {
        // Arrange
        var shipment = CreateValidShipment();
        var newPackage = CreateValidPackage();

        // Act
        shipment.AddPackage(newPackage);

        // Assert
        Assert.Equal(3, shipment.Packages.Count);
        Assert.Contains(newPackage, shipment.Packages);
        Assert.Contains(shipment.DomainEvents, e => e is PackageAddedToShipmentEvent);
    }

    [Fact]
    public void AddPackage_NullPackage_ShouldThrowArgumentNullException()
    {
        // Arrange
        var shipment = CreateValidShipment();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => shipment.AddPackage(null!));
    }

    [Fact]
    public void AddPackage_DuplicatePackage_ShouldSilentlyIgnore()
    {
        // Arrange
        var shipment = CreateValidShipment();
        var existingPackage = shipment.Packages.First();
        var initialEventCount = shipment.DomainEvents.Count;

        // Act
        shipment.AddPackage(existingPackage);

        // Assert
        Assert.Equal(2, shipment.Packages.Count); // Still 2 packages
        Assert.Equal(initialEventCount, shipment.DomainEvents.Count); // No new event raised
    }

    [Fact]
    public void AddDeliveryLeg_ValidConnectedLeg_ShouldAddLeg()
    {
        // Arrange
        var shipment = CreateValidShipment();
        var lastLegEndAddress = shipment.DeliveryLegs.Last().EndAddress;
        var newLeg = CreateDeliveryLegFromAddress(lastLegEndAddress, "Malmö");

        // Act
        shipment.AddDeliveryLeg(newLeg);

        // Assert
        Assert.Equal(3, shipment.DeliveryLegs.Count);
        Assert.Contains(newLeg, shipment.DeliveryLegs);
        Assert.Contains(shipment.DomainEvents, e => e is DeliveryLegAddedToShipmentEvent);
    }

    [Fact]
    public void AddDeliveryLeg_NullLeg_ShouldThrowArgumentNullException()
    {
        // Arrange
        var shipment = CreateValidShipment();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => shipment.AddDeliveryLeg(null!));
    }

    [Fact]
    public void AddDeliveryLeg_DisconnectedLeg_ShouldThrowArgumentException()
    {
        // Arrange
        var shipment = CreateValidShipment();
        var disconnectedLeg = CreateDeliveryLeg("Göteborg", "Malmö"); // Doesn't connect to last leg

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => shipment.AddDeliveryLeg(disconnectedLeg));
        Assert.Contains("does not connect", exception.Message);
    }

    [Fact]
    public void AddDeliveryLeg_DuplicateLeg_ShouldSilentlyIgnore()
    {
        // Arrange
        var shipment = CreateValidShipment();
        var existingLeg = shipment.DeliveryLegs.First();
        var initialEventCount = shipment.DomainEvents.Count;

        // Act
        shipment.AddDeliveryLeg(existingLeg);

        // Assert
        Assert.Equal(2, shipment.DeliveryLegs.Count); // Still 2 legs
        Assert.Equal(initialEventCount, shipment.DomainEvents.Count); // No new event raised
    }

    [Fact]
    public void ChangeStatus_ValidStatus_ShouldChangeStatus()
    {
        // Arrange
        var shipment = CreateValidShipment();

        // Act
        shipment.ChangeStatus(Shipment.ShipmentStatus.Shipped);

        // Assert
        Assert.Equal(Shipment.ShipmentStatus.Shipped, shipment.Status);
        Assert.Contains(shipment.DomainEvents, e => e is ShipmentStatusChangedEvent);
    }

    [Fact]
    public void ChangeStatus_SameStatus_ShouldNotRaiseEvent()
    {
        // Arrange
        var shipment = CreateValidShipment();
        var initialEventCount = shipment.DomainEvents.Count;

        // Act
        shipment.ChangeStatus(Shipment.ShipmentStatus.Pending);

        // Assert
        Assert.Equal(Shipment.ShipmentStatus.Pending, shipment.Status);
        Assert.Equal(initialEventCount, shipment.DomainEvents.Count);
    }

    [Fact]
    public void StartingAddress_ShouldReturnFirstLegStartAddress()
    {
        // Arrange
        var shipment = CreateValidShipment();

        // Act
        var startingAddress = shipment.StartingAddress;

        // Assert
        Assert.NotNull(startingAddress);
        Assert.Equal("Mura", startingAddress.City);
    }

    [Fact]
    public void EndingAddress_ShouldReturnLastLegEndAddress()
    {
        // Arrange
        var shipment = CreateValidShipment();

        // Act
        var endingAddress = shipment.EndingAddress;

        // Assert
        Assert.NotNull(endingAddress);
        Assert.Equal("Göteborg", endingAddress.City);
    }

    [Fact]
    public void IsValid_WithPackagesAndConnectedLegs_ShouldReturnTrue()
    {
        // Arrange
        var shipment = CreateValidShipment();

        // Act & Assert
        Assert.True(shipment.IsValid);
    }

    [Fact]
    public void HasPackages_WithPackages_ShouldReturnTrue()
    {
        // Arrange
        var shipment = CreateValidShipment();

        // Act & Assert
        Assert.True(shipment.HasPackages);
    }

    [Fact]
    public void HasValidDeliveryRoute_WithConnectedLegs_ShouldReturnTrue()
    {
        // Arrange
        var shipment = CreateValidShipment();

        // Act & Assert
        Assert.True(shipment.HasValidDeliveryRoute);
    }

    [Fact]
    public void ClearDomainEvents_ShouldClearAllEvents()
    {
        // Arrange
        var shipment = CreateValidShipment();
        shipment.AddPackage(CreateValidPackage());
        shipment.ChangeStatus(Shipment.ShipmentStatus.Shipped);

        // Act
        shipment.ClearDomainEvents();

        // Assert
        Assert.Empty(shipment.DomainEvents);
    }

    [Fact]
    public void ToString_ShouldReturnFormattedString()
    {
        // Arrange
        var shipment = CreateValidShipment();

        // Act
        var result = shipment.ToString();

        // Assert
        Assert.Contains("Shipment", result);
        Assert.Contains("2 packages", result);
        Assert.Contains("2 legs", result);
        Assert.Contains("Pending", result);
    }

    private Shipment CreateValidShipment()
    {
        var shipmentId = ShipmentId.NewId();
        var packages = CreateValidPackages();
        var deliveryLegs = CreateConnectedDeliveryLegs();
        return new Shipment(shipmentId, DateTime.UtcNow, packages, deliveryLegs);
    }

    private List<Package> CreateValidPackages()
    {
        var package1 = CreateValidPackage();
        var package2 = CreateValidPackage();
        return new List<Package> { package1, package2 };
    }

    private Package CreateValidPackage()
    {
        var packageId = PackageId.NewId();
        var sender = new Address("Tegelstensgatan 12", "Mura", "345 67", "Sverige");
        var recipient = new Address("Storgatan 45", "Stockholm", "111 22", "Sverige");
        var sensorId = new SensorId($"SENSOR_{Guid.NewGuid():N}");
        var createdAt = DateTime.UtcNow;
        var sensorAttachedAt = DateTime.UtcNow;
        
        // Use the hydration constructor to create a package with sensor already attached
        return new Package(packageId, sender, recipient, sensorId,
            new ExpectedRange<Temperature>(new Temperature(2.0m), new Temperature(8.0m)),
            new ExpectedRange<Humidity>(new Humidity(30.0m), new Humidity(70.0m)),
            createdAt, sensorAttachedAt);
    }

    private List<DeliveryLeg> CreateConnectedDeliveryLegs()
    {
        // Create properly connected legs where leg1 ends where leg2 starts
        var stockholmAddress = new Address("Central Station", "Stockholm", "111 22", "Sverige");
        var leg1 = new DeliveryLeg(
            new Address("Tegelstensgatan 12", "Mura", "345 67", "Sverige"),
            stockholmAddress);
        var leg2 = new DeliveryLeg(
            stockholmAddress,
            new Address("Central Station", "Göteborg", "411 38", "Sverige"));
        return new List<DeliveryLeg> { leg1, leg2 };
    }

    private List<DeliveryLeg> CreateDisconnectedDeliveryLegs()
    {
        var leg1 = CreateDeliveryLeg("Mura", "Stockholm");
        var leg2 = CreateDeliveryLeg("Göteborg", "Malmö"); // Doesn't connect to leg1
        return new List<DeliveryLeg> { leg1, leg2 };
    }

    private DeliveryLeg CreateDeliveryLeg(string startCity, string endCity)
    {
        var startAddress = new Address("Start Street 1", startCity, "123 45", "Sverige");
        var endAddress = new Address("End Street 1", endCity, "678 90", "Sverige");
        return new DeliveryLeg(startAddress, endAddress);
    }

    private DeliveryLeg CreateDeliveryLegFromAddress(Address startAddress, string endCity)
    {
        var endAddress = new Address("End Street 1", endCity, "678 90", "Sverige");
        return new DeliveryLeg(startAddress, endAddress);
    }

    [Fact]
    public void ConnectGatewayToDeliveryLeg_ValidLegAndGateway_ShouldConnectGateway()
    {
        // Arrange
        var shipment = CreateValidShipment();
        var originalLeg = shipment.DeliveryLegs.First();
        var gatewayId = GatewayId.NewId();
        var initialEventCount = shipment.DomainEvents.Count;

        // Act
        shipment.ConnectGatewayToDeliveryLeg(originalLeg, gatewayId);

        // Assert
        var updatedLeg = shipment.DeliveryLegs.First();
        Assert.Equal(gatewayId, updatedLeg.GatewayId);
        Assert.Equal(initialEventCount + 1, shipment.DomainEvents.Count);
        Assert.Contains(shipment.DomainEvents, e => e is GatewayConnectedToDeliveryLegEvent);
    }

    [Fact]
    public void ConnectGatewayToDeliveryLeg_NullLeg_ShouldThrowArgumentNullException()
    {
        // Arrange
        var shipment = CreateValidShipment();
        var gatewayId = GatewayId.NewId();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => shipment.ConnectGatewayToDeliveryLeg(null!, gatewayId));
    }

    [Fact]
    public void ConnectGatewayToDeliveryLeg_NullGatewayId_ShouldThrowArgumentNullException()
    {
        // Arrange
        var shipment = CreateValidShipment();
        var originalLeg = shipment.DeliveryLegs.First();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => shipment.ConnectGatewayToDeliveryLeg(originalLeg, null!));
    }

    [Fact]
    public void ConnectGatewayToDeliveryLeg_LegNotInShipment_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var shipment = CreateValidShipment();
        var externalLeg = CreateDeliveryLeg("Malmö", "Uppsala");
        var gatewayId = GatewayId.NewId();

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => shipment.ConnectGatewayToDeliveryLeg(externalLeg, gatewayId));
    }

    [Fact]
    public void ConnectGatewayToDeliveryLeg_AlreadyAssignedGateway_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var shipment = CreateValidShipment();
        var originalLeg = shipment.DeliveryLegs.First();
        var gatewayId1 = GatewayId.NewId();
        var gatewayId2 = GatewayId.NewId();

        // First assignment
        shipment.ConnectGatewayToDeliveryLeg(originalLeg, gatewayId1);

        // Act & Assert - Try to assign another gateway to the same leg
        var updatedLeg = shipment.DeliveryLegs.First();
        Assert.Throws<InvalidOperationException>(() => shipment.ConnectGatewayToDeliveryLeg(updatedLeg, gatewayId2));
    }

    [Fact]
    public void StartDeliveryLeg_ValidLeg_ShouldStartLeg()
    {
        // Arrange
        var shipment = CreateValidShipment();
        var leg = shipment.DeliveryLegs.First();
        var gatewayId = GatewayId.NewId();
        shipment.ConnectGatewayToDeliveryLeg(leg, gatewayId);
        var readyLeg = shipment.DeliveryLegs.First();
        var initialEventCount = shipment.DomainEvents.Count;

        // Act
        shipment.StartDeliveryLeg(readyLeg);

        // Assert
        var startedLeg = shipment.DeliveryLegs.First();
        Assert.Equal(DeliveryLegStatus.InProgress, startedLeg.Status);
        Assert.Equal(initialEventCount + 1, shipment.DomainEvents.Count);
        Assert.Contains(shipment.DomainEvents, e => e is DeliveryLegStartedEvent);
    }

    [Fact]
    public void StartDeliveryLeg_NullLeg_ShouldThrowArgumentNullException()
    {
        // Arrange
        var shipment = CreateValidShipment();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => shipment.StartDeliveryLeg(null!));
    }

    [Fact]
    public void StartDeliveryLeg_LegNotInShipment_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var shipment = CreateValidShipment();
        var externalLeg = CreateDeliveryLeg("Malmö", "Uppsala");
        var gatewayId = GatewayId.NewId();
        externalLeg = externalLeg.AssignGateway(gatewayId);

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => shipment.StartDeliveryLeg(externalLeg));
    }

    [Fact]
    public void StartDeliveryLeg_PlannedLeg_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var shipment = CreateValidShipment();
        var plannedLeg = shipment.DeliveryLegs.First();

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => shipment.StartDeliveryLeg(plannedLeg));
    }

    [Fact]
    public void StartDeliveryLeg_SecondLegWithoutFirstCompleted_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var shipment = CreateValidShipment();
        var firstLeg = shipment.DeliveryLegs.First();
        var secondLeg = shipment.DeliveryLegs.Last();
        var gatewayId1 = GatewayId.NewId();
        var gatewayId2 = GatewayId.NewId();

        // Assign gateways to both legs
        shipment.ConnectGatewayToDeliveryLeg(firstLeg, gatewayId1);
        shipment.ConnectGatewayToDeliveryLeg(secondLeg, gatewayId2);

        // Start first leg but don't complete it
        var readyFirstLeg = shipment.DeliveryLegs.First();
        shipment.StartDeliveryLeg(readyFirstLeg);

        // Act & Assert - Try to start second leg
        var readySecondLeg = shipment.DeliveryLegs.Last();
        Assert.Throws<InvalidOperationException>(() => shipment.StartDeliveryLeg(readySecondLeg));
    }

    [Fact]
    public void StartDeliveryLeg_PackagesWithoutSensors_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var shipment = CreateValidShipment();
        var leg = shipment.DeliveryLegs.First();
        var gatewayId = GatewayId.NewId();
        shipment.ConnectGatewayToDeliveryLeg(leg, gatewayId);
        
        // Create a package without sensor to simulate missing sensor
        var packageWithoutSensor = new Package(
            PackageId.NewId(),
            new Address("Test Street", "Test City", "12345", "Test Country"),
            new Address("Recipient Street", "Recipient City", "54321", "Recipient Country")
        );
        // Note: No sensor attached, no expected ranges set
        
        // Create fresh delivery legs (without any gateway assignments)
        var freshDeliveryLegs = new[]
        {
            new DeliveryLeg(
                new Address("Start Street", "Start City", "11111", "Start Country"),
                new Address("End Street", "End City", "22222", "End Country")
            )
        };
        
        // Create a new shipment with the package without sensor and fresh legs
        var packagesWithoutSensor = new[] { packageWithoutSensor };
        var shipmentWithoutSensors = new Shipment(
            ShipmentId.NewId(),
            DateTime.UtcNow,
            packagesWithoutSensor,
            freshDeliveryLegs
        );
        
        var newGatewayId = GatewayId.NewId(); // Use a different gateway ID
        shipmentWithoutSensors.ConnectGatewayToDeliveryLeg(shipmentWithoutSensors.DeliveryLegs.First(), newGatewayId);

        // Act & Assert
        var legToStart = shipmentWithoutSensors.DeliveryLegs.First(); // Get the leg from the shipment after gateway assignment
        var exception = Assert.Throws<InvalidOperationException>(() => 
            shipmentWithoutSensors.StartDeliveryLeg(legToStart));
        
        Assert.Contains("do not have sensors attached", exception.Message);
        Assert.Contains(packageWithoutSensor.PackageId.ToString(), exception.Message);
    }

    [Fact]
    public void CompleteDeliveryLeg_ValidLeg_ShouldCompleteLeg()
    {
        // Arrange
        var shipment = CreateValidShipment();
        var leg = shipment.DeliveryLegs.First();
        var gatewayId = GatewayId.NewId();
        shipment.ConnectGatewayToDeliveryLeg(leg, gatewayId);
        var readyLeg = shipment.DeliveryLegs.First();
        shipment.StartDeliveryLeg(readyLeg);
        var inProgressLeg = shipment.DeliveryLegs.First();
        var initialEventCount = shipment.DomainEvents.Count;

        // Act
        shipment.CompleteDeliveryLeg(inProgressLeg);

        // Assert
        var completedLeg = shipment.DeliveryLegs.First();
        Assert.Equal(DeliveryLegStatus.Completed, completedLeg.Status);
        Assert.Equal(initialEventCount + 1, shipment.DomainEvents.Count);
        Assert.Contains(shipment.DomainEvents, e => e is DeliveryLegCompletedEvent);
    }

    [Fact]
    public void CompleteDeliveryLeg_NullLeg_ShouldThrowArgumentNullException()
    {
        // Arrange
        var shipment = CreateValidShipment();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => shipment.CompleteDeliveryLeg(null!));
    }

    [Fact]
    public void CompleteDeliveryLeg_LegNotInShipment_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var shipment = CreateValidShipment();
        var externalLeg = CreateDeliveryLeg("Malmö", "Uppsala");
        var gatewayId = GatewayId.NewId();
        externalLeg = externalLeg.AssignGateway(gatewayId).Start();

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => shipment.CompleteDeliveryLeg(externalLeg));
    }

    [Fact]
    public void CompleteDeliveryLeg_PlannedLeg_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var shipment = CreateValidShipment();
        var plannedLeg = shipment.DeliveryLegs.First();

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => shipment.CompleteDeliveryLeg(plannedLeg));
    }

    [Fact]
    public void CompleteDeliveryLeg_ReadyLeg_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var shipment = CreateValidShipment();
        var leg = shipment.DeliveryLegs.First();
        var gatewayId = GatewayId.NewId();
        shipment.ConnectGatewayToDeliveryLeg(leg, gatewayId);
        var readyLeg = shipment.DeliveryLegs.First();

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => shipment.CompleteDeliveryLeg(readyLeg));
    }

    [Fact]
    public void StartMeasurementSession_ValidDeliveryLeg_ShouldRaiseMeasurementSessionStartedEvent()
    {
        // Arrange
        var shipment = CreateValidShipment();
        var deliveryLeg = shipment.DeliveryLegs.First();
        shipment.ConnectGatewayToDeliveryLeg(deliveryLeg, GatewayId.NewId());
        shipment.StartDeliveryLeg(deliveryLeg);

        // Act
        shipment.StartMeasurementSession(deliveryLeg);

        // Assert
        var measurementEvent = shipment.DomainEvents.OfType<MeasurementSessionStartedEvent>().FirstOrDefault();
        Assert.NotNull(measurementEvent);
        Assert.Equal(shipment.ShipmentId, measurementEvent.ShipmentId);
        Assert.Equal(deliveryLeg, measurementEvent.DeliveryLeg);
        Assert.Equal(shipment.Packages.Select(p => p.PackageId).ToList(), measurementEvent.PackageIds);
        Assert.Equal(deliveryLeg.GatewayId, measurementEvent.GatewayId);
    }

    [Fact]
    public void StartMeasurementSession_DeliveryLegNotInProgress_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var shipment = CreateValidShipment();
        var deliveryLeg = shipment.DeliveryLegs.First();

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => shipment.StartMeasurementSession(deliveryLeg));
    }

    [Fact]
    public void StartMeasurementSession_DeliveryLegWithoutGateway_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var shipment = CreateValidShipment();
        var deliveryLeg = shipment.DeliveryLegs.First();
        shipment.StartDeliveryLeg(deliveryLeg);

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => shipment.StartMeasurementSession(deliveryLeg));
    }

    [Fact]
    public void CompleteMeasurementSession_ValidDeliveryLeg_ShouldRaiseMeasurementSessionCompletedEvent()
    {
        // Arrange
        var shipment = CreateValidShipment();
        var deliveryLeg = shipment.DeliveryLegs.First();
        shipment.ConnectGatewayToDeliveryLeg(deliveryLeg, GatewayId.NewId());
        shipment.StartDeliveryLeg(deliveryLeg);
        shipment.CompleteDeliveryLeg(deliveryLeg);

        // Act
        shipment.CompleteMeasurementSession(deliveryLeg);

        // Assert
        var measurementEvent = shipment.DomainEvents.OfType<MeasurementSessionCompletedEvent>().FirstOrDefault();
        Assert.NotNull(measurementEvent);
        Assert.Equal(shipment.ShipmentId, measurementEvent.ShipmentId);
        Assert.Equal(deliveryLeg, measurementEvent.DeliveryLeg);
        Assert.NotNull(measurementEvent.Summary);
    }

    [Fact]
    public void CompleteMeasurementSession_DeliveryLegNotCompleted_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var shipment = CreateValidShipment();
        var deliveryLeg = shipment.DeliveryLegs.First();

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => shipment.CompleteMeasurementSession(deliveryLeg));
    }

    [Fact]
    public void GetExpectedSensorIds_ShouldReturnSensorIdsFromPackagesWithSensors()
    {
        // Arrange
        var shipment = CreateValidShipment();
        var package1 = shipment.Packages.First();
        var package2 = shipment.Packages.Last();
        
        package1.AttachSensor(SensorId.NewId());
        package2.AttachSensor(SensorId.NewId());

        // Act
        var expectedSensorIds = shipment.GetExpectedSensorIds();

        // Assert
        Assert.Equal(2, expectedSensorIds.Count);
        Assert.Contains(package1.SensorId!.Value, expectedSensorIds);
        Assert.Contains(package2.SensorId!.Value, expectedSensorIds);
    }

    [Fact]
    public void GetExpectedSensorIds_PackagesWithoutSensors_ShouldReturnEmptyList()
    {
        // Arrange
        var shipment = CreateValidShipment();

        // Act
        var expectedSensorIds = shipment.GetExpectedSensorIds();

        // Assert
        Assert.Empty(expectedSensorIds);
    }

    [Fact]
    public void VerifySensorPresence_ValidDeliveryLeg_ShouldRaiseSensorPresenceVerifiedEvent()
    {
        // Arrange
        var shipment = CreateValidShipment();
        var deliveryLeg = shipment.DeliveryLegs.First();
        shipment.ConnectGatewayToDeliveryLeg(deliveryLeg, GatewayId.NewId());
        shipment.StartDeliveryLeg(deliveryLeg);
        
        var package1 = shipment.Packages.First();
        var package2 = shipment.Packages.Last();
        package1.AttachSensor(SensorId.NewId());
        package2.AttachSensor(SensorId.NewId());
        
        var presentSensors = new List<SensorId> { package1.SensorId!.Value };

        // Act
        shipment.VerifySensorPresence(deliveryLeg, presentSensors);

        // Assert
        var presenceEvent = shipment.DomainEvents.OfType<SensorPresenceVerifiedEvent>().FirstOrDefault();
        Assert.NotNull(presenceEvent);
        Assert.Equal(shipment.ShipmentId, presenceEvent.ShipmentId);
        Assert.Equal(deliveryLeg, presenceEvent.DeliveryLeg);
        Assert.Equal(presentSensors, presenceEvent.PresentSensors);
        Assert.Single(presenceEvent.MissingSensors);
        Assert.Equal(package2.SensorId!.Value, presenceEvent.MissingSensors.First());
    }

    [Fact]
    public void VerifySensorPresence_DeliveryLegNotInProgress_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var shipment = CreateValidShipment();
        var deliveryLeg = shipment.DeliveryLegs.First();
        var presentSensors = new List<SensorId>();

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => shipment.VerifySensorPresence(deliveryLeg, presentSensors));
    }

    [Fact]
    public void RecordMeasurementBatch_ValidDeliveryLeg_ShouldRaiseMeasurementBatchReceivedEvent()
    {
        // Arrange
        var shipment = CreateValidShipment();
        var deliveryLeg = shipment.DeliveryLegs.First();
        shipment.ConnectGatewayToDeliveryLeg(deliveryLeg, GatewayId.NewId());
        shipment.StartDeliveryLeg(deliveryLeg);
        
        var readings = CreateValidReadings(3);

        // Act
        shipment.RecordMeasurementBatch(deliveryLeg, readings);

        // Assert
        var batchEvent = shipment.DomainEvents.OfType<MeasurementBatchReceivedEvent>().FirstOrDefault();
        Assert.NotNull(batchEvent);
        Assert.Equal(shipment.ShipmentId, batchEvent.ShipmentId);
        Assert.Equal(deliveryLeg, batchEvent.DeliveryLeg);
        Assert.Equal(readings, batchEvent.Readings);
    }

    [Fact]
    public void RecordMeasurementBatch_DeliveryLegNotInProgress_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var shipment = CreateValidShipment();
        var deliveryLeg = shipment.DeliveryLegs.First();
        var readings = CreateValidReadings(1);

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => shipment.RecordMeasurementBatch(deliveryLeg, readings));
    }

    [Fact]
    public void RecordMeasurementBatch_EmptyReadings_ShouldThrowArgumentException()
    {
        // Arrange
        var shipment = CreateValidShipment();
        var deliveryLeg = shipment.DeliveryLegs.First();
        shipment.ConnectGatewayToDeliveryLeg(deliveryLeg, GatewayId.NewId());
        shipment.StartDeliveryLeg(deliveryLeg);
        
        var emptyReadings = new List<MeasurementReading>();

        // Act & Assert
        Assert.Throws<ArgumentException>(() => shipment.RecordMeasurementBatch(deliveryLeg, emptyReadings));
    }

    [Fact]
    public void RecordMeasurementBatch_NullReadings_ShouldThrowArgumentException()
    {
        // Arrange
        var shipment = CreateValidShipment();
        var deliveryLeg = shipment.DeliveryLegs.First();
        shipment.ConnectGatewayToDeliveryLeg(deliveryLeg, GatewayId.NewId());
        shipment.StartDeliveryLeg(deliveryLeg);

        // Act & Assert
        Assert.Throws<ArgumentException>(() => shipment.RecordMeasurementBatch(deliveryLeg, null!));
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
