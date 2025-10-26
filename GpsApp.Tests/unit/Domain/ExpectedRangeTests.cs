using GpsApp.Domain.ValueObjects;

namespace GpsApp.Tests.Unit.Domain;

public class ExpectedRangeTests
{
    [Fact]
    public void Constructor_ValidRange_ShouldCreateInstance()
    {
        // Arrange
        var minimum = new Temperature(20m);
        var maximum = new Temperature(25m);

        // Act
        var result = new ExpectedRange<Temperature>(minimum, maximum);

        // Assert
        Assert.Equal(minimum, result.Minimum);
        Assert.Equal(maximum, result.Maximum);
    }

    [Fact]
    public void Constructor_MinimumGreaterThanMaximum_ShouldThrowArgumentException()
    {
        // Arrange
        var minimum = new Temperature(30m);
        var maximum = new Temperature(25m);

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => new ExpectedRange<Temperature>(minimum, maximum));
        Assert.Contains("Minimum cannot be greater than maximum", exception.Message);
    }

    [Fact]
    public void Constructor_SameMinimumAndMaximum_ShouldCreateInstance()
    {
        // Arrange
        var temperature = new Temperature(25m);

        // Act
        var result = new ExpectedRange<Temperature>(temperature, temperature);

        // Assert
        Assert.Equal(temperature, result.Minimum);
        Assert.Equal(temperature, result.Maximum);
    }

    [Fact]
    public void IsInRange_ValueWithinRange_ShouldReturnTrue()
    {
        // Arrange
        var range = new ExpectedRange<Temperature>(new Temperature(20m), new Temperature(25m));
        var value = new Temperature(22.5m);

        // Act
        var result = range.IsInRange(value);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IsInRange_ValueAtMinimum_ShouldReturnTrue()
    {
        // Arrange
        var range = new ExpectedRange<Temperature>(new Temperature(20m), new Temperature(25m));
        var value = new Temperature(20m);

        // Act
        var result = range.IsInRange(value);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IsInRange_ValueAtMaximum_ShouldReturnTrue()
    {
        // Arrange
        var range = new ExpectedRange<Temperature>(new Temperature(20m), new Temperature(25m));
        var value = new Temperature(25m);

        // Act
        var result = range.IsInRange(value);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IsInRange_ValueBelowMinimum_ShouldReturnFalse()
    {
        // Arrange
        var range = new ExpectedRange<Temperature>(new Temperature(20m), new Temperature(25m));
        var value = new Temperature(18m);

        // Act
        var result = range.IsInRange(value);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void IsInRange_ValueAboveMaximum_ShouldReturnFalse()
    {
        // Arrange
        var range = new ExpectedRange<Temperature>(new Temperature(20m), new Temperature(25m));
        var value = new Temperature(27m);

        // Act
        var result = range.IsInRange(value);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void ToString_ShouldReturnFormattedString()
    {
        // Arrange
        var range = new ExpectedRange<Temperature>(new Temperature(20m), new Temperature(25m));

        // Act
        var result = range.ToString();

        // Assert
        Assert.Equal("20°C - 25°C", result);
    }

    [Fact]
    public void Equals_SameRange_ShouldReturnTrue()
    {
        // Arrange
        var range1 = new ExpectedRange<Temperature>(new Temperature(20m), new Temperature(25m));
        var range2 = new ExpectedRange<Temperature>(new Temperature(20m), new Temperature(25m));

        // Act
        var result = range1.Equals(range2);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void Equals_DifferentRange_ShouldReturnFalse()
    {
        // Arrange
        var range1 = new ExpectedRange<Temperature>(new Temperature(20m), new Temperature(25m));
        var range2 = new ExpectedRange<Temperature>(new Temperature(22m), new Temperature(27m));

        // Act
        var result = range1.Equals(range2);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void Equals_NullRange_ShouldReturnFalse()
    {
        // Arrange
        var range = new ExpectedRange<Temperature>(new Temperature(20m), new Temperature(25m));

        // Act
        var result = range.Equals((ExpectedRange<Temperature>?)null);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void EqualsOperator_SameRange_ShouldReturnTrue()
    {
        // Arrange
        var range1 = new ExpectedRange<Temperature>(new Temperature(20m), new Temperature(25m));
        var range2 = new ExpectedRange<Temperature>(new Temperature(20m), new Temperature(25m));

        // Act
        var result = range1 == range2;

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void NotEqualsOperator_DifferentRange_ShouldReturnTrue()
    {
        // Arrange
        var range1 = new ExpectedRange<Temperature>(new Temperature(20m), new Temperature(25m));
        var range2 = new ExpectedRange<Temperature>(new Temperature(22m), new Temperature(27m));

        // Act
        var result = range1 != range2;

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void GetHashCode_SameRange_ShouldReturnSameHashCode()
    {
        // Arrange
        var range1 = new ExpectedRange<Temperature>(new Temperature(20m), new Temperature(25m));
        var range2 = new ExpectedRange<Temperature>(new Temperature(20m), new Temperature(25m));

        // Act
        var hashCode1 = range1.GetHashCode();
        var hashCode2 = range2.GetHashCode();

        // Assert
        Assert.Equal(hashCode1, hashCode2);
    }

    [Fact]
    public void GetHashCode_DifferentRange_ShouldReturnDifferentHashCode()
    {
        // Arrange
        var range1 = new ExpectedRange<Temperature>(new Temperature(20m), new Temperature(25m));
        var range2 = new ExpectedRange<Temperature>(new Temperature(22m), new Temperature(27m));

        // Act
        var hashCode1 = range1.GetHashCode();
        var hashCode2 = range2.GetHashCode();

        // Assert
        Assert.NotEqual(hashCode1, hashCode2);
    }

    [Fact]
    public void ExpectedRange_WithHumidity_ShouldWorkCorrectly()
    {
        // Arrange
        var range = new ExpectedRange<Humidity>(new Humidity(40m), new Humidity(60m));
        var valueInRange = new Humidity(50m);
        var valueOutOfRange = new Humidity(70m);

        // Act
        var inRangeResult = range.IsInRange(valueInRange);
        var outOfRangeResult = range.IsInRange(valueOutOfRange);

        // Assert
        Assert.True(inRangeResult);
        Assert.False(outOfRangeResult);
        Assert.Equal("40% - 60%", range.ToString());
    }

    [Fact]
    public void ExpectedRange_WithDecimal_ShouldWorkCorrectly()
    {
        // Arrange
        var range = new ExpectedRange<decimal>(10.5m, 20.5m);
        var valueInRange = 15.0m;
        var valueOutOfRange = 25.0m;

        // Act
        var inRangeResult = range.IsInRange(valueInRange);
        var outOfRangeResult = range.IsInRange(valueOutOfRange);

        // Assert
        Assert.True(inRangeResult);
        Assert.False(outOfRangeResult);
        Assert.Equal("10,5 - 20,5", range.ToString());
    }

    [Fact]
    public void ExpectedRange_WithInt_ShouldWorkCorrectly()
    {
        // Arrange
        var range = new ExpectedRange<int>(10, 20);
        var valueInRange = 15;
        var valueOutOfRange = 25;

        // Act
        var inRangeResult = range.IsInRange(valueInRange);
        var outOfRangeResult = range.IsInRange(valueOutOfRange);

        // Assert
        Assert.True(inRangeResult);
        Assert.False(outOfRangeResult);
        Assert.Equal("10 - 20", range.ToString());
    }
}
