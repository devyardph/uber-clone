namespace UberClone.Shared.Dtos;

public record GeoPoint(double Latitude, double Longitude)
{
    /// <summary>Great-circle distance in km using Haversine.</summary>
    public double DistanceKmTo(GeoPoint other)
    {
        const double R = 6371.0;
        var dLat = ToRad(other.Latitude - Latitude);
        var dLon = ToRad(other.Longitude - Longitude);
        var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                Math.Cos(ToRad(Latitude)) * Math.Cos(ToRad(other.Latitude)) *
                Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
        return R * c;
    }

    private static double ToRad(double deg) => deg * Math.PI / 180.0;
}
