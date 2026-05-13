using Microsoft.AspNetCore.SignalR.Client;
using UberClone.Mobile.Models;
using UberClone.Shared.Dtos;
using UberClone.Shared.Hubs;

namespace UberClone.Mobile.Services;

public interface IRideHubClient
{
    Task ConnectAsync(CancellationToken ct = default);
    Task DisconnectAsync();
    Task JoinRideAsync(string rideId);
    Task LeaveRideAsync(string rideId);
    Task GoOnlineAsync(GeoPoint location);
    Task GoOfflineAsync();
    Task PublishDriverLocationAsync(DriverLocationUpdate update);

    event Action<RideDto>? RideAssigned;
    event Action<RideStatusUpdate>? RideStatusChanged;
    event Action<DriverLocationUpdate>? DriverLocationUpdated;
    event Action<RideDto>? RideCompleted;
}

public sealed class RideHubClient : IRideHubClient, IAsyncDisposable
{
    private readonly AppSettings _settings;
    private readonly ISessionStore _session;
    private HubConnection? _conn;

    public event Action<RideDto>? RideAssigned;
    public event Action<RideStatusUpdate>? RideStatusChanged;
    public event Action<DriverLocationUpdate>? DriverLocationUpdated;
    public event Action<RideDto>? RideCompleted;

    public RideHubClient(AppSettings settings, ISessionStore session)
    {
        _settings = settings;
        _session = session;
    }

    public async Task ConnectAsync(CancellationToken ct = default)
    {
        if (_conn is not null && _conn.State == HubConnectionState.Connected) return;

        _conn = new HubConnectionBuilder()
            .WithUrl($"{_settings.ApiBaseUrl}/hubs/ride", o =>
            {
                o.AccessTokenProvider = async () => await _session.GetTokenAsync();
            })
            .WithAutomaticReconnect()
            .Build();

        _conn.On<RideDto>(RideHubEvents.RideAssigned, ride => RideAssigned?.Invoke(ride));
        _conn.On<RideStatusUpdate>(RideHubEvents.RideStatusChanged, u => RideStatusChanged?.Invoke(u));
        _conn.On<DriverLocationUpdate>(RideHubEvents.DriverLocationUpdated, u => DriverLocationUpdated?.Invoke(u));
        _conn.On<RideDto>(RideHubEvents.RideCompleted, r => RideCompleted?.Invoke(r));

        await _conn.StartAsync(ct);
    }

    public async Task DisconnectAsync()
    {
        if (_conn is null) return;
        await _conn.StopAsync();
        await _conn.DisposeAsync();
        _conn = null;
    }

    public Task JoinRideAsync(string rideId) =>
        _conn?.InvokeAsync(RideHubInvoke.JoinRide, rideId) ?? Task.CompletedTask;

    public Task LeaveRideAsync(string rideId) =>
        _conn?.InvokeAsync(RideHubInvoke.LeaveRide, rideId) ?? Task.CompletedTask;

    public Task GoOnlineAsync(GeoPoint location) =>
        _conn?.InvokeAsync(RideHubInvoke.GoOnline, location) ?? Task.CompletedTask;

    public Task GoOfflineAsync() =>
        _conn?.InvokeAsync(RideHubInvoke.GoOffline) ?? Task.CompletedTask;

    public Task PublishDriverLocationAsync(DriverLocationUpdate update) =>
        _conn?.InvokeAsync(RideHubInvoke.PublishDriverLocation, update) ?? Task.CompletedTask;

    public async ValueTask DisposeAsync() => await DisconnectAsync();
}
