using GpsApp.Domain.Aggregates;
using GpsApp.Domain.ValueObjects;
using GpsApp.Domain.Events;

namespace GpsApp.Tests.Unit.Domain;

/// <summary>
/// Tests for hydration constructors that allow reconstruction of domain objects from persistence storage
/// These tests ensure that objects can be properly reconstructed without raising domain events
/// </summary>
public class HydrationTests
{
    [Fact]
    public void Shipment_HydrationConstructor_ShouldReconstructWithoutDomainEvents()
    {
        // Arrange
        var shipmentId = ShipmentId.NewId();
        var shipmentDate = DateTime.UtcNow;
        var status = Shipment.ShipmentStatus.Shipped;
        var packages = CreateValidPackages();
        var deliveryLegs = CreateConnectedDeliveryLegs();

        // Act
        var shipment = new Shipment(shipmentId, shipmentDate, status, packages, deliveryLegs);

        // Assert
        Assert.Equal(shipmentId, shipment.ShipmentId);
        Assert.Equal(shipmentDate, shipment.ShipmentDate);
        Assert.Equal(status, shipment.Status);
        Assert.Equal(2, shipment.Packages.Count);
        Assert.Equal(2, shipment.DeliveryLegs.Count);
        Assert.Empty(shipment.DomainEvents); // No domain events should be raised during hydration
    }

