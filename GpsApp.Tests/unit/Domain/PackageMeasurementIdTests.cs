using GpsApp.Domain.ValueObjects;

namespace GpsApp.Tests.Unit.Domain;

public class PackageMeasurementIdTests
{
    [Fact]
    public void Constructor_ValidGuid_ShouldCreatePackageMeasurementId()
    {
        // Arrange
        var guid = Guid.NewGuid();

        // Act
        var id = new PackageMeasurementId(guid);

        // Assert
        Assert.Equal(guid, id.Value);
    }

    [Fact]
    public void Constructor_EmptyGuid_ShouldThrowArgumentException()
    {
        // Arrange
        var emptyGuid = Guid.Empty;

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => new PackageMeasurementId(emptyGuid));
        Assert.Contains("PackageMeasurementId cannot be empty", exception.Message);
    }

    [Fact]
    public void NewId_ShouldCreateUniqueIds()
    {
        // Act
        var id1 = PackageMeasurementId.NewId();
        var id2 = PackageMeasurementId.NewId();

        // Assert
        Assert.NotEqual(id1, id2);
        Assert.NotEqual(Guid.Empty, id1.Value);
        Assert.NotEqual(Guid.Empty, id2.Value);
    }

    [Fact]
    public void FromString_ValidGuidString_ShouldCreatePackageMeasurementId()
    {
        // Arrange
        var guid = Guid.NewGuid();
        var guidString = guid.ToString();

        // Act
        var id = PackageMeasurementId.FromString(guidString);

        // Assert
        Assert.Equal(guid, id.Value);
    }

    [Fact]
    public void FromString_InvalidGuidString_ShouldThrowArgumentException()
    {
        // Arrange
        var invalidString = "not-a-guid";

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => PackageMeasurementId.FromString(invalidString));
        Assert.Contains("Invalid GUID format", exception.Message);
    }

    [Fact]
    public void FromString_NullOrEmptyString_ShouldThrowArgumentException()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => PackageMeasurementId.FromString(null!));
        Assert.Throws<ArgumentException>(() => PackageMeasurementId.FromString(""));
        Assert.Throws<ArgumentException>(() => PackageMeasurementId.FromString("   "));
    }

    [Fact]
    public void ToString_ShouldReturnGuidString()
    {
        // Arrange
        var guid = Guid.NewGuid();
        var id = new PackageMeasurementId(guid);

        // Act
        var result = id.ToString();

        // Assert
        Assert.Equal(guid.ToString(), result);
    }

    [Fact]
    public void Equals_SameGuid_ShouldReturnTrue()
    {
        // Arrange
        var guid = Guid.NewGuid();
        var id1 = new PackageMeasurementId(guid);
        var id2 = new PackageMeasurementId(guid);

        // Act & Assert
        Assert.True(id1.Equals(id2));
        Assert.True(id1 == id2);
        Assert.False(id1 != id2);
        Assert.Equal(id1.GetHashCode(), id2.GetHashCode());
    }

    [Fact]
    public void Equals_DifferentGuid_ShouldReturnFalse()
    {
        // Arrange
        var id1 = PackageMeasurementId.NewId();
        var id2 = PackageMeasurementId.NewId();

        // Act & Assert
        Assert.False(id1.Equals(id2));
        Assert.False(id1 == id2);
        Assert.True(id1 != id2);
    }

    [Fact]
    public void Equals_Null_ShouldReturnFalse()
    {
        // Arrange
        var id = PackageMeasurementId.NewId();

        // Act & Assert
        Assert.False(id.Equals(null));
        Assert.False(id == null);
        Assert.True(id != null);
    }

    [Fact]
    public void ImplicitConversion_ToGuid_ShouldWork()
    {
        // Arrange
        var guid = Guid.NewGuid();
        var id = new PackageMeasurementId(guid);

        // Act
        Guid convertedGuid = id;

        // Assert
        Assert.Equal(guid, convertedGuid);
    }
}
