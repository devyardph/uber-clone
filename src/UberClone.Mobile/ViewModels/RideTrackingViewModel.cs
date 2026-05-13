using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using UberClone.Mobile.Services;
using UberClone.Shared.Dtos;

namespace UberClone.Mobile.ViewModels;

[QueryProperty(nameof(RideId), "rideId")]
public partial class RideTrackingViewModel : BaseViewModel, IDisposable
{
    private readonly IRideService _rides;
    private readonly IRideHubClient _hub;

    [ObservableProperty] private string rideId = "";
    [ObservableProperty] private RideDto? ride;
    [ObservableProperty] private GeoPoint? driverLocation;
    [ObservableProperty] private string statusText = "Looking for a driver…";

    public RideTrackingViewModel(IRideService rides, IRideHubClient hub)
    {
        _rides = rides;
        _hub = hub;
        Title = "Your ride";

        _hub.RideStatusChanged += OnRideStatusChanged;
        _hub.DriverLocationUpdated += OnDriverLocationUpdated;
        _hub.RideCompleted += OnRideCompleted;
    }

    [RelayCommand]
    public Task AppearingAsync() => SafeExecuteAsync(async () =>
    {
        if (string.IsNullOrEmpty(RideId)) return;
        await _hub.ConnectAsync();
        await _hub.JoinRideAsync(RideId);
        Ride = await _rides.GetAsync(RideId);
        if (Ride?.CurrentDriverLocation is not null)
            DriverLocation = Ride.CurrentDriverLocation;
        StatusText = HumanStatus(Ride?.Status ?? RideStatus.Requested);
    });

    [RelayCommand]
    private Task CancelAsync() => SafeExecuteAsync(async () =>
    {
        if (string.IsNullOrEmpty(RideId)) return;
        await _rides.UpdateStatusAsync(RideId, RideStatus.Cancelled);
        await Shell.Current.GoToAsync("..");
    });

    private void OnRideStatusChanged(RideStatusUpdate u)
    {
        if (u.RideId != RideId) return;
        MainThread.BeginInvokeOnMainThread(() => StatusText = HumanStatus(u.Status));
    }

    private void OnDriverLocationUpdated(DriverLocationUpdate u)
    {
        if (u.RideId != RideId) return;
        MainThread.BeginInvokeOnMainThread(() => DriverLocation = u.Location);
    }

    private void OnRideCompleted(RideDto r)
    {
        if (r.Id != RideId) return;
        MainThread.BeginInvokeOnMainThread(() =>
        {
            StatusText = "Trip completed";
            Ride = r;
        });
    }

    private static string HumanStatus(RideStatus status) => status switch
    {
        RideStatus.Requested => "Looking for a driver…",
        RideStatus.Assigned => "Driver assigned, on the way",
        RideStatus.DriverArriving => "Driver is arriving",
        RideStatus.InProgress => "On your trip",
        RideStatus.Completed => "Trip completed",
        RideStatus.Cancelled => "Ride cancelled",
        _ => ""
    };

    public void Dispose()
    {
        _hub.RideStatusChanged -= OnRideStatusChanged;
        _hub.DriverLocationUpdated -= OnDriverLocationUpdated;
        _hub.RideCompleted -= OnRideCompleted;
        _ = _hub.LeaveRideAsync(RideId);
    }
}
