namespace GpsApp.Domain.ValueObjects;

/// <summary>
/// Value object representing a physical address
/// Implements IEquatable&lt;Address&gt; for comparison
/// </summary>
public class Address : IEquatable<Address>
{
    public string Street { get; private set; }
    public string City { get; private set; }
    public string PostalCode { get; private set; }
    public string Country { get; private set; }

    public Address(string street, string city, string postalCode, string country)
    {
        if (string.IsNullOrWhiteSpace(street))
            throw new ArgumentException("Street cannot be null or empty", nameof(street));
        if (string.IsNullOrWhiteSpace(city))
            throw new ArgumentException("City cannot be null or empty", nameof(city));
        if (string.IsNullOrWhiteSpace(postalCode))
            throw new ArgumentException("Postal code cannot be null or empty", nameof(postalCode));
        if (string.IsNullOrWhiteSpace(country))
            throw new ArgumentException("Country cannot be null or empty", nameof(country));

        Street = street.Trim();
        City = city.Trim();
        PostalCode = postalCode.Trim();
        Country = country.Trim();
    }

    // Parameterless constructor for EF Core
    private Address()
    {
        Street = string.Empty;
        City = string.Empty;
        PostalCode = string.Empty;
        Country = string.Empty;
    }

    /// <summary>
    /// Creates an address from persistence storage (hydration constructor)
    /// This constructor allows setting all properties for reconstruction from storage
    /// </summary>
    public Address(string street, string city, string postalCode, string country, bool skipValidation = false)
    {
        if (!skipValidation)
        {
            if (string.IsNullOrWhiteSpace(street))
                throw new ArgumentException("Street cannot be null or empty", nameof(street));
            if (string.IsNullOrWhiteSpace(city))
                throw new ArgumentException("City cannot be null or empty", nameof(city));
            if (string.IsNullOrWhiteSpace(postalCode))
                throw new ArgumentException("Postal code cannot be null or empty", nameof(postalCode));
            if (string.IsNullOrWhiteSpace(country))
                throw new ArgumentException("Country cannot be null or empty", nameof(country));
        }

        Street = street?.Trim() ?? string.Empty;
        City = city?.Trim() ?? string.Empty;
        PostalCode = postalCode?.Trim() ?? string.Empty;
        Country = country?.Trim() ?? string.Empty;
    }

    public override string ToString() => $"{Street}, {City}, {PostalCode}, {Country}";

    public override bool Equals(object? obj) => obj is Address address && Equals(address);

    public override int GetHashCode() => HashCode.Combine(Street, City, PostalCode, Country);

    public bool Equals(Address? other) => other is not null && 
        Street == other.Street && 
        City == other.City && 
        PostalCode == other.PostalCode && 
        Country == other.Country;

    public static bool operator ==(Address? left, Address? right) => left?.Equals(right) ?? right is null;

    public static bool operator !=(Address? left, Address? right) => !(left == right);
}
