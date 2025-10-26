namespace GpsApp.Domain.ValueObjects;

/// <summary>
/// Interface for business value objects that can be used in ExpectedRange
/// Ensures only business types (Temperature, Humidity) can be used as generic parameters
/// </summary>
public interface IBusinessValue : IComparable<IBusinessValue>, IEquatable<IBusinessValue>
{
    decimal Value { get; }

    /// <summary>
    /// Gets the unit of measurement for this business value (e.g., "°C", "%")
    /// </summary>
    string Unit { get; }
}
