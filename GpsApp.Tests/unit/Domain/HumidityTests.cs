using GpsApp.Domain.ValueObjects;

namespace GpsApp.Tests.Unit.Domain;

public class HumidityTests
{
    [Fact]
    public void Constructor_ValidHumidity_ShouldCreateInstance()
    {
        // Arrange
        var humidity = 65.5m;

        // Act
        var result = new Humidity(humidity);

        // Assert
        Assert.Equal(humidity, result.Value);
        Assert.Equal("%", result.Unit);
    }

    [Fact]
    public void Constructor_HumidityBelowZero_ShouldThrowArgumentException()
    {
        // Arrange
        var invalidHumidity = -10m;

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => new Humidity(invalidHumidity));
        Assert.Contains("Humidity must be between 0% and 100%", exception.Message);
    }

    [Fact]
    public void Constructor_HumidityAbove100_ShouldThrowArgumentException()
    {
        // Arrange
        var invalidHumidity = 150m;

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => new Humidity(invalidHumidity));
        Assert.Contains("Humidity must be between 0% and 100%", exception.Message);
    }

    [Fact]
    public void Constructor_ZeroHumidity_ShouldCreateInstance()
    {
        // Arrange
        var humidity = 0m;

        // Act
        var result = new Humidity(humidity);

        // Assert
        Assert.Equal(humidity, result.Value);
    }

    [Fact]
    public void Constructor_100PercentHumidity_ShouldCreateInstance()
    {
        // Arrange
        var humidity = 100m;

        // Act
        var result = new Humidity(humidity);

        // Assert
        Assert.Equal(humidity, result.Value);
    }

    [Fact]
    public void ToString_ShouldReturnFormattedString()
    {
        // Arrange
        var humidity = new Humidity(65.5m);

        // Act
        var result = humidity.ToString();

        // Assert
        Assert.Equal("65.5%", result);
    }

    [Fact]
    public void Equals_SameHumidity_ShouldReturnTrue()
    {
        // Arrange
        var humidity1 = new Humidity(65.5m);
        var humidity2 = new Humidity(65.5m);

        // Act
        var result = humidity1.Equals(humidity2);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void Equals_DifferentHumidity_ShouldReturnFalse()
    {
        // Arrange
        var humidity1 = new Humidity(65.5m);
        var humidity2 = new Humidity(70.0m);

        // Act
        var result = humidity1.Equals(humidity2);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void Equals_NullHumidity_ShouldReturnFalse()
    {
        // Arrange
        var humidity = new Humidity(65.5m);

        // Act
        var result = humidity.Equals((Humidity?)null);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void EqualsOperator_SameHumidity_ShouldReturnTrue()
    {
        // Arrange
        var humidity1 = new Humidity(65.5m);
        var humidity2 = new Humidity(65.5m);

        // Act
        var result = humidity1 == humidity2;

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void NotEqualsOperator_DifferentHumidity_ShouldReturnTrue()
    {
        // Arrange
        var humidity1 = new Humidity(65.5m);
        var humidity2 = new Humidity(70.0m);

        // Act
        var result = humidity1 != humidity2;

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void CompareTo_SameHumidity_ShouldReturnZero()
    {
        // Arrange
        var humidity1 = new Humidity(65.5m);
        var humidity2 = new Humidity(65.5m);

        // Act
        var result = humidity1.CompareTo(humidity2);

        // Assert
        Assert.Equal(0, result);
    }

    [Fact]
    public void CompareTo_LowerHumidity_ShouldReturnPositive()
    {
        // Arrange
        var humidity1 = new Humidity(70.0m);
        var humidity2 = new Humidity(65.5m);

        // Act
        var result = humidity1.CompareTo(humidity2);

        // Assert
        Assert.True(result > 0);
    }

    [Fact]
    public void CompareTo_HigherHumidity_ShouldReturnNegative()
    {
        // Arrange
        var humidity1 = new Humidity(65.5m);
        var humidity2 = new Humidity(70.0m);

        // Act
        var result = humidity1.CompareTo(humidity2);

        // Assert
        Assert.True(result < 0);
    }

    [Fact]
    public void CompareTo_NullHumidity_ShouldReturnPositive()
    {
        // Arrange
        var humidity = new Humidity(65.5m);

        // Act
        var result = humidity.CompareTo((Humidity?)null);

        // Assert
        Assert.Equal(1, result);
    }

    [Fact]
    public void LessThanOperator_LowerHumidity_ShouldReturnTrue()
    {
        // Arrange
        var humidity1 = new Humidity(65.5m);
        var humidity2 = new Humidity(70.0m);

        // Act
        var result = humidity1 < humidity2;

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void LessThanOrEqualOperator_SameHumidity_ShouldReturnTrue()
    {
        // Arrange
        var humidity1 = new Humidity(65.5m);
        var humidity2 = new Humidity(65.5m);

        // Act
        var result = humidity1 <= humidity2;

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void GreaterThanOperator_HigherHumidity_ShouldReturnTrue()
    {
        // Arrange
        var humidity1 = new Humidity(70.0m);
        var humidity2 = new Humidity(65.5m);

        // Act
        var result = humidity1 > humidity2;

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void GreaterThanOrEqualOperator_SameHumidity_ShouldReturnTrue()
    {
        // Arrange
        var humidity1 = new Humidity(65.5m);
        var humidity2 = new Humidity(65.5m);

        // Act
        var result = humidity1 >= humidity2;

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void GetHashCode_SameHumidity_ShouldReturnSameHashCode()
    {
        // Arrange
        var humidity1 = new Humidity(65.5m);
        var humidity2 = new Humidity(65.5m);

        // Act
        var hashCode1 = humidity1.GetHashCode();
        var hashCode2 = humidity2.GetHashCode();

        // Assert
        Assert.Equal(hashCode1, hashCode2);
    }

    [Fact]
    public void GetHashCode_DifferentHumidity_ShouldReturnDifferentHashCode()
    {
        // Arrange
        var humidity1 = new Humidity(65.5m);
        var humidity2 = new Humidity(70.0m);

        // Act
        var hashCode1 = humidity1.GetHashCode();
        var hashCode2 = humidity2.GetHashCode();

        // Assert
        Assert.NotEqual(hashCode1, hashCode2);
    }

    [Fact]
    public void Equals_IBusinessValue_SameHumidity_ShouldReturnTrue()
    {
        // Arrange
        IBusinessValue humidity1 = new Humidity(65.5m);
        IBusinessValue humidity2 = new Humidity(65.5m);

        // Act
        var result = humidity1.Equals(humidity2);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void CompareTo_IBusinessValue_SameHumidity_ShouldReturnZero()
    {
        // Arrange
        IBusinessValue humidity1 = new Humidity(65.5m);
        IBusinessValue humidity2 = new Humidity(65.5m);

        // Act
        var result = humidity1.CompareTo(humidity2);

        // Assert
        Assert.Equal(0, result);
    }

    [Fact]
    public void CompareTo_IBusinessValue_DifferentType_ShouldThrowArgumentException()
    {
        // Arrange
        IBusinessValue humidity = new Humidity(65.5m);
        IBusinessValue temperature = new Temperature(25.5m);

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => humidity.CompareTo(temperature));
        Assert.Contains("Cannot compare Humidity with Temperature", exception.Message);
    }
}
