using CommunityToolkit.Mvvm.Input;
using UberClone.Mobile.Services;
using UberClone.Shared.Dtos;

namespace UberClone.Mobile.ViewModels;

public partial class RoleSelectViewModel : BaseViewModel
{
    private readonly ISessionStore _session;
    private readonly IAuthService _auth;
    private readonly IRideHubClient _hub;

    public string Greeting => $"Hi {_session.CurrentUser?.FullName ?? "there"} — how would you like to use the app?";

    public RoleSelectViewModel(ISessionStore session, IAuthService auth, IRideHubClient hub)
    {
        _session = session;
        _auth = auth;
        _hub = hub;
        Title = "Choose mode";
    }

    [RelayCommand]
    private Task RideAsync() => SafeExecuteAsync(async () =>
    {
        _session.ActiveRole = UserRole.Rider;
        await _hub.ConnectAsync();
        await Shell.Current.GoToAsync("//rider-home");
    });

    [RelayCommand]
    private Task DriveAsync() => SafeExecuteAsync(async () =>
    {
        _session.ActiveRole = UserRole.Driver;
        await _hub.ConnectAsync();
        await Shell.Current.GoToAsync("//driver-home");
    });

    [RelayCommand]
    private Task LogoutAsync() => SafeExecuteAsync(async () =>
    {
        await _auth.LogoutAsync();
        await Shell.Current.GoToAsync("//login");
    });
}