    [Fact]
    public void Shipment_HydrationConstructor_NullShipmentId_ShouldThrowArgumentNullException()
    {
        // Arrange
        var packages = CreateValidPackages();
        var deliveryLegs = CreateConnectedDeliveryLegs();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => 
            new Shipment(null!, DateTime.UtcNow, Shipment.ShipmentStatus.Pending, packages, deliveryLegs));
    }

    [Fact]
    public void Shipment_HydrationConstructor_EmptyPackages_ShouldThrowArgumentException()
    {
        // Arrange
        var shipmentId = ShipmentId.NewId();
        var deliveryLegs = CreateConnectedDeliveryLegs();

        // Act & Assert
        Assert.Throws<ArgumentException>(() => 
            new Shipment(shipmentId, DateTime.UtcNow, Shipment.ShipmentStatus.Pending, new List<Package>(), deliveryLegs));
    }

    [Fact]
    public void DeliveryLeg_HydrationConstructor_ShouldReconstructWithAllProperties()
    {
        // Arrange
        var startAddress = new Address("Start Street", "Start City", "12345", "Start Country");
        var endAddress = new Address("End Street", "End City", "67890", "End Country");
        var gatewayId = GatewayId.NewId();
        var status = DeliveryLegStatus.InProgress;

        // Act
        var deliveryLeg = new DeliveryLeg(DeliveryLegId.NewId(), startAddress, endAddress, gatewayId, status);

        // Assert
        Assert.Equal(startAddress, deliveryLeg.StartAddress);
        Assert.Equal(endAddress, deliveryLeg.EndAddress);
        Assert.Equal(gatewayId, deliveryLeg.GatewayId);
        Assert.Equal(status, deliveryLeg.Status);
    }

    [Fact]
    public void DeliveryLeg_HydrationConstructor_NullGatewayId_ShouldAcceptNull()
    {
        // Arrange
        var startAddress = new Address("Start Street", "Start City", "12345", "Start Country");
        var endAddress = new Address("End Street", "End City", "67890", "End Country");
        var status = DeliveryLegStatus.Planned;

        // Act
        var deliveryLeg = new DeliveryLeg(DeliveryLegId.NewId(), startAddress, endAddress, null, status);

        // Assert
        Assert.Equal(startAddress, deliveryLeg.StartAddress);
        Assert.Equal(endAddress, deliveryLeg.EndAddress);
        Assert.Null(deliveryLeg.GatewayId);
        Assert.Equal(status, deliveryLeg.Status);
    }

    [Fact]
    public void DeliveryLeg_HydrationConstructor_NullStartAddress_ShouldThrowArgumentNullException()
    {
        // Arrange
        var endAddress = new Address("End Street", "End City", "67890", "End Country");

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => 
            new DeliveryLeg(DeliveryLegId.NewId(), null!, endAddress, null, DeliveryLegStatus.Planned));
    }

    [Fact]
    public void Address_HydrationConstructor_ShouldReconstructWithAllProperties()
    {
        // Arrange
        var street = "Test Street";
        var city = "Test City";
        var postalCode = "12345";
        var country = "Test Country";

        // Act
        var address = new Address(street, city, postalCode, country, skipValidation: true);

        // Assert
        Assert.Equal(street, address.Street);
        Assert.Equal(city, address.City);
        Assert.Equal(postalCode, address.PostalCode);
        Assert.Equal(country, address.Country);
    }

    [Fact]
    public void Address_HydrationConstructor_WithNullValues_ShouldHandleGracefully()
    {
        // Arrange
        string? street = null;
        string? city = null;
        string? postalCode = null;
        string? country = null;

        // Act
        var address = new Address(street!, city!, postalCode!, country!, skipValidation: true);

        // Assert
        Assert.Equal(string.Empty, address.Street);
        Assert.Equal(string.Empty, address.City);
        Assert.Equal(string.Empty, address.PostalCode);
        Assert.Equal(string.Empty, address.Country);
    }

    [Fact]
    public void Address_HydrationConstructor_WithValidationEnabled_ShouldValidate()
    {
        // Arrange
        var street = "";
        var city = "Test City";
        var postalCode = "12345";
        var country = "Test Country";

        // Act & Assert
        Assert.Throws<ArgumentException>(() => 
            new Address(street, city, postalCode, country, skipValidation: false));
    }

    [Fact]
    public void ExpectedRange_HydrationConstructor_ShouldReconstructWithAllProperties()
    {
        // Arrange
        var minimum = new Temperature(2.0m);
        var maximum = new Temperature(8.0m);

        // Act
        var range = new ExpectedRange<Temperature>(minimum, maximum, skipValidation: true);

        // Assert
        Assert.Equal(minimum, range.Minimum);
        Assert.Equal(maximum, range.Maximum);
    }

    [Fact]
    public void ExpectedRange_HydrationConstructor_WithInvalidRange_ShouldSkipValidation()
    {
        // Arrange
        var minimum = new Temperature(8.0m); // Higher than maximum
        var maximum = new Temperature(2.0m); // Lower than minimum

        // Act
        var range = new ExpectedRange<Temperature>(minimum, maximum, skipValidation: true);

        // Assert
        Assert.Equal(minimum, range.Minimum);
        Assert.Equal(maximum, range.Maximum);
    }

    [Fact]
    public void ExpectedRange_HydrationConstructor_WithValidationEnabled_ShouldValidate()
    {
        // Arrange
        var minimum = new Temperature(8.0m); // Higher than maximum
        var maximum = new Temperature(2.0m); // Lower than minimum

        // Act & Assert
        Assert.Throws<ArgumentException>(() => 
            new ExpectedRange<Temperature>(minimum, maximum, skipValidation: false));
    }

    [Fact]
    public void Temperature_HydrationConstructor_ShouldReconstructWithValue()
    {
        // Arrange
        var value = 25.5m;

        // Act
        var temperature = new Temperature(value, skipValidation: true);

        // Assert
        Assert.Equal(value, temperature.Value);
        Assert.Equal("°C", temperature.Unit);
    }

    [Fact]
    public void Temperature_HydrationConstructor_WithInvalidValue_ShouldSkipValidation()
    {
        // Arrange
        var value = -300m; // Below absolute zero

        // Act
        var temperature = new Temperature(value, skipValidation: true);

        // Assert
        Assert.Equal(value, temperature.Value);
    }

    [Fact]
    public void Temperature_HydrationConstructor_WithValidationEnabled_ShouldValidate()
    {
        // Arrange
        var value = -300m; // Below absolute zero

        // Act & Assert
        Assert.Throws<ArgumentException>(() => 
            new Temperature(value, skipValidation: false));
    }

    [Fact]
    public void Humidity_HydrationConstructor_ShouldReconstructWithValue()
    {
        // Arrange
        var value = 65.5m;

        // Act
        var humidity = new Humidity(value, skipValidation: true);

        // Assert
        Assert.Equal(value, humidity.Value);
        Assert.Equal("%", humidity.Unit);
    }

    [Fact]
    public void Humidity_HydrationConstructor_WithInvalidValue_ShouldSkipValidation()
    {
        // Arrange
        var value = 150m; // Above 100%

        // Act
        var humidity = new Humidity(value, skipValidation: true);

        // Assert
        Assert.Equal(value, humidity.Value);
    }

    [Fact]
    public void Humidity_HydrationConstructor_WithValidationEnabled_ShouldValidate()
    {
        // Arrange
        var value = 150m; // Above 100%

        // Act & Assert
        Assert.Throws<ArgumentException>(() => 
            new Humidity(value, skipValidation: false));
    }

    [Fact]
    public void Package_HydrationConstructor_ShouldReconstructWithoutDomainEvents()
    {
        // Arrange
        var packageId = PackageId.NewId();
        var sender = new Address("Sender Street", "Sender City", "12345", "Sender Country");
        var recipient = new Address("Recipient Street", "Recipient City", "67890", "Recipient Country");
        var sensorId = SensorId.NewId();
        var temperatureRange = new ExpectedRange<Temperature>(new Temperature(2.0m), new Temperature(8.0m));
        var humidityRange = new ExpectedRange<Humidity>(new Humidity(30.0m), new Humidity(70.0m));
        var createdAt = DateTime.UtcNow;
        var sensorAttachedAt = DateTime.UtcNow.AddMinutes(5);

        // Act
        var package = new Package(packageId, sender, recipient, sensorId, temperatureRange, humidityRange, createdAt, sensorAttachedAt);

        // Assert
        Assert.Equal(packageId, package.PackageId);
        Assert.Equal(sender, package.Sender);
        Assert.Equal(recipient, package.Recipient);
        Assert.Equal(sensorId, package.SensorId);
        Assert.Equal(temperatureRange, package.ExpectedTemperatureRange);
        Assert.Equal(humidityRange, package.ExpectedHumidityRange);
        Assert.Equal(createdAt, package.CreatedAt);
        Assert.Equal(sensorAttachedAt, package.SensorAttachedAt);
        Assert.Empty(package.DomainEvents); // No domain events should be raised during hydration
    }

    [Fact]
    public void Package_HydrationConstructor_WithNullOptionalValues_ShouldHandleGracefully()
    {
        // Arrange
        var packageId = PackageId.NewId();
        var sender = new Address("Sender Street", "Sender City", "12345", "Sender Country");
        var recipient = new Address("Recipient Street", "Recipient City", "67890", "Recipient Country");
        var createdAt = DateTime.UtcNow;

        // Act
        var package = new Package(packageId, sender, recipient, null, null, null, createdAt, null);

        // Assert
        Assert.Equal(packageId, package.PackageId);
        Assert.Equal(sender, package.Sender);
        Assert.Equal(recipient, package.Recipient);
        Assert.Null(package.SensorId);
        Assert.Null(package.ExpectedTemperatureRange);
        Assert.Null(package.ExpectedHumidityRange);
        Assert.Equal(createdAt, package.CreatedAt);
        Assert.Null(package.SensorAttachedAt);
        Assert.Empty(package.DomainEvents);
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
        var sensorId = SensorId.NewId();
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
}
