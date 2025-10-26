using GpsApp.Domain.Aggregates;
using GpsApp.Domain.ValueObjects;
using GpsApp.Domain.Events;

namespace GpsApp.Tests.Unit.Domain;

public class PackageTests
{
    [Fact]
    public void Constructor_ValidParameters_ShouldCreatePackage()
    {
        // Arrange
        var packageId = PackageId.NewId();
        var sender = new Address("Tegelstensgatan 12", "Mura", "345 67", "Sverige");
        var recipient = new Address("Storgatan 45", "Stockholm", "111 22", "Sverige");

        // Act
        var package = new Package(packageId, sender, recipient);

        // Assert
        Assert.Equal(packageId, package.PackageId);
        Assert.Equal(sender, package.Sender);
        Assert.Equal(recipient, package.Recipient);
        Assert.False(package.HasSensor);
        Assert.False(package.IsReadyForShipping);
        Assert.Null(package.SensorId);
        Assert.Null(package.ExpectedTemperatureRange);
        Assert.Null(package.ExpectedHumidityRange);
        Assert.Single(package.DomainEvents);
        Assert.IsType<PackageCreatedEvent>(package.DomainEvents.First());
    }

    [Fact]
    public void Constructor_NullPackageId_ShouldThrowArgumentNullException()
    {
        // Arrange
        var sender = new Address("Tegelstensgatan 12", "Mura", "345 67", "Sverige");
        var recipient = new Address("Storgatan 45", "Stockholm", "111 22", "Sverige");

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new Package(null!, sender, recipient));
    }

    [Fact]
    public void Constructor_NullSender_ShouldThrowArgumentNullException()
    {
        // Arrange
        var packageId = PackageId.NewId();
        var recipient = new Address("Storgatan 45", "Stockholm", "111 22", "Sverige");

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new Package(packageId, null!, recipient));
    }

    [Fact]
    public void Constructor_NullRecipient_ShouldThrowArgumentNullException()
    {
        // Arrange
        var packageId = PackageId.NewId();
        var sender = new Address("Tegelstensgatan 12", "Mura", "345 67", "Sverige");

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new Package(packageId, sender, null!));
    }

    [Fact]
    public void AttachSensor_ValidSensorId_ShouldAttachSensor()
    {
        // Arrange
        var package = CreateValidPackage();
        var sensorId = new SensorId("SENSOR-001");

        // Act
        package.AttachSensor(sensorId);

        // Assert
        Assert.Equal(sensorId, package.SensorId);
        Assert.True(package.HasSensor);
        Assert.NotNull(package.SensorAttachedAt);
        Assert.Contains(package.DomainEvents, e => e is SensorAttachedEvent);
    }

    [Fact]
    public void AttachSensor_NullSensorId_ShouldThrowArgumentNullException()
    {
        // Arrange
        var package = CreateValidPackage();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => package.AttachSensor(null!));
    }

    [Fact]
    public void AttachSensor_SensorAlreadyAttached_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var package = CreateValidPackage();
        var sensorId1 = new SensorId("SENSOR-001");
        var sensorId2 = new SensorId("SENSOR-002");

        // Act
        package.AttachSensor(sensorId1);

        // Assert
        Assert.Throws<InvalidOperationException>(() => package.AttachSensor(sensorId2));
    }

    [Fact]
    public void SetExpectedTemperatureRange_ValidRange_ShouldSetRange()
    {
        // Arrange
        var package = CreateValidPackage();
        var temperatureRange = new ExpectedRange<Temperature>(
            new Temperature(2.0m), 
            new Temperature(8.0m)
        );

        // Act
        package.SetExpectedTemperatureRange(temperatureRange);

        // Assert
        Assert.Equal(temperatureRange, package.ExpectedTemperatureRange);
        Assert.Contains(package.DomainEvents, e => e is ExpectedTemperatureRangeSetEvent);
    }

    [Fact]
    public void SetExpectedTemperatureRange_NullRange_ShouldThrowArgumentNullException()
    {
        // Arrange
        var package = CreateValidPackage();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => package.SetExpectedTemperatureRange(null!));
    }

    [Fact]
    public void SetExpectedHumidityRange_ValidRange_ShouldSetRange()
    {
        // Arrange
        var package = CreateValidPackage();
        var humidityRange = new ExpectedRange<Humidity>(
            new Humidity(30.0m), 
            new Humidity(70.0m)
        );

        // Act
        package.SetExpectedHumidityRange(humidityRange);

        // Assert
        Assert.Equal(humidityRange, package.ExpectedHumidityRange);
        Assert.Contains(package.DomainEvents, e => e is ExpectedHumidityRangeSetEvent);
    }

    [Fact]
    public void SetExpectedHumidityRange_NullRange_ShouldThrowArgumentNullException()
    {
        // Arrange
        var package = CreateValidPackage();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => package.SetExpectedHumidityRange(null!));
    }

    [Fact]
    public void IsReadyForShipping_WithSensorAndRanges_ShouldReturnTrue()
    {
        // Arrange
        var package = CreateValidPackage();
        package.AttachSensor(new SensorId("SENSOR-001"));
        package.SetExpectedTemperatureRange(new ExpectedRange<Temperature>(
            new Temperature(2.0m), 
            new Temperature(8.0m)
        ));
        package.SetExpectedHumidityRange(new ExpectedRange<Humidity>(
            new Humidity(30.0m), 
            new Humidity(70.0m)
        ));

        // Act & Assert
        Assert.True(package.IsReadyForShipping);
    }

    [Fact]
    public void IsReadyForShipping_WithoutSensor_ShouldReturnFalse()
    {
        // Arrange
        var package = CreateValidPackage();
        package.SetExpectedTemperatureRange(new ExpectedRange<Temperature>(
            new Temperature(2.0m), 
            new Temperature(8.0m)
        ));
        package.SetExpectedHumidityRange(new ExpectedRange<Humidity>(
            new Humidity(30.0m), 
            new Humidity(70.0m)
        ));

        // Act & Assert
        Assert.False(package.IsReadyForShipping);
    }

    [Fact]
    public void IsReadyForShipping_WithoutTemperatureRange_ShouldReturnFalse()
    {
        // Arrange
        var package = CreateValidPackage();
        package.AttachSensor(new SensorId("SENSOR-001"));
        package.SetExpectedHumidityRange(new ExpectedRange<Humidity>(
            new Humidity(30.0m), 
            new Humidity(70.0m)
        ));

        // Act & Assert
        Assert.False(package.IsReadyForShipping);
    }

    [Fact]
    public void IsReadyForShipping_WithoutHumidityRange_ShouldReturnFalse()
    {
        // Arrange
        var package = CreateValidPackage();
        package.AttachSensor(new SensorId("SENSOR-001"));
        package.SetExpectedTemperatureRange(new ExpectedRange<Temperature>(
            new Temperature(2.0m), 
            new Temperature(8.0m)
        ));

        // Act & Assert
        Assert.False(package.IsReadyForShipping);
    }

    [Fact]
    public void IsTemperatureInRange_WithinRange_ShouldReturnTrue()
    {
        // Arrange
        var package = CreateValidPackage();
        var temperatureRange = new ExpectedRange<Temperature>(
            new Temperature(2.0m), 
            new Temperature(8.0m)
        );
        package.SetExpectedTemperatureRange(temperatureRange);
        var temperature = new Temperature(5.0m);

        // Act
        var result = package.IsTemperatureInRange(temperature);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IsTemperatureInRange_OutsideRange_ShouldReturnFalse()
    {
        // Arrange
        var package = CreateValidPackage();
        var temperatureRange = new ExpectedRange<Temperature>(
            new Temperature(2.0m), 
            new Temperature(8.0m)
        );
        package.SetExpectedTemperatureRange(temperatureRange);
        var temperature = new Temperature(10.0m);

        // Act
        var result = package.IsTemperatureInRange(temperature);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void IsTemperatureInRange_NoRangeSet_ShouldReturnTrue()
    {
        // Arrange
        var package = CreateValidPackage();
        var temperature = new Temperature(5.0m);

        // Act
        var result = package.IsTemperatureInRange(temperature);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IsHumidityInRange_WithinRange_ShouldReturnTrue()
    {
        // Arrange
        var package = CreateValidPackage();
        var humidityRange = new ExpectedRange<Humidity>(
            new Humidity(30.0m), 
            new Humidity(70.0m)
        );
        package.SetExpectedHumidityRange(humidityRange);
        var humidity = new Humidity(50.0m);

        // Act
        var result = package.IsHumidityInRange(humidity);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IsHumidityInRange_OutsideRange_ShouldReturnFalse()
    {
        // Arrange
        var package = CreateValidPackage();
        var humidityRange = new ExpectedRange<Humidity>(
            new Humidity(30.0m), 
            new Humidity(70.0m)
        );
        package.SetExpectedHumidityRange(humidityRange);
        var humidity = new Humidity(80.0m);

        // Act
        var result = package.IsHumidityInRange(humidity);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void IsHumidityInRange_NoRangeSet_ShouldReturnTrue()
    {
        // Arrange
        var package = CreateValidPackage();
        var humidity = new Humidity(50.0m);

        // Act
        var result = package.IsHumidityInRange(humidity);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void ClearDomainEvents_ShouldClearAllEvents()
    {
        // Arrange
        var package = CreateValidPackage();
        package.AttachSensor(new SensorId("SENSOR-001"));
        package.SetExpectedTemperatureRange(new ExpectedRange<Temperature>(
            new Temperature(2.0m), 
            new Temperature(8.0m)
        ));

        // Act
        package.ClearDomainEvents();

        // Assert
        Assert.Empty(package.DomainEvents);
    }

    [Fact]
    public void ToString_ShouldReturnFormattedString()
    {
        // Arrange
        var package = CreateValidPackage();

        // Act
        var result = package.ToString();

        // Assert
        Assert.Contains("Package", result);
        Assert.Contains("Mura", result);
        Assert.Contains("Stockholm", result);
    }

    private Package CreateValidPackage()
    {
        var packageId = PackageId.NewId();
        var sender = new Address("Tegelstensgatan 12", "Mura", "345 67", "Sverige");
        var recipient = new Address("Storgatan 45", "Stockholm", "111 22", "Sverige");
        return new Package(packageId, sender, recipient);
    }
}
