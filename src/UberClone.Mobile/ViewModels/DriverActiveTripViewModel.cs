using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using UberClone.Mobile.Services;
using UberClone.Shared.Dtos;

namespace UberClone.Mobile.ViewModels;

[QueryProperty(nameof(RideId), "rideId")]
public partial class DriverActiveTripViewModel : BaseViewModel, IDisposable
{
    private readonly IRideService _rides;
    private readonly IRideHubClient _hub;
    private readonly ILocationService _location;
    private IDisposable? _locationSub;

    [ObservableProperty] private string rideId = "";
    [ObservableProperty] private RideDto? ride;
    [ObservableProperty] private RideStatus currentStatus = RideStatus.Assigned;
    [ObservableProperty] private string nextActionLabel = "Arrived at pickup";

    public DriverActiveTripViewModel(IRideService rides, IRideHubClient hub, ILocationService location)
    {
        _rides = rides;
        _hub = hub;
        _location = location;
        Title = "Active trip";
    }

    [RelayCommand]
    public Task AppearingAsync() => SafeExecuteAsync(async () =>
    {
        await _hub.ConnectAsync();
        await _hub.JoinRideAsync(RideId);
        Ride = await _rides.GetAsync(RideId);

        // Stream driver position to the rider every 3 seconds.
        _locationSub = _location.Track(TimeSpan.FromSeconds(3)).Subscribe(async loc =>
        {
            await _hub.PublishDriverLocationAsync(new DriverLocationUpdate(
                RideId, loc, HeadingDegrees: null, AtUtc: DateTime.UtcNow));
        });
    });

    [RelayCommand]
    private Task AdvanceAsync() => SafeExecuteAsync(async () =>
    {
        var next = CurrentStatus switch
        {
            RideStatus.Assigned => RideStatus.DriverArriving,
            RideStatus.DriverArriving => RideStatus.InProgress,
            RideStatus.InProgress => RideStatus.Completed,
            _ => RideStatus.Completed
        };
        Ride = await _rides.UpdateStatusAsync(RideId, next);
        CurrentStatus = next;
        NextActionLabel = next switch
        {
            RideStatus.Assigned => "Arrived at pickup",
            RideStatus.DriverArriving => "Start trip",
            RideStatus.InProgress => "Complete trip",
            _ => "Done"
        };

        if (next == RideStatus.Completed)
        {
            _locationSub?.Dispose();
            await _hub.LeaveRideAsync(RideId);
            await Shell.Current.GoToAsync("..");
        }
    });

    public void Dispose() => _locationSub?.Dispose();
}
