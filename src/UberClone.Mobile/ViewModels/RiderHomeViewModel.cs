using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using UberClone.Mobile.Services;
using UberClone.Shared.Dtos;

namespace UberClone.Mobile.ViewModels;

public partial class RiderHomeViewModel : BaseViewModel
{
    private readonly IRideService _rides;
    private readonly ILocationService _location;

    [ObservableProperty] private GeoPoint? pickupLocation;
    [ObservableProperty] private string pickupAddress = "Current location";
    [ObservableProperty] private GeoPoint? dropoffLocation;
    [ObservableProperty] private string dropoffAddress = "";
    [ObservableProperty] private RideType selectedRideType = RideType.Standard;
    [ObservableProperty] private FareEstimate? estimate;

    public ObservableCollection<RideType> AvailableTypes { get; } =
        new() { RideType.Standard, RideType.Comfort, RideType.Xl };

    public RiderHomeViewModel(IRideService rides, ILocationService location)
    {
        _rides = rides;
        _location = location;
        Title = "Where to?";
    }

    [RelayCommand]
    public Task LoadAsync() => SafeExecuteAsync(async () =>
    {
        PickupLocation = await _location.GetCurrentAsync();
    });

    [RelayCommand]
    private Task EstimateAsync() => SafeExecuteAsync(async () =>
    {
        if (PickupLocation is null || DropoffLocation is null) return;
        Estimate = await _rides.EstimateAsync(new RideRequest(
            PickupLocation, PickupAddress, DropoffLocation, DropoffAddress, SelectedRideType));
    });

    [RelayCommand]
    private Task ConfirmAsync() => SafeExecuteAsync(async () =>
    {
        if (PickupLocation is null || DropoffLocation is null) return;
        var ride = await _rides.RequestAsync(new RideRequest(
            PickupLocation, PickupAddress, DropoffLocation, DropoffAddress, SelectedRideType));
        await Shell.Current.GoToAsync($"ride-tracking?rideId={ride.Id}");
    });

    [RelayCommand]
    private Task GoToHistoryAsync() => Shell.Current.GoToAsync("trip-history");

    [RelayCommand]
    private Task GoToPaymentsAsync() => Shell.Current.GoToAsync("payments");
}
