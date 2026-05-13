using System.Collections.Concurrent;
using System.Security.Cryptography;
using System.Text;
using UberClone.Shared.Dtos;

namespace UberClone.Api.Services;

public record StoredUser(string Id, string Email, string FullName, string PhoneNumber, string PasswordHash, UserRole DefaultRole);

public interface IUserStore
{
    StoredUser? FindByEmail(string email);
    StoredUser? FindById(string id);
    StoredUser Create(string email, string password, string fullName, string phoneNumber, UserRole defaultRole);
    void SeedDemo();
}

public sealed class InMemoryUserStore : IUserStore
{
    private readonly ConcurrentDictionary<string, StoredUser> _byId = new();

    public StoredUser? FindByEmail(string email) =>
        _byId.Values.FirstOrDefault(u => string.Equals(u.Email, email, StringComparison.OrdinalIgnoreCase));

    public StoredUser? FindById(string id) => _byId.TryGetValue(id, out var u) ? u : null;

    public StoredUser Create(string email, string password, string fullName, string phoneNumber, UserRole defaultRole)
    {
        var user = new StoredUser(
            Guid.NewGuid().ToString("N"),
            email.Trim().ToLowerInvariant(),
            fullName,
            phoneNumber,
            Hash(password),
            defaultRole);
        _byId[user.Id] = user;
        return user;
    }

    public void SeedDemo()
    {
        if (_byId.IsEmpty is false) return;
        Create("rider@demo.com", "Password1!", "Rita Rider", "+15555550100", UserRole.Rider);
        Create("driver@demo.com", "Password1!", "Dan Driver", "+15555550101", UserRole.Driver);
    }

    public static string Hash(string password)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(password));
        return Convert.ToHexString(bytes);
    }
}

public interface IRideStore
{
    RideDto Create(RideDto ride);
    RideDto? Get(string id);
    RideDto Update(RideDto ride);
    IEnumerable<RideDto> ForRider(string riderId);
    IEnumerable<RideDto> ForDriver(string driverId);
}

public sealed class InMemoryRideStore : IRideStore
{
    private readonly ConcurrentDictionary<string, RideDto> _rides = new();

    public RideDto Create(RideDto ride) { _rides[ride.Id] = ride; return ride; }
    public RideDto? Get(string id) => _rides.TryGetValue(id, out var r) ? r : null;
    public RideDto Update(RideDto ride) { _rides[ride.Id] = ride; return ride; }
    public IEnumerable<RideDto> ForRider(string riderId) => _rides.Values.Where(r => r.RiderId == riderId);
    public IEnumerable<RideDto> ForDriver(string driverId) => _rides.Values.Where(r => r.DriverId == driverId);
}

public interface IPaymentStore
{
    IReadOnlyList<PaymentMethodDto> ForUser(string userId);
    PaymentMethodDto Add(string userId, AddPaymentMethodRequest req);
    bool Remove(string userId, string methodId);
}

public sealed class InMemoryPaymentStore : IPaymentStore
{
    private readonly ConcurrentDictionary<string, List<PaymentMethodDto>> _byUser = new();
    private readonly object _gate = new();

    public IReadOnlyList<PaymentMethodDto> ForUser(string userId) =>
        _byUser.TryGetValue(userId, out var list) ? list.ToList() : Array.Empty<PaymentMethodDto>();

    public PaymentMethodDto Add(string userId, AddPaymentMethodRequest req)
    {
        lock (_gate)
        {
            var list = _byUser.GetOrAdd(userId, _ => new List<PaymentMethodDto>());
            var method = new PaymentMethodDto(Guid.NewGuid().ToString("N"), req.Brand, req.Last4, list.Count == 0);
            list.Add(method);
            return method;
        }
    }

    public bool Remove(string userId, string methodId)
    {
        lock (_gate)
        {
            if (!_byUser.TryGetValue(userId, out var list)) return false;
            return list.RemoveAll(p => p.Id == methodId) > 0;
        }
    }
}

public interface IDriverPresence
{
    void GoOnline(string driverId, string connectionId, GeoPoint location);
    void GoOffline(string driverId);
    void UpdateLocation(string driverId, GeoPoint location);
    string? ConnectionFor(string driverId);
    IReadOnlyList<(string DriverId, GeoPoint Location, string ConnectionId)> Online();
}

public sealed class DriverPresence : IDriverPresence
{
    private record Entry(string DriverId, GeoPoint Location, string ConnectionId);
    private readonly ConcurrentDictionary<string, Entry> _byDriver = new();

    public void GoOnline(string driverId, string connectionId, GeoPoint location) =>
        _byDriver[driverId] = new Entry(driverId, location, connectionId);

    public void GoOffline(string driverId) => _byDriver.TryRemove(driverId, out _);

    public void UpdateLocation(string driverId, GeoPoint location)
    {
        if (_byDriver.TryGetValue(driverId, out var entry))
            _byDriver[driverId] = entry with { Location = location };
    }

    public string? ConnectionFor(string driverId) =>
        _byDriver.TryGetValue(driverId, out var entry) ? entry.ConnectionId : null;

    public IReadOnlyList<(string, GeoPoint, string)> Online() =>
        _byDriver.Values.Select(e => (e.DriverId, e.Location, e.ConnectionId)).ToList();
}
