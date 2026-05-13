using UberClone.Shared.Dtos;

namespace UberClone.Mobile.Services;

public interface ILocationService
{
    Task<GeoPoint?> GetCurrentAsync();
    Task<bool> EnsurePermissionAsync();
    IObservable<GeoPoint> Track(TimeSpan interval);
}

public sealed class LocationService : ILocationService
{
    public async Task<bool> EnsurePermissionAsync()
    {
        var status = await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>();
        if (status != PermissionStatus.Granted)
            status = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();
        return status == PermissionStatus.Granted;
    }

    public async Task<GeoPoint?> GetCurrentAsync()
    {
        if (!await EnsurePermissionAsync()) return null;
        try
        {
            var loc = await Geolocation.Default.GetLocationAsync(new GeolocationRequest(GeolocationAccuracy.Best, TimeSpan.FromSeconds(8)));
            return loc is null ? null : new GeoPoint(loc.Latitude, loc.Longitude);
        }
        catch
        {
            return null;
        }
    }

    public IObservable<GeoPoint> Track(TimeSpan interval) =>
        new LocationStream(this, interval);

    private sealed class LocationStream : IObservable<GeoPoint>
    {
        private readonly LocationService _owner;
        private readonly TimeSpan _interval;

        public LocationStream(LocationService owner, TimeSpan interval)
        {
            _owner = owner;
            _interval = interval;
        }

        public IDisposable Subscribe(IObserver<GeoPoint> observer)
        {
            var cts = new CancellationTokenSource();
            _ = Task.Run(async () =>
            {
                while (!cts.IsCancellationRequested)
                {
                    var p = await _owner.GetCurrentAsync();
                    if (p is not null) observer.OnNext(p);
                    try { await Task.Delay(_interval, cts.Token); }
                    catch (TaskCanceledException) { break; }
                }
                observer.OnCompleted();
            }, cts.Token);
            return new Stop(cts);
        }

        private sealed class Stop : IDisposable
        {
            private readonly CancellationTokenSource _cts;
            public Stop(CancellationTokenSource cts) => _cts = cts;
            public void Dispose() { _cts.Cancel(); _cts.Dispose(); }
        }
    }
}
