using UberClone.Shared.Dtos;

namespace UberClone.Api.Services;

public interface IFareCalculator
{
    FareEstimate Estimate(GeoPoint pickup, GeoPoint dropoff, RideType type);
}

public sealed class FareCalculator : IFareCalculator
{
    public FareEstimate Estimate(GeoPoint pickup, GeoPoint dropoff, RideType type)
    {
        var km = pickup.DistanceKmTo(dropoff);
        var (baseFare, perKm) = type switch
        {
            RideType.Comfort => (3.50m, 1.85m),
            RideType.Xl => (5.00m, 2.40m),
            _ => (2.50m, 1.40m),
        };
        var amount = Math.Round(baseFare + (decimal)km * perKm, 2);
        var eta = (int)Math.Ceiling(km / 0.5); // ~30 km/h average urban
        return new FareEstimate(amount, "USD", Math.Round(km, 2), Math.Max(eta, 3));
    }
}
