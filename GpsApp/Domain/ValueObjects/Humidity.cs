namespace GpsApp.Domain.ValueObjects;

/// <summary>
/// Value object for the humidity measurement in percentage
/// Implements IEquatable&lt;Humidity&gt; and IComparable&lt;Humidity&gt; for comparison
/// </summary>
public class Humidity : IEquatable<Humidity>, IComparable<Humidity>, IBusinessValue
{
    public decimal Value { get; private set; }
    public string Unit => "%";

    public Humidity(decimal value)
    {
        if (value < 0m || value > 100m)
        {
            throw new ArgumentException("Humidity must be between 0% and 100%");
        }
        Value = value;
    }

    /// <summary>
    /// Creates a humidity from persistence storage (hydration constructor)
    /// This constructor allows setting the value for reconstruction from storage
    /// </summary>
    public Humidity(decimal value, bool skipValidation = false)
    {
        if (!skipValidation && (value < 0m || value > 100m))
        {
            throw new ArgumentException("Humidity must be between 0% and 100%");
        }
        Value = value;
    }

    public override string ToString() => $"{Value.ToString(System.Globalization.CultureInfo.InvariantCulture)}%";

    public override bool Equals(object? obj) => obj is Humidity humidity && Value == humidity.Value;

    public override int GetHashCode() => Value.GetHashCode();

    public bool Equals(Humidity? other) => other is not null && Value == other.Value;

    public int CompareTo(Humidity? other) => other is null ? 1 : Value.CompareTo(other.Value);

    public int CompareTo(IBusinessValue? other) => other is Humidity humidity ? CompareTo(humidity) : throw new ArgumentException($"Cannot compare Humidity with {other?.GetType().Name ?? "null"}");

    public bool Equals(IBusinessValue? other) => other is Humidity humidity && Equals(humidity);

    public static bool operator ==(Humidity? left, Humidity? right) => left?.Equals(right) ?? right is null;

    public static bool operator !=(Humidity? left, Humidity? right) => !(left == right);

    public static bool operator <(Humidity? left, Humidity? right) => left is null ? right is not null : left.CompareTo(right) < 0;

    public static bool operator <=(Humidity? left, Humidity? right) => left is null || left.CompareTo(right) <= 0;

    public static bool operator >(Humidity? left, Humidity? right) => left is not null && left.CompareTo(right) > 0;

    public static bool operator >=(Humidity? left, Humidity? right) => left is null ? right is null : left.CompareTo(right) >= 0;
}