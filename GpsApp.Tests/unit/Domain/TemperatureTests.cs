using GpsApp.Domain.ValueObjects;

namespace GpsApp.Tests.Unit.Domain;

public class TemperatureTests
{
    [Fact]
    public void Constructor_ValidTemperature_ShouldCreateInstance()
    {
        // Arrange
        var temperature = 25.5m;

        // Act
        var result = new Temperature(temperature);

        // Assert
        Assert.Equal(temperature, result.Value);
        Assert.Equal("°C", result.Unit);
    }

    [Fact]
    public void Constructor_TemperatureBelowAbsoluteZero_ShouldThrowArgumentException()
    {
        // Arrange
        var invalidTemperature = -300m;

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => new Temperature(invalidTemperature));
        Assert.Contains("Temperature cannot be less than -273.15°C", exception.Message);
    }

    [Fact]
    public void Constructor_AbsoluteZero_ShouldCreateInstance()
    {
        // Arrange
        var absoluteZero = -273.15m;

        // Act
        var result = new Temperature(absoluteZero);

        // Assert
        Assert.Equal(absoluteZero, result.Value);
    }

    [Fact]
    public void ToString_ShouldReturnFormattedString()
    {
        // Arrange
        var temperature = new Temperature(25.5m);

        // Act
        var result = temperature.ToString();

        // Assert
        Assert.Equal("25.5°C", result);
    }

    [Fact]
    public void Equals_SameTemperature_ShouldReturnTrue()
    {
        // Arrange
        var temperature1 = new Temperature(25.5m);
        var temperature2 = new Temperature(25.5m);

        // Act
        var result = temperature1.Equals(temperature2);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void Equals_DifferentTemperature_ShouldReturnFalse()
    {
        // Arrange
        var temperature1 = new Temperature(25.5m);
        var temperature2 = new Temperature(30.0m);

        // Act
        var result = temperature1.Equals(temperature2);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void Equals_NullTemperature_ShouldReturnFalse()
    {
        // Arrange
        var temperature = new Temperature(25.5m);

        // Act
        var result = temperature.Equals((Temperature?)null);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void EqualsOperator_SameTemperature_ShouldReturnTrue()
    {
        // Arrange
        var temperature1 = new Temperature(25.5m);
        var temperature2 = new Temperature(25.5m);

        // Act
        var result = temperature1 == temperature2;

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void NotEqualsOperator_DifferentTemperature_ShouldReturnTrue()
    {
        // Arrange
        var temperature1 = new Temperature(25.5m);
        var temperature2 = new Temperature(30.0m);

        // Act
        var result = temperature1 != temperature2;

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void CompareTo_SameTemperature_ShouldReturnZero()
    {
        // Arrange
        var temperature1 = new Temperature(25.5m);
        var temperature2 = new Temperature(25.5m);

        // Act
        var result = temperature1.CompareTo(temperature2);

        // Assert
        Assert.Equal(0, result);
    }

    [Fact]
    public void CompareTo_LowerTemperature_ShouldReturnPositive()
    {
        // Arrange
        var temperature1 = new Temperature(30.0m);
        var temperature2 = new Temperature(25.5m);

        // Act
        var result = temperature1.CompareTo(temperature2);

        // Assert
        Assert.True(result > 0);
    }

    [Fact]
    public void CompareTo_HigherTemperature_ShouldReturnNegative()
    {
        // Arrange
        var temperature1 = new Temperature(25.5m);
        var temperature2 = new Temperature(30.0m);

        // Act
        var result = temperature1.CompareTo(temperature2);

        // Assert
        Assert.True(result < 0);
    }

    [Fact]
    public void CompareTo_NullTemperature_ShouldReturnPositive()
    {
        // Arrange
        var temperature = new Temperature(25.5m);

        // Act
        var result = temperature.CompareTo((Temperature?)null);

        // Assert
        Assert.Equal(1, result);
    }

    [Fact]
    public void LessThanOperator_LowerTemperature_ShouldReturnTrue()
    {
        // Arrange
        var temperature1 = new Temperature(25.5m);
        var temperature2 = new Temperature(30.0m);

        // Act
        var result = temperature1 < temperature2;

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void LessThanOrEqualOperator_SameTemperature_ShouldReturnTrue()
    {
        // Arrange
        var temperature1 = new Temperature(25.5m);
        var temperature2 = new Temperature(25.5m);

        // Act
        var result = temperature1 <= temperature2;

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void GreaterThanOperator_HigherTemperature_ShouldReturnTrue()
    {
        // Arrange
        var temperature1 = new Temperature(30.0m);
        var temperature2 = new Temperature(25.5m);

        // Act
        var result = temperature1 > temperature2;

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void GreaterThanOrEqualOperator_SameTemperature_ShouldReturnTrue()
    {
        // Arrange
        var temperature1 = new Temperature(25.5m);
        var temperature2 = new Temperature(25.5m);

        // Act
        var result = temperature1 >= temperature2;

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void GetHashCode_SameTemperature_ShouldReturnSameHashCode()
    {
        // Arrange
        var temperature1 = new Temperature(25.5m);
        var temperature2 = new Temperature(25.5m);

        // Act
        var hashCode1 = temperature1.GetHashCode();
        var hashCode2 = temperature2.GetHashCode();

        // Assert
        Assert.Equal(hashCode1, hashCode2);
    }

    [Fact]
    public void GetHashCode_DifferentTemperature_ShouldReturnDifferentHashCode()
    {
        // Arrange
        var temperature1 = new Temperature(25.5m);
        var temperature2 = new Temperature(30.0m);

        // Act
        var hashCode1 = temperature1.GetHashCode();
        var hashCode2 = temperature2.GetHashCode();

        // Assert
        Assert.NotEqual(hashCode1, hashCode2);
    }

    [Fact]
    public void Equals_IBusinessValue_SameTemperature_ShouldReturnTrue()
    {
        // Arrange
        IBusinessValue temperature1 = new Temperature(25.5m);
        IBusinessValue temperature2 = new Temperature(25.5m);

        // Act
        var result = temperature1.Equals(temperature2);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void CompareTo_IBusinessValue_SameTemperature_ShouldReturnZero()
    {
        // Arrange
        IBusinessValue temperature1 = new Temperature(25.5m);
        IBusinessValue temperature2 = new Temperature(25.5m);

        // Act
        var result = temperature1.CompareTo(temperature2);

        // Assert
        Assert.Equal(0, result);
    }

    [Fact]
    public void CompareTo_IBusinessValue_DifferentType_ShouldThrowArgumentException()
    {
        // Arrange
        IBusinessValue temperature = new Temperature(25.5m);
        IBusinessValue humidity = new Humidity(50m);

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => temperature.CompareTo(humidity));
        Assert.Contains("Cannot compare Temperature with Humidity", exception.Message);
    }
}
