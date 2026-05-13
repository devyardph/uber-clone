namespace UberClone.Shared.Dtos;

public enum RideStatus
{
    Requested,
    Assigned,
    DriverArriving,
    InProgress,
    Completed,
    Cancelled
}

public enum RideType
{
    Standard,
    Comfort,
    Xl
}

public record FareEstimate(decimal Amount, string Currency, double DistanceKm, int EtaMinutes);

public record RideRequest(
    GeoPoint Pickup,
    string PickupAddress,
    GeoPoint Dropoff,
    string DropoffAddress,
    RideType RideType);

public record RideDto(
    string Id,
    string RiderId,
    string? DriverId,
    GeoPoint Pickup,
    string PickupAddress,
    GeoPoint Dropoff,
    string DropoffAddress,
    RideType RideType,
    RideStatus Status,
    FareEstimate Fare,
    DateTime CreatedAtUtc,
    DateTime? CompletedAtUtc,
    GeoPoint? CurrentDriverLocation);

public record RideStatusUpdate(string RideId, RideStatus Status, DateTime AtUtc);
public record DriverLocationUpdate(string RideId, GeoPoint Location, double? HeadingDegrees, DateTime AtUtc);
