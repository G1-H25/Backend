namespace GpsApp.Domain.ValueObjects;

/// <summary>
/// GPS coordinate value object representing latitude and longitude
/// </summary>
public class GpsCoordinate : IEquatable<GpsCoordinate>
{
    public decimal Latitude { get; private set; }
    public decimal Longitude { get; private set; }

    public GpsCoordinate(decimal latitude, decimal longitude)
    {
        if (latitude < -90 || latitude > 90)
            throw new ArgumentException("Latitude must be between -90 and 90 degrees", nameof(latitude));
        
        if (longitude < -180 || longitude > 180)
            throw new ArgumentException("Longitude must be between -180 and 180 degrees", nameof(longitude));

        Latitude = latitude;
        Longitude = longitude;
    }

    /// <summary>
    /// Creates a GPS coordinate from persistence storage (hydration constructor)
    /// </summary>
    public GpsCoordinate(decimal latitude, decimal longitude, bool skipValidation = false)
    {
        if (!skipValidation)
        {
            if (latitude < -90 || latitude > 90)
                throw new ArgumentException("Latitude must be between -90 and 90 degrees", nameof(latitude));
            
            if (longitude < -180 || longitude > 180)
                throw new ArgumentException("Longitude must be between -180 and 180 degrees", nameof(longitude));
        }

        Latitude = latitude;
        Longitude = longitude;
    }

    /// <summary>
    /// Calculates the distance between two GPS coordinates using Haversine formula
    /// Returns distance in kilometers
    /// </summary>
    public double DistanceTo(GpsCoordinate other)
    {
        if (other == null)
            throw new ArgumentNullException(nameof(other));

        const double earthRadiusKm = 6371.0;
        
        var lat1Rad = ToRadians((double)Latitude);
        var lat2Rad = ToRadians((double)other.Latitude);
        var deltaLatRad = ToRadians((double)(other.Latitude - Latitude));
        var deltaLonRad = ToRadians((double)(other.Longitude - Longitude));

        var a = Math.Sin(deltaLatRad / 2) * Math.Sin(deltaLatRad / 2) +
                Math.Cos(lat1Rad) * Math.Cos(lat2Rad) *
                Math.Sin(deltaLonRad / 2) * Math.Sin(deltaLonRad / 2);
        
        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
        
        return earthRadiusKm * c;
    }

    private static double ToRadians(double degrees) => degrees * Math.PI / 180;

    public override string ToString() => $"{Latitude:F6}, {Longitude:F6}";

    public override bool Equals(object? obj) => obj is GpsCoordinate coordinate && Equals(coordinate);

    public override int GetHashCode() => HashCode.Combine(Latitude, Longitude);

    public bool Equals(GpsCoordinate? other) => other is not null &&
        Latitude == other.Latitude &&
        Longitude == other.Longitude;

    public static bool operator ==(GpsCoordinate? left, GpsCoordinate? right) => 
        left?.Equals(right) ?? right is null;

    public static bool operator !=(GpsCoordinate? left, GpsCoordinate? right) => 
        !(left == right);
}
