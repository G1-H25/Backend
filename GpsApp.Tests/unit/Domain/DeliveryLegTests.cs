using GpsApp.Domain.ValueObjects;

namespace GpsApp.Tests.Unit.Domain;

public class DeliveryLegTests
{
    [Fact]
    public void Constructor_ValidAddresses_ShouldCreateDeliveryLeg()
    {
        // Arrange
        var startAddress = new Address("Tegelstensgatan 12", "Mura", "345 67", "Sverige");
        var endAddress = new Address("Storgatan 45", "Stockholm", "111 22", "Sverige");

        // Act
        var deliveryLeg = new DeliveryLeg(startAddress, endAddress);

        // Assert
        Assert.Equal(startAddress, deliveryLeg.StartAddress);
        Assert.Equal(endAddress, deliveryLeg.EndAddress);
    }

    [Fact]
    public void Constructor_NullStartAddress_ShouldThrowArgumentNullException()
    {
        // Arrange
        var endAddress = new Address("Storgatan 45", "Stockholm", "111 22", "Sverige");

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new DeliveryLeg(null!, endAddress));
    }

    [Fact]
    public void Constructor_NullEndAddress_ShouldThrowArgumentNullException()
    {
        // Arrange
        var startAddress = new Address("Tegelstensgatan 12", "Mura", "345 67", "Sverige");

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new DeliveryLeg(startAddress, null!));
    }

    [Fact]
    public void ConnectsTo_ConnectedLeg_ShouldReturnTrue()
    {
        // Arrange
        var stockholmAddress = new Address("Central Station", "Stockholm", "111 22", "Sverige");
        var leg1 = new DeliveryLeg(
            new Address("Tegelstensgatan 12", "Mura", "345 67", "Sverige"),
            stockholmAddress);
        var leg2 = new DeliveryLeg(
            stockholmAddress,
            new Address("Central Station", "Göteborg", "411 38", "Sverige"));

        // Act
        var result = leg1.ConnectsTo(leg2);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void ConnectsTo_DisconnectedLeg_ShouldReturnFalse()
    {
        // Arrange
        var leg1 = CreateDeliveryLeg("Mura", "Stockholm");
        var leg2 = CreateDeliveryLeg("Göteborg", "Malmö");

        // Act
        var result = leg1.ConnectsTo(leg2);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void ConnectsTo_NullLeg_ShouldReturnFalse()
    {
        // Arrange
        var leg1 = CreateDeliveryLeg("Mura", "Stockholm");

        // Act
        var result = leg1.ConnectsTo(null!);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void IsConnectedFrom_ConnectedLeg_ShouldReturnTrue()
    {
        // Arrange
        var stockholmAddress = new Address("Central Station", "Stockholm", "111 22", "Sverige");
        var leg1 = new DeliveryLeg(
            new Address("Tegelstensgatan 12", "Mura", "345 67", "Sverige"),
            stockholmAddress);
        var leg2 = new DeliveryLeg(
            stockholmAddress,
            new Address("Central Station", "Göteborg", "411 38", "Sverige"));

        // Act
        var result = leg2.IsConnectedFrom(leg1);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IsConnectedFrom_DisconnectedLeg_ShouldReturnFalse()
    {
        // Arrange
        var leg1 = CreateDeliveryLeg("Mura", "Stockholm");
        var leg2 = CreateDeliveryLeg("Göteborg", "Malmö");

        // Act
        var result = leg2.IsConnectedFrom(leg1);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void IsConnectedFrom_NullLeg_ShouldReturnFalse()
    {
        // Arrange
        var leg2 = CreateDeliveryLeg("Stockholm", "Göteborg");

        // Act
        var result = leg2.IsConnectedFrom(null!);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void Equals_SameDeliveryLeg_ShouldReturnTrue()
    {
        // Arrange
        var leg1 = CreateDeliveryLeg("Mura", "Stockholm");
        var leg2 = CreateDeliveryLeg("Mura", "Stockholm");

        // Act & Assert
        Assert.True(leg1.Equals(leg2));
        Assert.True(leg1 == leg2);
        Assert.False(leg1 != leg2);
    }

    [Fact]
    public void Equals_DifferentDeliveryLeg_ShouldReturnFalse()
    {
        // Arrange
        var leg1 = CreateDeliveryLeg("Mura", "Stockholm");
        var leg2 = CreateDeliveryLeg("Stockholm", "Göteborg");

        // Act & Assert
        Assert.False(leg1.Equals(leg2));
        Assert.False(leg1 == leg2);
        Assert.True(leg1 != leg2);
    }

    [Fact]
    public void Equals_NullDeliveryLeg_ShouldReturnFalse()
    {
        // Arrange
        var leg1 = CreateDeliveryLeg("Mura", "Stockholm");

        // Act & Assert
        Assert.False(leg1.Equals(null));
        Assert.False(leg1 == null);
        Assert.True(leg1 != null);
    }

    [Fact]
    public void GetHashCode_SameDeliveryLeg_ShouldReturnSameHashCode()
    {
        // Arrange
        var leg1 = CreateDeliveryLeg("Mura", "Stockholm");
        var leg2 = CreateDeliveryLeg("Mura", "Stockholm");

        // Act
        var hashCode1 = leg1.GetHashCode();
        var hashCode2 = leg2.GetHashCode();

        // Assert
        Assert.Equal(hashCode1, hashCode2);
    }

    [Fact]
    public void GetHashCode_DifferentDeliveryLeg_ShouldReturnDifferentHashCode()
    {
        // Arrange
        var leg1 = CreateDeliveryLeg("Mura", "Stockholm");
        var leg2 = CreateDeliveryLeg("Stockholm", "Göteborg");

        // Act
        var hashCode1 = leg1.GetHashCode();
        var hashCode2 = leg2.GetHashCode();

        // Assert
        Assert.NotEqual(hashCode1, hashCode2);
    }

    [Fact]
    public void ToString_ShouldReturnFormattedString()
    {
        // Arrange
        var leg = CreateDeliveryLeg("Mura", "Stockholm");

        // Act
        var result = leg.ToString();

        // Assert
        Assert.Contains("From", result);
        Assert.Contains("to", result);
        Assert.Contains("Mura", result);
        Assert.Contains("Stockholm", result);
    }

    private DeliveryLeg CreateDeliveryLeg(string startCity, string endCity)
    {
        var startAddress = new Address("Start Street 1", startCity, "123 45", "Sverige");
        var endAddress = new Address("End Street 1", endCity, "678 90", "Sverige");
        return new DeliveryLeg(startAddress, endAddress);
    }
}
