using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.Input;
using UberClone.Mobile.Services;
using UberClone.Shared.Dtos;

namespace UberClone.Mobile.ViewModels;

public partial class TripHistoryViewModel : BaseViewModel
{
    private readonly ITripService _trips;
    private readonly ISessionStore _session;

    public ObservableCollection<TripSummaryDto> Trips { get; } = new();

    public TripHistoryViewModel(ITripService trips, ISessionStore session)
    {
        _trips = trips;
        _session = session;
        Title = "Your trips";
    }

    [RelayCommand]
    public Task LoadAsync() => SafeExecuteAsync(async () =>
    {
        Trips.Clear();
        var data = await _trips.HistoryAsync(_session.ActiveRole);
        foreach (var t in data) Trips.Add(t);
    });
}
