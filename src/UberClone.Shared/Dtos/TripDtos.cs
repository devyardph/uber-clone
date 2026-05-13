namespace UberClone.Shared.Dtos;

public record TripSummaryDto(
    string Id,
    string PickupAddress,
    string DropoffAddress,
    decimal Amount,
    string Currency,
    DateTime CompletedAtUtc,
    string DriverName,
    double DistanceKm,
    int DurationMinutes);

public record PaymentMethodDto(string Id, string Brand, string Last4, bool IsDefault);
public record AddPaymentMethodRequest(string Brand, string Last4);
