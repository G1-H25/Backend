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
        return new Package(packageId, sender, recipient);
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
}
