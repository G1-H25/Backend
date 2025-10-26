namespace GpsApp.Domain.ValueObjects;

/// <summary>
/// Value object for the temperature measurement in degrees Celsius
/// Implements IEquatable&lt;Temperature&gt;, IComparable&lt;Temperature&gt;, and IBusinessValue for comparison
/// </summary>
public class Temperature : IEquatable<Temperature>, IComparable<Temperature>, IBusinessValue
{
    public decimal Value { get; private set; }
    public string Unit => "°C";

    public Temperature(decimal value)
    {
        if (value < -273.15m)
        {
            throw new ArgumentException("Temperature cannot be less than -273.15°C");
        }
        Value = value;
    }

    /// <summary>
    /// Creates a temperature from persistence storage (hydration constructor)
    /// This constructor allows setting the value for reconstruction from storage
    /// </summary>
    public Temperature(decimal value, bool skipValidation = false)
    {
        if (!skipValidation && value < -273.15m)
        {
            throw new ArgumentException("Temperature cannot be less than -273.15°C");
        }
        Value = value;
    }

    public override string ToString() => $"{Value.ToString(System.Globalization.CultureInfo.InvariantCulture)}°C";

    public override bool Equals(object? obj) => obj is Temperature temperature && Value == temperature.Value;

    public override int GetHashCode() => Value.GetHashCode();

    public bool Equals(Temperature? other) => other is not null && Value == other.Value;

    public int CompareTo(Temperature? other) => other is null ? 1 : Value.CompareTo(other.Value);

    public int CompareTo(IBusinessValue? other) => other is Temperature temperature ? CompareTo(temperature) : throw new ArgumentException($"Cannot compare Temperature with {other?.GetType().Name ?? "null"}");

    public bool Equals(IBusinessValue? other) => other is Temperature temperature && Equals(temperature);

    public static bool operator ==(Temperature? left, Temperature? right) => left?.Equals(right) ?? right is null;

    public static bool operator !=(Temperature? left, Temperature? right) => !(left == right);

    public static bool operator <(Temperature? left, Temperature? right) => left is null ? right is not null : left.CompareTo(right) < 0;

    public static bool operator <=(Temperature? left, Temperature? right) => left is null || left.CompareTo(right) <= 0;

    public static bool operator >(Temperature? left, Temperature? right) => left is not null && left.CompareTo(right) > 0;

    public static bool operator >=(Temperature? left, Temperature? right) => left is null ? right is null : left.CompareTo(right) >= 0;
}
