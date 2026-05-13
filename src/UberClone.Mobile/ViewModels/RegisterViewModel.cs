using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using UberClone.Mobile.Services;

namespace UberClone.Mobile.ViewModels;

public partial class RegisterViewModel : BaseViewModel
{
    private readonly IAuthService _auth;

    [ObservableProperty] private string fullName = "";
    [ObservableProperty] private string email = "";
    [ObservableProperty] private string phone = "";
    [ObservableProperty] private string password = "";

    public RegisterViewModel(IAuthService auth)
    {
        _auth = auth;
        Title = "Create your account";
    }

    [RelayCommand]
    private Task RegisterAsync() => SafeExecuteAsync(async () =>
    {
        await _auth.RegisterAsync(Email.Trim(), Password, FullName.Trim(), Phone.Trim());
        await Shell.Current.GoToAsync("//role-select");
    });
}
