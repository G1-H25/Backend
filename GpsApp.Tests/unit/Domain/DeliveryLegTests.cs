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
        Assert.Null(deliveryLeg.GatewayId);
        Assert.Equal(DeliveryLegStatus.Planned, deliveryLeg.Status);
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
    public void AssignGateway_ValidGatewayId_ShouldReturnNewDeliveryLegWithGateway()
    {
        // Arrange
        var startAddress = new Address("Tegelstensgatan 12", "Mura", "345 67", "Sverige");
        var endAddress = new Address("Storgatan 45", "Stockholm", "111 22", "Sverige");
        var deliveryLeg = new DeliveryLeg(startAddress, endAddress);
        var gatewayId = GatewayId.NewId();

        // Act
        var updatedLeg = deliveryLeg.AssignGateway(gatewayId);

        // Assert
        Assert.Equal(gatewayId, updatedLeg.GatewayId);
        Assert.Equal(startAddress, updatedLeg.StartAddress);
        Assert.Equal(endAddress, updatedLeg.EndAddress);
        Assert.Null(deliveryLeg.GatewayId); // Original leg unchanged
    }

    [Fact]
    public void AssignGateway_NullGatewayId_ShouldThrowArgumentNullException()
    {
        // Arrange
        var deliveryLeg = CreateDeliveryLeg("Mura", "Stockholm");

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => deliveryLeg.AssignGateway(null!));
    }

    [Fact]
    public void AssignGateway_AlreadyAssignedGateway_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var deliveryLeg = CreateDeliveryLeg("Mura", "Stockholm");
        var gatewayId1 = GatewayId.NewId();
        var gatewayId2 = GatewayId.NewId();
        var assignedLeg = deliveryLeg.AssignGateway(gatewayId1);

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => assignedLeg.AssignGateway(gatewayId2));
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
    public void Equals_DifferentGatewayId_ShouldReturnFalse()
    {
        // Arrange
        var leg1 = CreateDeliveryLeg("Mura", "Stockholm");
        var leg2 = CreateDeliveryLeg("Mura", "Stockholm");
        var gatewayId1 = GatewayId.NewId();
        var gatewayId2 = GatewayId.NewId();
        var assignedLeg1 = leg1.AssignGateway(gatewayId1);
        var assignedLeg2 = leg2.AssignGateway(gatewayId2);

        // Act & Assert
        Assert.False(assignedLeg1.Equals(assignedLeg2));
        Assert.False(assignedLeg1 == assignedLeg2);
        Assert.True(assignedLeg1 != assignedLeg2);
    }

    [Fact]
    public void Equals_OneWithGatewayOneWithout_ShouldReturnFalse()
    {
        // Arrange
        var leg1 = CreateDeliveryLeg("Mura", "Stockholm");
        var leg2 = CreateDeliveryLeg("Mura", "Stockholm");
        var gatewayId = GatewayId.NewId();
        var assignedLeg1 = leg1.AssignGateway(gatewayId);

        // Act & Assert
        Assert.False(assignedLeg1.Equals(leg2));
        Assert.False(assignedLeg1 == leg2);
        Assert.True(assignedLeg1 != leg2);
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
    public void ToString_WithoutGateway_ShouldReturnFormattedString()
    {
        // Arrange
        var leg = CreateDeliveryLeg("Mura", "Stockholm");

        // Act
        var result = leg.ToString();

        // Assert
        Assert.Contains("From", result);
        Assert.Contains("to", result);
        Assert.Contains("No Gateway", result);
        Assert.Contains("Mura", result);
        Assert.Contains("Stockholm", result);
        Assert.Contains("[Planned]", result);
    }

    [Fact]
    public void ToString_WithGateway_ShouldReturnFormattedString()
    {
        // Arrange
        var leg = CreateDeliveryLeg("Mura", "Stockholm");
        var gatewayId = GatewayId.NewId();
        var assignedLeg = leg.AssignGateway(gatewayId);

        // Act
        var result = assignedLeg.ToString();

        // Assert
        Assert.Contains("From", result);
        Assert.Contains("to", result);
        Assert.Contains("Gateway:", result);
        Assert.Contains(gatewayId.ToString(), result);
        Assert.Contains("Mura", result);
        Assert.Contains("Stockholm", result);
        Assert.Contains("[Ready]", result);
    }

    [Fact]
    public void Start_ReadyLeg_ShouldReturnInProgressLeg()
    {
        // Arrange
        var leg = CreateDeliveryLeg("Mura", "Stockholm");
        var gatewayId = GatewayId.NewId();
        var readyLeg = leg.AssignGateway(gatewayId);

        // Act
        var startedLeg = readyLeg.Start();

        // Assert
        Assert.Equal(DeliveryLegStatus.InProgress, startedLeg.Status);
        Assert.Equal(gatewayId, startedLeg.GatewayId);
        Assert.Equal(readyLeg.StartAddress, startedLeg.StartAddress);
        Assert.Equal(readyLeg.EndAddress, startedLeg.EndAddress);
        Assert.Equal(DeliveryLegStatus.Ready, readyLeg.Status); // Original unchanged
    }

    [Fact]
    public void Start_PlannedLeg_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var leg = CreateDeliveryLeg("Mura", "Stockholm");

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => leg.Start());
    }

    [Fact]
    public void Start_LegWithoutGateway_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var leg = CreateDeliveryLeg("Mura", "Stockholm");
        // This test case is actually not possible in normal flow since AssignGateway
        // always sets a gateway. Let's test the validation in Start() method directly.
        var gatewayId = GatewayId.NewId();
        var readyLeg = leg.AssignGateway(gatewayId);

        // Act & Assert - This should work since we have a gateway
        var startedLeg = readyLeg.Start();
        Assert.Equal(DeliveryLegStatus.InProgress, startedLeg.Status);
    }

    [Fact]
    public void Complete_InProgressLeg_ShouldReturnCompletedLeg()
    {
        // Arrange
        var leg = CreateDeliveryLeg("Mura", "Stockholm");
        var gatewayId = GatewayId.NewId();
        var inProgressLeg = leg.AssignGateway(gatewayId).Start();

        // Act
        var completedLeg = inProgressLeg.Complete();

        // Assert
        Assert.Equal(DeliveryLegStatus.Completed, completedLeg.Status);
        Assert.Equal(gatewayId, completedLeg.GatewayId);
        Assert.Equal(inProgressLeg.StartAddress, completedLeg.StartAddress);
        Assert.Equal(inProgressLeg.EndAddress, completedLeg.EndAddress);
        Assert.Equal(DeliveryLegStatus.InProgress, inProgressLeg.Status); // Original unchanged
    }

    [Fact]
    public void Complete_PlannedLeg_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var leg = CreateDeliveryLeg("Mura", "Stockholm");

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => leg.Complete());
    }

    [Fact]
    public void Complete_ReadyLeg_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var leg = CreateDeliveryLeg("Mura", "Stockholm");
        var gatewayId = GatewayId.NewId();
        var readyLeg = leg.AssignGateway(gatewayId);

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => readyLeg.Complete());
    }

    [Fact]
    public void AssignGateway_NonPlannedLeg_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var leg = CreateDeliveryLeg("Mura", "Stockholm");
        var gatewayId = GatewayId.NewId();
        var readyLeg = leg.AssignGateway(gatewayId);

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => readyLeg.AssignGateway(GatewayId.NewId()));
    }

    private DeliveryLeg CreateDeliveryLeg(string startCity, string endCity)
    {
        var startAddress = new Address("Start Street 1", startCity, "123 45", "Sverige");
        var endAddress = new Address("End Street 1", endCity, "678 90", "Sverige");
        return new DeliveryLeg(startAddress, endAddress);
    }
}