using UberClone.Shared.Dtos;

namespace UberClone.Api.Services;

public interface IRideMatchingService
{
    /// <summary>Returns nearest online driver within radiusKm, or null.</summary>
    (string DriverId, string ConnectionId, GeoPoint Location)? FindNearestDriver(GeoPoint pickup, double radiusKm = 8.0);
}

public sealed class RideMatchingService : IRideMatchingService
{
    private readonly IDriverPresence _presence;
    public RideMatchingService(IDriverPresence presence) => _presence = presence;

    public (string, string, GeoPoint)? FindNearestDriver(GeoPoint pickup, double radiusKm = 8.0)
    {
        var best = _presence.Online()
            .Select(d => (d.DriverId, d.ConnectionId, d.Location, Dist: pickup.DistanceKmTo(d.Location)))
            .Where(d => d.Dist <= radiusKm)
            .OrderBy(d => d.Dist)
            .FirstOrDefault();
        return best == default ? null : (best.DriverId, best.ConnectionId, best.Location);
    }
}
