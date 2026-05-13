using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using UberClone.Mobile.Services;

namespace UberClone.Mobile.ViewModels;

public partial class LoginViewModel : BaseViewModel
{
    private readonly IAuthService _auth;

    [ObservableProperty] private string email = "rider@demo.com";
    [ObservableProperty] private string password = "Password1!";

    public LoginViewModel(IAuthService auth)
    {
        _auth = auth;
        Title = "Welcome back";
    }

    [RelayCommand]
    private Task LoginAsync() => SafeExecuteAsync(async () =>
    {
        await _auth.LoginAsync(Email.Trim(), Password);
        await Shell.Current.GoToAsync("role-select");
    });

    [RelayCommand]
    private Task GoToRegisterAsync() =>
        Shell.Current.GoToAsync("register");
}
