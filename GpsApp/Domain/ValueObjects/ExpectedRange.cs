namespace GpsApp.Domain.ValueObjects;

/// <summary>
/// Value object for the expected range of a T value (temperature or humidity)
/// Implements IEquatable&lt;ExpectedRange&lt;T&gt;&gt; for comparison
/// </summary>
public class ExpectedRange<T> where T : IComparable<T>
{
    public T Minimum { get; private set; }
    public T Maximum { get; private set; }
    
    // Alias properties for backward compatibility
    public T Min => Minimum;
    public T Max => Maximum;

    public ExpectedRange(T minimum, T maximum)
    {
        if (minimum.CompareTo(maximum) > 0)
        {
            throw new ArgumentException("Minimum cannot be greater than maximum");
        }
        Minimum = minimum;
        Maximum = maximum;
    }

    /// <summary>
    /// Creates an expected range from persistence storage (hydration constructor)
    /// This constructor allows setting all properties for reconstruction from storage
    /// </summary>
    public ExpectedRange(T minimum, T maximum, bool skipValidation = false)
    {
        if (!skipValidation && minimum.CompareTo(maximum) > 0)
        {
            throw new ArgumentException("Minimum cannot be greater than maximum");
        }
        Minimum = minimum;
        Maximum = maximum;
    }

    public bool IsInRange(T value) => value.CompareTo(Minimum) >= 0 && value.CompareTo(Maximum) <= 0;

    public override string ToString() => $"{Minimum.ToString()} - {Maximum.ToString()}";

    public override bool Equals(object? obj) => obj is ExpectedRange<T> range && Minimum.CompareTo(range.Minimum) == 0 && Maximum.CompareTo(range.Maximum) == 0;

    public override int GetHashCode() => HashCode.Combine(Minimum, Maximum);

    public bool Equals(ExpectedRange<T>? other) => other is not null && Minimum.CompareTo(other.Minimum) == 0 && Maximum.CompareTo(other.Maximum) == 0;

    public static bool operator ==(ExpectedRange<T>? left, ExpectedRange<T>? right) => left?.Equals(right) ?? right is null;

    public static bool operator !=(ExpectedRange<T>? left, ExpectedRange<T>? right) => !(left == right);
}