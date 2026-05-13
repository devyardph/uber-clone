using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using UberClone.Mobile.Services;
using UberClone.Shared.Dtos;

namespace UberClone.Mobile.ViewModels;

public partial class DriverHomeViewModel : BaseViewModel, IDisposable
{
    private readonly IRideHubClient _hub;
    private readonly ILocationService _location;
    private IDisposable? _trackingSub;

    [ObservableProperty] private bool isOnline;
    [ObservableProperty] private GeoPoint? currentLocation;
    [ObservableProperty] private string statusText = "Offline — go online to receive trips";
    [ObservableProperty] private RideDto? incomingRide;

    public DriverHomeViewModel(IRideHubClient hub, ILocationService location)
    {
        _hub = hub;
        _location = location;
        Title = "Driving";

        _hub.RideAssigned += OnRideAssigned;
    }

    [RelayCommand]
    public Task AppearingAsync() => SafeExecuteAsync(async () =>
    {
        await _hub.ConnectAsync();
        CurrentLocation = await _location.GetCurrentAsync();
    });

    [RelayCommand]
    private Task ToggleOnlineAsync() => SafeExecuteAsync(async () =>
    {
        if (!IsOnline)
        {
            CurrentLocation ??= await _location.GetCurrentAsync();
            if (CurrentLocation is null) { ErrorMessage = "Location unavailable"; return; }
            await _hub.GoOnlineAsync(CurrentLocation);
            _trackingSub = _location.Track(TimeSpan.FromSeconds(5)).Subscribe(p =>
            {
                CurrentLocation = p;
            });
            IsOnline = true;
            StatusText = "Online — waiting for trips";
        }
        else
        {
            _trackingSub?.Dispose();
            _trackingSub = null;
            await _hub.GoOfflineAsync();
            IsOnline = false;
            StatusText = "Offline";
        }
    });

    [RelayCommand]
    private Task AcceptRideAsync() => SafeExecuteAsync(async () =>
    {
        if (IncomingRide is null) return;
        await Shell.Current.GoToAsync($"driver-active-trip?rideId={IncomingRide.Id}");
        IncomingRide = null;
    });

    [RelayCommand]
    private void Decline() => IncomingRide = null;

    private void OnRideAssigned(RideDto ride) =>
        MainThread.BeginInvokeOnMainThread(() =>
        {
            IncomingRide = ride;
            StatusText = $"New trip from {ride.PickupAddress}";
        });

    public void Dispose()
    {
        _hub.RideAssigned -= OnRideAssigned;
        _trackingSub?.Dispose();
    }
}
